namespace Aurila.Contracts.Layout;

public interface ILayoutParent
{
    RenderFragment? ChildContent { get; }
}