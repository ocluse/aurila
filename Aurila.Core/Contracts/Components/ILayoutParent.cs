namespace Aurila.Contracts.Components;

public interface ILayoutParent
{
    RenderFragment? ChildContent { get; }
}