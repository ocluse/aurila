namespace Aurila.Contracts.Input;

public interface IForm
{
    void Register(IFormControl control);

    void Unregister(IFormControl control);
}
