namespace Aurila.Enums.Input;

/// <summary>
/// Determines how a multiline field treats a virtual keyboard's line-break action when the browser
/// exposes it through <c>beforeinput</c>.
/// </summary>
public enum VirtualEnterBehavior
{
    /// <summary>Use the action assigned to unmodified Enter.</summary>
    FollowUnmodifiedEnter,

    /// <summary>Keep the virtual keyboard's native line-break behavior.</summary>
    NewLine,

    /// <summary>Submit instead of inserting a line break.</summary>
    Submit
}
