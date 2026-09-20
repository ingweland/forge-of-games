using Ingweland.Fog.App.Views.CityViewer;
using Markdig;
using Markdig.Extensions.Tables;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Shapes;

namespace Ingweland.Fog.App.Views.Guides;

/// <summary>
///     Guide markdown as native views: the app's answer to the web's <c>.markdown-city-guide</c> div. It is
///     parsed with the same pipeline <c>MarkdownSecurityService</c> builds, but walked as a Markdig syntax tree
///     rather than turned into HTML, so the whitelist that service applies afterwards has to live in the walk
///     instead. That is what <see cref="SafeUrl" /> is: the sanitizer keeps https and nothing else.
/// </summary>
internal sealed class MarkdownView : ContentView
{
    private const double BODY_FONT_SIZE = 14;

    // The gap the web gets from `h1..h3 { margin: 12px 0 }` collapsing with the paragraphs around it.
    private const double BLOCK_SPACING = 12;

    private static readonly InlineStyle BodyStyle = new(BODY_FONT_SIZE);

    // The two calls MarkdownSecurityService makes. DisableHtml leaves raw HTML in a guide as inert text, which
    // is what the sanitizer's tag whitelist does with it on the web.
    private static readonly MarkdownPipeline Pipeline =
        new MarkdownPipelineBuilder().DisableHtml().UseAdvancedExtensions().Build();

    public MarkdownView(string markdown)
    {
        Padding = 12;
        Content = BuildBlocks(Markdown.Parse(markdown, Pipeline));
    }

    private static VerticalStackLayout BuildBlocks(ContainerBlock container, double spacing = BLOCK_SPACING)
    {
        var stack = new VerticalStackLayout {Spacing = spacing};
        foreach (var block in container)
        {
            switch (block)
            {
                case HeadingBlock heading:
                    stack.Add(BuildHeading(heading));
                    break;
                case ParagraphBlock paragraph:
                    EmitParagraph(stack, paragraph.Inline);
                    break;
                case ThematicBreakBlock:
                    stack.Add(BuildRule());
                    break;
                case ListBlock list:
                    stack.Add(BuildList(list));
                    break;
                case QuoteBlock quote:
                    stack.Add(BuildQuote(quote));
                    break;
                case Table table:
                    stack.Add(BuildTable(table));
                    break;
                case CodeBlock code:
                    stack.Add(BuildCode(code));
                    break;
                case HtmlBlock html:
                    // Out of reach while the pipeline disables HTML, but the text is still the author's.
                    stack.Add(BuildText(LinesOf(html), BodyStyle));
                    break;
            }
        }

        return stack;
    }

    private static Label BuildHeading(HeadingBlock heading)
    {
        // A gentler ramp than the browser's, which would put h5 and h6 below the body size.
        var size = heading.Level switch
        {
            1 => 24d,
            2 => 20d,
            3 => 17d,
            4 => 15d,
            5 => 14d,
            _ => 13d,
        };

        var label = BuildLabel(heading.Inline, new InlineStyle(size, true));
        if (heading.Level == 1)
        {
            // .markdown-city-guide h1 { text-align: center }
            label.HorizontalTextAlignment = TextAlignment.Center;
        }

        return label;
    }

    private static View BuildRule()
    {
        var rule = new ContentView {BackgroundColor = ThemeResources.Color("FogBorderColor")};
        rule.SetDynamicResource(HeightRequestProperty, "FogHairlineThickness");
        return rule;
    }

    private static View BuildList(ListBlock list)
    {
        var stack = new VerticalStackLayout {Spacing = 6};
        foreach (var block in list)
        {
            if (block is not ListItemBlock item)
            {
                continue;
            }

            var row = new Grid {ColumnSpacing = 6};
            row.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
            row.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            row.Add(new Label
            {
                Text = list.IsOrdered ? $"{item.Order}." : "•",
                Style = ThemeResources.Style("FogTextLabel"),
            });
            // The marker column is the indent, so a nested list needs nothing of its own. A tight list is the
            // one the web writes without a paragraph inside each item, so its blocks sit closer together.
            row.Add(BuildBlocks(item, list.IsLoose ? BLOCK_SPACING : 4), 1);
            stack.Add(row);
        }

        return stack;
    }

    private static View BuildQuote(QuoteBlock quote)
    {
        var bar = new ContentView {WidthRequest = 3, BackgroundColor = ThemeResources.Color("FogContainerColor")};
        var grid = new Grid {ColumnSpacing = 8};
        grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        grid.Add(bar);
        grid.Add(BuildBlocks(quote), 1);
        return grid;
    }

