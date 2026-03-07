using Ocluse.LiquidSnow.Data;

namespace Aurila.Components.Controls;

public class Image : ControlBase<Image>, IDisposable
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
                builder.Add("au-image__loading");
                break;

            case LoadState.Error:
                builder.Add("au-image__error");
                break;

            case LoadState.NotLoading:
                builder.Add("au-image__not-loading");
                break;
        }
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(1, "div");
        {
            builder.AddMultipleAttributes(2, GetAppliedAttributes());

            if (_loadState == LoadState.NotLoading)
            {
                if (_resolvedSource.IsNotEmpty())
                {
                    builder.OpenElement(3, "img");
                    {
                        builder.AddAttribute(4, "src", _resolvedSource);
                        if (!string.IsNullOrEmpty(Description))
                        {
                            builder.AddAttribute(5, "alt", Description);
                        }
                    }
                    builder.CloseElement();
                }
                else if (PlaceholderContent != null)
                {
                    builder.AddContent(6, PlaceholderContent);
                }
                else if (PlaceholderSrc != null)
                {
                    builder.OpenElement(7, "img");
                    builder.AddAttribute(8, "src", PlaceholderSrc);
                    builder.CloseElement();
                }
            }
            else if (_loadState == LoadState.Loading)
            {
                if (LoadingContent != null)
                {
                    builder.AddContent(9, LoadingContent);
                }
                else if (PlaceholderContent != null)
                {
                    builder.AddContent(10, PlaceholderContent);
                }
                else if (PlaceholderSrc.IsNotEmpty())
                {
                    builder.OpenElement(11, "img");
                    builder.AddAttribute(12, "src", PlaceholderSrc);
                    builder.CloseElement();
                }
            }
            else if (_loadState == LoadState.Error)
            {
                if (ErrorContent != null)
                {
                    builder.AddContent(13, ErrorContent);
                }
                else if (ErrorSrc.IsNotEmpty())
                {
                    builder.OpenElement(14, "img");
                    builder.AddAttribute(15, "src", ErrorSrc);
                    builder.CloseElement();
                }
                else if (PlaceholderContent != null)
                {
                    builder.AddContent(16, PlaceholderContent);
                }
                else if (PlaceholderSrc.IsNotEmpty())
                {
                    builder.OpenElement(17, "img");
                    builder.AddAttribute(18, "src", PlaceholderSrc);
                    builder.CloseElement();
                }
            }
        }
        builder.CloseElement();
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
