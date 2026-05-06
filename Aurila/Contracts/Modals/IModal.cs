namespace Aurila.Contracts.Modals;
public interface IModal
{
    Task ShowAsync();
    Task HideAsync();
}
