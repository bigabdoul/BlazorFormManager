using BlazorFormManager.Components;
using BlazorFormManager.Components.Forms;
using BlazorFormManager.Components.UI;
using BlazorFormManager.Debugging;
using BlazorFormManager.Demo.Client.Extensions;
using BlazorFormManager.Demo.Client.Models;
using System;
using System.Threading.Tasks;

namespace BlazorFormManager.Demo.Client.Pages
{
    public partial class AutoEditFormRegister
    {
        private DragDropArea DragDropAreaRef { get; set; }
        private RegisterUserModel Model { get; set; } = new RegisterUserModel();
        private AutoEditForm<RegisterUserModel> Manager { get; set; }
        private ConsoleLogLevel LogLevel => Manager?.LogLevel ?? ConsoleLogLevel.Debug;

        private async Task HandleFieldChanged(FormFieldChangedEventArgs e)
        {
            if (LogLevel > ConsoleLogLevel.None)
            {
                Console.WriteLine($"Field value changed: FieldName={e.Field.FieldName} ; FieldId={e.FieldId} Value={e.Value} ; IsFile={e.IsFile}");
            }
            if (e.IsEmptyFile(nameof(RegisterUserModel.Photo)))
            {
                if (DragDropAreaRef != null) await DragDropAreaRef.DeleteFileListAsync();
                StateHasChanged();
            }
        }

        private async Task HandleSubmitDone(FormManagerSubmitResult result)
        {
            Manager.ProcessCustomServerResponse(result);
            if (true == Manager.SubmitResult?.Succeeded)
            {
                await Task.Delay(2000);
                NavigationManager.NavigateTo("/account/autoeditform", true);
            }
        }
    }
}
