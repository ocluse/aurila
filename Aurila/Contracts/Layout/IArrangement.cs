using Aurila.Design;
using Aurila.Enums.Layout;
using Microsoft.AspNetCore.Components;

namespace Aurila.Contracts.Layout;

public interface IArrangement
{
    void BuildClass(Axis? axis, ComponentBase component, ClassBuilder builder);

    void BuildStyle(Axis? axis, ComponentBase component, StyleBuilder builder);
}

public interface IVerticalArrangement : IArrangement { }

public interface IHorizontalArrangement : IArrangement { }

public interface IBidirectionalArrangement : IVerticalArrangement, IHorizontalArrangement { }