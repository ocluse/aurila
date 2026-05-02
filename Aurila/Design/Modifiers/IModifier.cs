namespace Aurila.Design.Modifiers;

public interface IModifier
{

}

public interface IClassModifier : IModifier
{
    void BuildClass(ComponentBase component, ClassBuilder builder);
}

public interface IAttributeModifier : IModifier
{
    void BuildAttributes(ComponentBase component, IDictionary<string, object> attributes);
}

public interface IStyleModifier : IModifier
{
    void BuildStyle(ComponentBase component, StyleBuilder builder);
}