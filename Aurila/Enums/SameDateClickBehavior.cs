namespace Aurila.Components.Controls;

public enum SameDateClickBehavior
{
    Toggle,  // null if already selected, otherwise select
    Refire,  // always call NotifyValueChanged with the same value
    Ignore   // do nothing if already selected
}
