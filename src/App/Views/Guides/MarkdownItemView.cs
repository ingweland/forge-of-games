using System.Text;
using Ingweland.Fog.Application.Client.Web.Providers.Interfaces;
using Ingweland.Fog.Application.Core.Extensions;
using Ingweland.Fog.Models.Fog.Entities;
using Ingweland.Fog.Models.Hoh.Enums;

namespace Ingweland.Fog.App.Views.Guides;

/// <summary>
///     A timeline item whose content is markdown: the white, bordered, scrolling card that is the whole of
///     DescriptionTimelineItemViewerComponent and IntroTimelineItemViewerComponent. The one difference between
///     those two is the header the intro puts in front of its text, so it is the one difference here too.
/// </summary>
internal sealed class MarkdownItemView : ContentView
{
    private MarkdownItemView(string markdown)
    {
        Content = new Border
        {
            Style = ThemeResources.Style("FogGuideContent"),
            Content = new ScrollView {Content = new MarkdownView(markdown)},
        };
    }

    public static MarkdownItemView ForDescription(CityStrategyDescriptionTimelineItem item)
    {
        return new MarkdownItemView(item.Description);
    }

    public static MarkdownItemView ForIntro(CityStrategyIntroTimelineItem item, IAssetUrlProvider assetUrlProvider)
    {
        return new MarkdownItemView(IntroMarkdown(item, assetUrlProvider));
    }

    /// <summary>
    ///     IntroTimelineItemViewerComponent.PrependHeader, unchanged: the wonder's banner, the item's title as a
    ///     heading, and a rule. Writing markdown and parsing it back is what the web does, and it keeps the
    ///     header on the same rendering path as the text under it - the banner is sized by its own alt text.
    /// </summary>
    private static string IntroMarkdown(CityStrategyIntroTimelineItem item, IAssetUrlProvider assetUrlProvider)
    {
        if (item.WonderId == WonderId.Undefined)
        {
            return item.Description;
        }

        var sb = new StringBuilder();
        sb.AppendLine($"![header]({assetUrlProvider.GetHohImageUrl(item.WonderId.GetImageFileName())})");
        sb.AppendLine();
        sb.AppendLine($"# {item.Title}");
        sb.AppendLine();
        sb.AppendLine("---");
        sb.AppendLine();
        sb.Append(item.Description);
        return sb.ToString();
    }
}
