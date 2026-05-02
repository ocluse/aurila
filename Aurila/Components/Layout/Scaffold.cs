using Aurila.Design;

namespace Aurila.Components.Layout;

public class Scaffold : ControlBase<Scaffold>
{
    [Parameter]
    public RenderFragment? TopBar { get; set; }

    [Parameter]
    public string? TopBarClass { get; set; }

    [Parameter]
    public Action<ClassBuilder>? BuildTopBarClass { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public string? ChildContentClass { get; set; }

    [Parameter]
    public Action<ClassBuilder>? BuildChildContentClass { get; set; }

    [Parameter]
    public RenderFragment? BottomBar { get; set; }

    [Parameter]
    public string? BottomBarClass { get; set; }

    [Parameter]
    public Action<ClassBuilder>? BuildBottomBarClass { get; set; }

    [Parameter]
    public RenderFragment? FloatingActionButton { get; set; }

    [Parameter]
    public string? FloatingActionButtonClass { get; set; }

    [Parameter]
    public Action<ClassBuilder>? BuildFloatingActionButtonClass { get; set; }

    [Parameter]
    public RenderFragment? SnackbarHost { get; set; }

    [Parameter]
    public string? SnackbarHostClass { get; set; }

    [Parameter]
    public Action<ClassBuilder>? BuildSnackbarHostClass { get; set; }

    protected override void BuildClass(ClassBuilder builder)
    {
        base.BuildClass(builder);
        builder.Add("au-scaffold");
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        {
            builder.AddMultipleAttributes(1, GetAppliedAttributes());

            if (TopBar != null)
            {
                builder.OpenRegion(3);
                RenderTopBar(builder);
                builder.CloseRegion();
            }

            builder.OpenRegion(4);
            RenderMain(builder);
            builder.CloseRegion();

            //bottom bar:
            if (BottomBar is not null)
            {
                builder.OpenRegion(5);
                RenderBottomBar(builder);
                builder.CloseRegion();
            }

            //floating action button:
            if (FloatingActionButton is not null)
            {
                builder.OpenRegion(6);
                RenderFab(builder);
                builder.CloseRegion();
            }

            //snackbar host:
            if (SnackbarHost is not null)
            {
                builder.OpenRegion(7);
                RenderSnackbarHost(builder);
                builder.CloseRegion();
            }
        }
        builder.CloseElement(); //div
    }

    private void RenderMain(RenderTreeBuilder builder)
    {
        //main:
        ClassBuilder childContentClassBuilder = new();
        childContentClassBuilder.Add("au-scaffold__content");
        BuildChildContentClass?.Invoke(childContentClassBuilder);
        
        if (ChildContentClass.IsNotWhiteSpace())
        {
            childContentClassBuilder.Add(ChildContentClass);
        }

        builder.OpenElement(0, "main");
        {
            builder.AddAttribute(1, "class", childContentClassBuilder.ToString());
            builder.AddContent(2, ChildContent);
        }
        builder.CloseElement(); //main
    }

    private void RenderTopBar(RenderTreeBuilder builder)
    {
        ClassBuilder topBarClassBuilder = new();
        topBarClassBuilder.Add("au-scaffold__topbar");
        BuildTopBarClass?.Invoke(topBarClassBuilder);

        if (TopBarClass.IsNotWhiteSpace())
        {
            topBarClassBuilder.Add(TopBarClass);
        }

        builder.OpenElement(0, "header");
        {
            builder.AddAttribute(1, "class", topBarClassBuilder.ToString());
            builder.AddContent(2, TopBar);
        }
        builder.CloseElement(); //header
    }

    private void RenderBottomBar(RenderTreeBuilder builder)
    {
        ClassBuilder bottomBarClassBuilder = new();
        bottomBarClassBuilder.Add("au-scaffold__bottombar");
        BuildBottomBarClass?.Invoke(bottomBarClassBuilder);

        if (BottomBarClass.IsNotWhiteSpace())
        {
            bottomBarClassBuilder.Add(BottomBarClass);
        }

        builder.OpenElement(0, "footer");
        {
            builder.AddAttribute(1, "class", bottomBarClassBuilder.ToString());
            builder.AddContent(2, BottomBar);
        }
        builder.CloseElement(); //footer
    }

    private void RenderFab(RenderTreeBuilder builder)
    {
        ClassBuilder fabClassBuilder = new();
        fabClassBuilder.Add("au-scaffold__fab");
        BuildFloatingActionButtonClass?.Invoke(fabClassBuilder);

        if (FloatingActionButtonClass.IsNotWhiteSpace())
        {
            fabClassBuilder.Add(FloatingActionButtonClass);
        }

        builder.OpenElement(0, "div");
        {
            builder.AddAttribute(1, "class", fabClassBuilder.ToString());
            builder.AddContent(2, FloatingActionButton);
        }
        builder.CloseElement(); //div
    }

    private void RenderSnackbarHost(RenderTreeBuilder builder)
    {
        ClassBuilder snackbarHostClassBuilder = new();
        snackbarHostClassBuilder.Add("au-scaffold__snackbarhost");
        BuildSnackbarHostClass?.Invoke(snackbarHostClassBuilder);
        
        if (SnackbarHostClass.IsNotWhiteSpace())
        {
            snackbarHostClassBuilder.Add(SnackbarHostClass);
        }
        builder.OpenElement(0, "div");
        {
            builder.AddAttribute(1, "class", snackbarHostClassBuilder.ToString());
            builder.AddContent(2, SnackbarHost);
        }
        builder.CloseElement(); //div
    }
}
