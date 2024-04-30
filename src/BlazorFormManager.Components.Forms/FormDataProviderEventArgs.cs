using System.ComponentModel;

namespace BlazorFormManager.Components.Forms;

public class FormDataProviderEventArgs : CancelEventArgs
{
    public FormDataProviderEventArgs(IDictionary<string, object?> model)
    {
        Model = model;
    }

    public FormDataProviderEventArgs(IDictionary<string, object?> model, bool cancel) : base(cancel)
    {
        Model = model;
    }

    public IDictionary<string, object?> Model { get; init; }
    public Exception? Error { get; set; }
}
