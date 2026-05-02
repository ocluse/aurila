using Aurila.Design;
using Ocluse.LiquidSnow.Data;

namespace Aurila.Components.Controls;

public class AuImage : AuControlBase<AuImage>, IDisposable, IHasMargin, IHasSize
{
    [Parameter]
    public ImageSource? Source { get; set; }

    [Parameter]
    public string? PlaceholderSrc { get; set; }

    [Parameter]
    public RenderFragment? PlaceholderContent { get; set; }

    [Parameter]
    public RenderFragment? LoadingContent { get; set; }

    [Parameter]
    public string? ErrorSrc { get; set; }

    [Parameter]
    public RenderFragment? ErrorContent { get; set; }

    [Parameter]
    public string? Description { get; set; }

    [Parameter]
    public IImageLoader? ImageLoader { get; set; }

    [Parameter]
    public EventCallback Loaded { get; set; }

    [Parameter]
    public EventCallback Error { get; set; }

    [Parameter]
    public CssLength? Margin { get; set; }
    
    [Parameter]
    public CssLength? MarginHorizontal { get; set; }
    
    [Parameter]
    public CssLength? MarginVertical { get; set; }
    
    [Parameter]
    public CssLength? MarginRight { get; set; }
    
    [Parameter]
    public CssLength? MarginLeft { get; set; }
    
    [Parameter]
    public CssLength? MarginTop { get; set; }
    
    [Parameter]
    public CssLength? MarginBottom { get; set; }
    
    [Parameter]
    public CssLength? Width { get; set; }
    
    [Parameter]
    public CssLength? Height { get; set; }
    
    [Parameter]
    public CssLength? MinWidth { get; set; }
    
    [Parameter]
    public CssLength? MaxWidth { get; set; }
    
    [Parameter]
    public CssLength? MinHeight { get; set; }
    
    [Parameter]
    public CssLength? MaxHeight { get; set; }

    [Inject]
    private IImageLoader DefaultImageLoader { get; set; } = null!;

   

    private LoadState _loadState = LoadState.NotLoading;

    private string? _resolvedSource;
    private CancellationTokenSource? _cts;
    private bool _disposedValue;
    private bool _isLoadCancellation;

    public override async Task SetParametersAsync(ParameterView parameters)
    {
        var sourceProvided = parameters.TryGetValue<ImageSource?>(
            nameof(Source), out var newSource);

        await base.SetParametersAsync(parameters);

        if (sourceProvided)
        {
            await LoadSourceAsync(newSource);
        }
    }

    private async Task LoadSourceAsync(ImageSource? source)
    {
        CancelOngoingLoad();
        var loader = ImageLoader ?? DefaultImageLoader;
        _cts = new CancellationTokenSource();

        if (source == null)
        {
            _loadState = LoadState.NotLoading;
            _resolvedSource = null;
            await InvokeAsync(StateHasChanged);
        }
        else
        {
            _loadState = LoadState.Loading;
            _resolvedSource = null;
            await InvokeAsync(StateHasChanged);

            try
            {
                _resolvedSource = await loader.LoadAsync(source, _cts.Token);
                _loadState = LoadState.NotLoading;
            }
            catch (OperationCanceledException) when (_isLoadCancellation)
            {
                // Load was cancelled, do nothing
            }
            catch
            {
                _loadState = LoadState.Error;
            }
            finally
            {
                await InvokeAsync(StateHasChanged);
            }
        }

        _isLoadCancellation = false;
    }

    private void CancelOngoingLoad()
    {
        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
            _isLoadCancellation = true;
        }
    }

    protected override void BuildClass(ClassBuilder builder)
    {
        base.BuildClass(builder);
        builder.Add("au-image");

        switch (_loadState)
        {
            case LoadState.Loading:
                builder.Add("au-image--loading");
                break;

            case LoadState.Error:
                builder.Add("au-image--error");
                break;

            case LoadState.NotLoading:
                builder.Add("au-image--not-loading");
                break;
        }
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        {
            builder.AddMultipleAttributes(1, GetAppliedAttributes());

            if (_loadState == LoadState.NotLoading)
            {
                if (_resolvedSource.IsNotEmpty())
                {
                    builder.OpenElement(2, "img");
                    {
                        builder.AddAttribute(3, "src", _resolvedSource);
                        if (!string.IsNullOrEmpty(Description))
                        {
                            builder.AddAttribute(4, "alt", Description);
                        }
                        builder.AddAttribute(5, "onload", EventCallback.Factory.Create(this, OnLoadedAsync));
                        builder.AddAttribute(6, "onerror", EventCallback.Factory.Create(this, OnErrorAsync));
                    }
                    builder.CloseElement(); // img
                }
                else if (PlaceholderContent != null)
                {
                    builder.AddContent(7, PlaceholderContent);
                }
                else if (PlaceholderSrc != null)
                {
                    builder.OpenElement(8, "img");
                    builder.AddAttribute(9, "src", PlaceholderSrc);
                    builder.CloseElement(); // img
                }
            }
            else if (_loadState == LoadState.Loading)
            {
                if (LoadingContent != null)
                {
                    builder.AddContent(100, LoadingContent);
                }
                else if (PlaceholderContent != null)
                {
                    builder.AddContent(101, PlaceholderContent);
                }
                else if (PlaceholderSrc.IsNotEmpty())
                {
                    builder.OpenElement(102, "img");
                    builder.AddAttribute(103, "src", PlaceholderSrc);
                    builder.CloseElement(); // img
                }
            }
            else if (_loadState == LoadState.Error)
            {
                if (ErrorContent != null)
                {
                    builder.AddContent(200, ErrorContent);
                }
                else if (ErrorSrc.IsNotEmpty())
                {
                    builder.OpenElement(201, "img");
                    builder.AddAttribute(202, "src", ErrorSrc);
                    builder.CloseElement(); // img
                }
                else if (PlaceholderContent != null)
                {
                    builder.AddContent(203, PlaceholderContent);
                }
                else if (PlaceholderSrc.IsNotEmpty())
                {
                    builder.OpenElement(204, "img");
                    builder.AddAttribute(205, "src", PlaceholderSrc);
                    builder.CloseElement(); // img
                }
            }
        }
        builder.CloseElement(); // div
    }


    private async Task OnLoadedAsync()
    {
        if (Loaded.HasDelegate)
        {
            await Loaded.InvokeAsync(null);
        }
    }

    private async Task OnErrorAsync()
    {
        _loadState = LoadState.Error;
        await InvokeAsync(StateHasChanged);

        if (Error.HasDelegate)
        {
            await Error.InvokeAsync(null);
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _cts?.Cancel();
                _cts?.Dispose();
            }
            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
