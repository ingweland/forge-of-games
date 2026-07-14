using System.Diagnostics.CodeAnalysis;
using System.Net.Mime;
using Ingweland.Fog.Application.Client.Core.Localization;
using Ingweland.Fog.Application.Client.Web.CityPlanner;
using Ingweland.Fog.Application.Client.Web.CityPlanner.Abstractions;
using Ingweland.Fog.Application.Client.Web.CityStrategyBuilder;
using Ingweland.Fog.Application.Client.Web.CityStrategyBuilder.Abstractions;
using Ingweland.Fog.Application.Client.Web.Services.Abstractions;
using Ingweland.Fog.Application.Core.Helpers;
using Ingweland.Fog.Models.Fog.Entities;
using Ingweland.Fog.Models.Hoh.Enums;
using Ingweland.Fog.Shared.Constants;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Localization;
using MudBlazor;
using SkiaSharp.Views.Blazor;
using Size = System.Drawing.Size;

namespace Ingweland.Fog.WebApp.Client.Components.Elements.CityStrategyBuilder;

[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
public partial class CityStrategyBuilderComponent : ComponentBase, IDisposable
{
    private Size _canvasSize = Size.Empty;
    private bool _extendedExpansionModeIsActive;
    private bool _fitOnPaint = true;
    private bool _isInitialized;
    private bool _leftPanelIsVisible = true;
    private bool _rightPanelIsVisible = true;
    private SKGLViewComponent? _skComponent;

    [Inject]
    private ICityPlannerInteractionManager CityPlannerInteractionManager { get; set; }

    [Inject]
    private CityPlannerSettings CityPlannerSettings { get; set; }

    [Inject]
    private ICityStrategyBuilderService CityStrategyBuilderService { get; set; }

    [Inject]
    private IDialogService DialogService { get; set; }

    [Inject]
    public IFogSharingUiService FogSharingUiService { get; set; }

    [Inject]
    public IJSInteropService JsInteropService { get; set; }

    [Inject]
    private IStringLocalizer<FogResource> Loc { get; set; }

    [Inject]
    private ILogger<CityStrategyBuilderComponent> Logger { get; set; }

    [Inject]
    private NavigationManager NavigationManager { get; set; }

    public void Dispose()
    {
        _skComponent?.SkCanvasView?.Dispose();
        CityStrategyBuilderService.StateHasChanged -= CityPlannerOnStateHasHasChanged;
        CityPlannerSettings.StateChanged -= CityPlannerSettingsOnStateChanged;
        CityStrategyBuilderService.Dispose();
        Logger.LogDebug("Disposing CityStrategyComponent");
    }

    protected override async Task OnInitializedAsync()
    {
        if (!OperatingSystem.IsBrowser())
        {
            return;
        }

        await CityStrategyBuilderService.InitializeAsync(Strategy);

        CityPlannerSettings.StateChanged += CityPlannerSettingsOnStateChanged;
        CityStrategyBuilderService.StateHasChanged += CityPlannerOnStateHasHasChanged;
        _isInitialized = true;
    }

    private void BuildingSelectorOnItemClicked(BuildingGroup buildingGroup)
    {
        if (_skComponent?.SkCanvasView == null)
        {
            return;
        }

        CityStrategyBuilderService.AddNewCityMapEntity(buildingGroup);
        _skComponent?.SkCanvasView.Invalidate();
    }

    private void CityPlannerOnStateHasHasChanged()
    {
        if (_skComponent?.SkCanvasView == null)
        {
            return;
        }

        _skComponent?.SkCanvasView.Invalidate();
        StateHasChanged();
    }

    private void CityPlannerSettingsOnStateChanged()
    {
        _skComponent?.SkCanvasView?.Invalidate();
    }

    private void DeleteCityMapEntity()
    {
        if (_skComponent?.SkCanvasView == null)
        {
            return;
        }

        if (CityStrategyBuilderService.DeleteSelectedCityMapEntity())
        {
            _skComponent?.SkCanvasView.Invalidate();
        }
    }

    private async Task ShareStrategy()
    {
        await CityStrategyBuilderService.Save();
        var data = FogSharingUiService.CreateSharedData(CityStrategyBuilderService.Strategy);
        var parameters = new DialogParameters<ShareResourceDialog>
        {
            {d => d.Data, data},
            {
                d => d.BaseUrl,
                $"{NavigationManager.BaseUri.TrimEnd('/')}{FogUrlBuilder.PageRoutes.GET_SHARED_STRATEGY_TEMPLATE}"
            },
        };
        _ = await DialogService.ShowAsync<ShareResourceDialog>(null, parameters, GetDefaultDialogOptions());
    }

    private async Task DeleteStrategy()
    {
        await CityStrategyBuilderService.DeleteStrategy();
        NavigationManager.NavigateTo(FogUrlBuilder.PageRoutes.CITY_STRATEGIES_DASHBOARD_PATH, false, true);
    }

    private Task RenameStrategy(string newName)
    {
        return CityStrategyBuilderService.Rename(newName);
    }

    private void FitToScreen()
    {
        if (_skComponent?.SkCanvasView == null)
        {
            return;
        }

        CityPlannerInteractionManager.FitToScreen(_canvasSize);
        _skComponent?.SkCanvasView.Invalidate();
    }

    private void InteractiveCanvasOnPointerDown(PointerEventArgs args)
    {
        CityPlannerInteractionManager.OnPointerDown((float) args.OffsetX, (float) args.OffsetY);
    }

    private void InteractiveCanvasOnPointerMove(PointerEventArgs args)
    {
        if (_skComponent?.SkCanvasView == null)
        {
            return;
        }

        if (args.Buttons != 1)
        {
            return;
        }

        if (CityPlannerInteractionManager.OnPointerMove((float) args.OffsetX, (float) args.OffsetY))
        {
            _skComponent?.SkCanvasView.Invalidate();
        }
    }

    private Task InteractiveCanvasOnPointerUp(PointerEventArgs args)
    {
        if (_skComponent?.SkCanvasView == null)
        {
            return Task.CompletedTask;
        }

        if (CityPlannerInteractionManager.OnPointerUp((float) args.OffsetX, (float) args.OffsetY,
                _extendedExpansionModeIsActive))
        {
            _skComponent?.SkCanvasView.Invalidate();
        }

        return Task.CompletedTask;
    }

    private void InteractiveCanvasOnWheel(WheelEventArgs e)
    {
        if (_skComponent?.SkCanvasView == null)
        {
            return;
        }

        if (CityPlannerInteractionManager.Zoom((float) e.OffsetX, (float) e.OffsetY, (float) e.DeltaY))
        {
            _skComponent?.SkCanvasView.Invalidate();
        }
    }

    private Task OnKeyUp(KeyboardEventArgs args)
    {
        switch (args.Code)
        {
            case KeyboardKeys.Delete:
            case KeyboardKeys.Backspace:
            {
                DeleteCityMapEntity();
                break;
            }

            case "KeyR":
            {
                Rotate();
                break;
            }

            case "KeyD":
            {
                Duplicate();
                break;
            }
        }

        return Task.CompletedTask;
    }

    private void Rotate()
    {
        if (_skComponent?.SkCanvasView == null)
        {
            return;
        }

        if (CityStrategyBuilderService.RotateSelectedCityMapEntity())
        {
            _skComponent?.SkCanvasView.Invalidate();
        }
    }

    private void Duplicate()
    {
        if (_skComponent?.SkCanvasView == null)
        {
            return;
        }

        if (CityStrategyBuilderService.DuplicateSelectedCityMapEntity())
        {
            _skComponent?.SkCanvasView.Invalidate();
        }
    }

    private Task Save()
    {
        return CityStrategyBuilderService.Save();
    }

    private void RequestSaving()
    {
        CityStrategyBuilderService.RequestSaving();
    }

    private void SelectGroup()
    {
        if (_skComponent?.SkCanvasView == null)
        {
            return;
        }

        CityStrategyBuilderService.SelectCityMapEntityGroup();
        _skComponent?.SkCanvasView.Invalidate();
    }

    private void SkCanvasView_OnPaintSurface(SKPaintGLSurfaceEventArgs e)
    {
        var surface = e.Surface;
        var canvas = surface.Canvas;
        _canvasSize = new Size(e.Info.Width, e.Info.Height);
        if (_fitOnPaint)
        {
            CityPlannerInteractionManager.FitToScreen(_canvasSize);
            _fitOnPaint = false;
        }

        CityPlannerInteractionManager.TransformMapArea(canvas);
        CityStrategyBuilderService.RenderScene(canvas);
    }

    private void ToggleLeftPanel(bool toggled)
    {
        _leftPanelIsVisible = toggled;
    }

    private void ToggleRightPanel(bool toggled)
    {
        _rightPanelIsVisible = toggled;
    }

    private void ZoomIn()
    {
        if (_skComponent?.SkCanvasView == null)
        {
            return;
        }

        if (CityPlannerInteractionManager.Zoom(_canvasSize.Width / 2, _canvasSize.Height / 2, -100))
        {
            _skComponent?.SkCanvasView.Invalidate();
        }
    }

    private void ZoomOut()
    {
        if (_skComponent?.SkCanvasView == null)
        {
            return;
        }

        if (CityPlannerInteractionManager.Zoom(_canvasSize.Width / 2, _canvasSize.Height / 2, 100))
        {
            _skComponent?.SkCanvasView.Invalidate();
        }
    }

    private async Task OnCreateTimelineItem(CityStrategyTimelineItemCreateRequest request)
    {
        if (request.Type == CityStrategyNewTimelineItemType.LayoutImport)
        {
            var cities = await CityStrategyBuilderService.GetCities();
            if (cities.Count == 0)
            {
                await DialogService.ShowMessageBoxAsync(
                    null,
                    Loc[FogResource.CityStrategy_NoCityForImport],
                    Loc[FogResource.Common_Ok]);
            }
            else
            {
                var parameters = new DialogParameters<CityPickerDialog>
                {
                    {d => d.Cities, cities},
                };
                var dialog = await DialogService.ShowAsync<CityPickerDialog>(null, parameters,
                    GetDefaultDialogOptions());
                var result = await dialog.Result;
                if (result is not {Canceled: true})
                {
                    request.ExistingCityId = result?.Data as string;
                    await CityStrategyBuilderService.CreateTimelineItemAsync(request);
                }
            }
        }
        else
        {
            await CityStrategyBuilderService.CreateTimelineItemAsync(request);
        }
    }

    private async Task OpenItemTitleDialog(CityStrategyTimelineItemBase item)
    {
        var parameters = new DialogParameters<TimelineItemTitleDialog>
        {
            {d => d.Data, item},
        };
        var dialog = await DialogService.ShowAsync<TimelineItemTitleDialog>(null, parameters,
            GetDefaultDialogOptions());
        await dialog.Result;
        await Save();
    }

    private Task OnDeleteTimelineItem(CityStrategyTimelineItemBase item)
    {
        return CityStrategyBuilderService.DeleteTimelineItem(item);
    }

    private Task OnEditTimelineItem(CityStrategyTimelineItemBase item)
    {
        return OpenItemTitleDialog(item);
    }

    private Task OnSelectTimelineItem(string itemId)
    {
        return CityStrategyBuilderService.SelectTimelineItem(itemId);
    }

    private static DialogOptions GetDefaultDialogOptions()
    {
        return new DialogOptions
        {
            MaxWidth = MaxWidth.Medium,
            FullWidth = true,
            BackgroundClass = "dialog-blur-bg",
            NoHeader = true,
            CloseOnEscapeKey = true,
            Position = DialogPosition.TopCenter,
        };
    }

    private Task OnMoveTimelineItemDown(CityStrategyTimelineItemBase item)
    {
        return CityStrategyBuilderService.MoveTimelineItemDown(item);
    }

    private Task OnMoveTimelineItemUp(CityStrategyTimelineItemBase item)
    {
        return CityStrategyBuilderService.MoveTimelineItemUp(item);
    }

    private Task OnExportToCityPlanner()
    {
        return CityStrategyBuilderService.ExportCurrentLayoutItemToCityPlanner();
    }

    private async Task OnSaveImage()
    {
        var image = CityStrategyBuilderService.GenerateCurrentLayoutItemImage();
        if (image.IsSuccess)
        {
            await JsInteropService.SaveFileAsync($"{CityStrategyBuilderService.SelectedTimelineItem!.Title}.png",
                MediaTypeNames.Image.Png, image.Value);
        }
        else
        {
            _ = await DialogService.ShowMessageBoxAsync("Error generating image",
                string.Join("; ", image.Reasons.Select(r => r.Message)), Loc[FogResource.Common_Ok]);
        }
    }

    private async Task OnShareLayout(CityStrategyLayoutTimelineItem item)
    {
        var data = FogSharingUiService.CreateSharedData(CityStrategyBuilderService.CreateCity(item));
        var parameters = new DialogParameters<ShareResourceDialog>
        {
            {d => d.Data, data},
            {
                d => d.BaseUrl,
                $"{NavigationManager.BaseUri.TrimEnd('/')}{FogUrlBuilder.PageRoutes.GET_SHARED_CITY_TEMPLATE}"
            },
        };
        _ = await DialogService.ShowAsync<ShareResourceDialog>(null, parameters, GetDefaultDialogOptions());
    }

    private Task ChangeWonder(WonderId newWonder)
    {
        return CityStrategyBuilderService.ChangeWonder(newWonder);
    }
}
