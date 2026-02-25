using System;
using System.Collections.Generic;
using System.Text;

namespace Aurila.Components.Controls;

public class SnackbarHost : ControlBase<SnackbarHost>
{
    [Parameter]
    public int MaxVisibleItems { get; set; } = 1;

    protected override void BuildClass(ClassBuilder builder)
    {
        base.BuildClass(builder);
        builder.Add("au-snackbar-host");
    }

    public async Task ShowMessageAsync(string message, TimeSpan? duration = null)
    {
        
    }

    public void ShowMessage(string message, TimeSpan? duration = null)
    {

    }
}
