namespace Aurila.Contracts;

public interface IFocusable
{
    Task FocusAsync();

    Task BlurAsync();
}