    private static View BuildCode(CodeBlock code)
    {
        return new Border
        {
            Style = ThemeResources.Style("FogCodeBlock"),
            Content = BuildText(LinesOf(code), BodyStyle),
        };
    }

    private static View BuildTable(Table table)
    {
        var rows = table.OfType<TableRow>().ToList();
        if (rows.Count == 0)
        {
            return new VerticalStackLayout();
        }

        // A pipe table always opens with its header row, which is the one StatsTable tints.
        var header = CellsOf(rows[0], true);
        var body = rows.Skip(1).Select(row => Limit(CellsOf(row, false), header.Count));

        var grid = new Grid();
        StatsTable.Fill(grid, header, body, new Thickness(6, 4));
        return grid;
    }

    private static IReadOnlyList<View?> CellsOf(TableRow row, bool isHeader)
    {
        var style = new InlineStyle(BODY_FONT_SIZE, isHeader);
        return row.OfType<TableCell>()
            .Select(cell => (View?) BuildLabel((cell.Count > 0 ? cell[0] as LeafBlock : null)?.Inline, style))
            .ToList();
    }

    // A malformed table can carry more cells in a row than its header declares columns, and the grid has no
    // column for those.
    private static IReadOnlyList<View?> Limit(IReadOnlyList<View?> cells, int columns)
    {
        return cells.Count <= columns ? cells : cells.Take(columns).ToList();
    }

    private static Label BuildLabel(ContainerInline? inlines, InlineStyle style)
    {
        var formatted = new FormattedString();
        AppendInlines(inlines, new InlineWriter(formatted), style);
        return new Label {FormattedText = formatted};
    }

    private static Label BuildText(string text, InlineStyle style)
    {
        var formatted = new FormattedString();
        formatted.Spans.Add(CreateSpan(text, style));
        return new Label {FormattedText = formatted};
    }

    private static void EmitParagraph(VerticalStackLayout stack, ContainerInline? inlines)
    {
        var writer = new InlineWriter(stack);
        AppendInlines(inlines, writer, BodyStyle);
        writer.Flush();
    }

    private static void AppendInlines(ContainerInline? container, InlineWriter writer, InlineStyle style)
    {
        if (container == null)
        {
            return;
        }

        foreach (var inline in container)
        {
            switch (inline)
            {
                case LiteralInline literal:
                {
                    writer.Add(CreateSpan(literal.Content.ToString(), style));
                    break;
                }

                case EmphasisInline emphasis:
                {
                    AppendInlines(emphasis, writer, Emphasize(style, emphasis));
                    break;
                }

                case CodeInline code:
                {
                    var span = CreateSpan(code.Content, style);
                    span.BackgroundColor = ThemeResources.Color("FogZebraColor");
                    writer.Add(span);
                    break;
                }

                case LinkInline {IsImage: true} image:
                {
                    var view = CreateImage(image);
                    if (view != null)
                    {
                        writer.Add(view);
                    }

                    break;
                }

                case LinkInline link:
                {
                    AppendInlines(link, writer, style with {LinkUrl = SafeUrl(link.Url)});
                    break;
                }

                case AutolinkInline autolink:
                {
                    writer.Add(CreateSpan(autolink.Url, style with {LinkUrl = SafeUrl(autolink.Url)}));
                    break;
                }

                case LineBreakInline lineBreak:
                {
                    writer.Add(CreateSpan(lineBreak.IsHard ? "\n" : " ", style));
                    break;
                }

                case HtmlInline html:
                {
                    writer.Add(CreateSpan(html.Tag, style));
                    break;
                }

                case HtmlEntityInline entity:
                {
                    writer.Add(CreateSpan(entity.Transcoded.ToString(), style));
                    break;
                }

                case ContainerInline nested:
                {
                    AppendInlines(nested, writer, style);
                    break;
                }

                case LeafInline leaf:
                {
                    writer.Add(CreateSpan(leaf.ToString() ?? string.Empty, style));
                    break;
                }
            }
        }
    }

    // CommonMark's own emphasis, plus the strikethrough the advanced extensions add. Any other delimiter those
    // extensions produce - superscript, subscript - keeps its text and loses only its styling.
    private static InlineStyle Emphasize(InlineStyle style, EmphasisInline emphasis)
    {
        return emphasis.DelimiterChar switch
        {
            '*' or '_' => emphasis.DelimiterCount switch
            {
                1 => style with {Italic = true},
                2 => style with {Bold = true},
                _ => style with {Bold = true, Italic = true},
            },
            '~' when emphasis.DelimiterCount >= 2 => style with {Strike = true},
            _ => style,
        };
    }

