namespace Aurila.Contracts.Components;

public interface IForm
{
    void Register(IFormControl control);

    void Unregister(IFormControl control);
}
