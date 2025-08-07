using Ocluse.LiquidSnow.Data;

namespace Aurila.Components.Controls;

public class Image : ControlBase<Image>
{
    [Parameter]
    public string? Source { get; set; }

    [Parameter]
    public string? FallbackSource { get; set; }

    [Parameter]
    public string? PlaceholderSource { get; set; }

    [Parameter]
    public string? ErrorSource { get; set; }

    [Parameter]
    public string? Alt { get; set; }

    [Parameter]
    public RenderFragment? Placeholder { get; set; }

    [Parameter]
    public RenderFragment? Error { get; set; }

    [Parameter]
    public IImageLoader? ImageLoader { get; set; }

    [Inject]
    private IImageLoader DefaultImageLoader { get; set; } = null!;

    private string? _actualSource;
    private LoadState _loadState;

    protected override async Task OnInitializedAsync()
    {
        var imageLoader = ImageLoader ?? DefaultImageLoader;
        _loadState = LoadState.Loading;
        _actualSource = PlaceholderSource ?? FallbackSource;

        try
        {
            _actualSource = await imageLoader.LoadAsync(Source);
            _loadState = LoadState.NotLoading;
        }
        catch (Exception)
        {
            _actualSource = ErrorSource ?? FallbackSource;
            _loadState = LoadState.Error;
        }
    }

    protected override void BuildClass(ClassBuilder builder)
    {
        base.BuildClass(builder);
        builder.Add("au-image");
        if(_loadState == LoadState.Loading)
        {
            builder.Add("loading");
        }
        else if (_loadState == LoadState.Error)
        {
            builder.Add("error");
        }
        else
        {
            builder.Add("not-loading");
        }
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        base.BuildRenderTree(builder);
        builder.OpenElement(1, "div");
        {
            builder.AddMultipleAttributes(2, GetAppliedAttributes());
            if(!string.IsNullOrEmpty(_actualSource))
            {
                builder.OpenElement(3, "img");
                {
                    builder.AddAttribute(4, "src", _actualSource);
                    if (!string.IsNullOrEmpty(Alt))
                    {
                        builder.AddAttribute(5, "alt", Alt);
                    }
                }
                builder.CloseElement();
            }
            else if (_loadState == LoadState.Loading && Placeholder != null)
            {
                builder.AddContent(6, Placeholder);
            }
            else if (_loadState == LoadState.Error && Error != null)
            {
                builder.AddContent(7, Error);
            }
        }
        builder.CloseElement();
    }
}