    private static Span CreateSpan(string text, InlineStyle style)
    {
        var isLink = style.LinkUrl != null;
        var span = new Span
        {
            Text = text,
            // A span takes neither size nor colour from the label holding it, so every one carries its own.
            FontSize = style.FontSize,
            FontFamily = style.Bold ? "OpenSansSemibold" : "OpenSansRegular",
            FontAttributes = style.Italic ? FontAttributes.Italic : FontAttributes.None,
            TextColor = ThemeResources.Color(isLink ? "FogPrimaryColor" : "FogTextColor"),
            TextDecorations = (style.Strike ? TextDecorations.Strikethrough : TextDecorations.None) |
                (isLink ? TextDecorations.Underline : TextDecorations.None),
        };

        if (style.LinkUrl is { } url)
        {
            var tap = new TapGestureRecognizer();
            tap.Tapped += (_, _) => _ = OpenAsync(url);
            span.GestureRecognizers.Add(tap);
        }

        return span;
    }

    private static View? CreateImage(LinkInline image)
    {
        var url = SafeUrl(image.Url);
        if (url == null)
        {
            return null;
        }

        var view = new AssetImage
        {
            Url = url,
            Aspect = Aspect.AspectFit,
            HorizontalOptions = LayoutOptions.Center,
        };

        // The three img rules of .markdown-city-guide. Their max-width is what the stack already gives them.
        var alt = AltTextOf(image);
        if (alt == "header")
        {
            view.HeightRequest = 250;
            return new Border
            {
                Content = view,
                Stroke = Colors.Transparent,
                StrokeThickness = 0,
                Padding = 0,
                StrokeShape = new RoundRectangle {CornerRadius = 20},
                HorizontalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 0, 0, 12),
            };
        }

        if (alt.StartsWith("layout", StringComparison.Ordinal))
        {
            view.HeightRequest = 400;
            view.Margin = new Thickness(0, 12);
        }

        return view;
    }

    private static string AltTextOf(LinkInline image)
    {
        return string.Concat(image.Descendants<LiteralInline>().Select(literal => literal.Content.ToString()));
    }

    // What the web sanitizer's AllowedSchemes comes to: an https link or image is followed, anything else is
    // left as the text it is and loads nothing.
    private static string? SafeUrl(string? url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps ? url : null;
    }

    private static async Task OpenAsync(string url)
    {
        try
        {
            // In a browser tab over the app, rather than handing the reader to another one.
            await Browser.Default.OpenAsync(url, BrowserLaunchMode.SystemPreferred);
        }
        catch (Exception e)
        {
            IPlatformApplication.Current?.Services.GetService<ILogger<MarkdownView>>()
                ?.LogWarning(e, "Could not open the link {Url}", url);
        }
    }

    // The backing array is longer than the content, so the count has to bound it.
    private static string LinesOf(LeafBlock block)
    {
        var group = block.Lines;
        return group.Lines == null
            ? string.Empty
            : string.Join("\n", group.Lines.Take(group.Count).Select(line => line.ToString()));
    }

    private readonly record struct InlineStyle(
        double FontSize,
        bool Bold = false,
        bool Italic = false,
        bool Strike = false,
        string? LinkUrl = null);

    /// <summary>
    ///     Collects spans into one label at a time. A MAUI span cannot hold a view, so an image closes the label
    ///     it appears in, goes into the stack on its own, and the spans after it start a new one. Guides put
    ///     their images on a line of their own, which is the case this renders exactly as the web does.
    /// </summary>
    /// <remarks>
    ///     A writer made over a <see cref="FormattedString" /> fills that one label and has nowhere to put an
    ///     image, which is the right answer inside a heading or a table cell.
    /// </remarks>
    private sealed class InlineWriter
    {
        private readonly VerticalStackLayout? _stack;
        private FormattedString _formatted;

        public InlineWriter(FormattedString formatted)
        {
            _formatted = formatted;
        }

        public InlineWriter(VerticalStackLayout stack)
        {
            _stack = stack;
            _formatted = new FormattedString();
        }

        public void Add(Span span)
        {
            _formatted.Spans.Add(span);
        }

        public void Add(View view)
        {
            if (_stack == null)
            {
                return;
            }

            Flush();
            _stack.Add(view);
        }

        public void Flush()
        {
            if (_stack == null || _formatted.Spans.Count == 0)
            {
                return;
            }

            _stack.Add(new Label {FormattedText = _formatted});
            _formatted = new FormattedString();
        }
    }
}
