using BlazorFormManager.Components;
using BlazorFormManager.Debugging;
using BlazorFormManager.Demo.Client.Extensions;
using BlazorFormManager.Demo.Client.Models;
using BlazorFormManager.Demo.Client.Shared;
using BlazorFormManager.IO;
using System;
using System.Threading.Tasks;

namespace BlazorFormManager.Demo.Client.Pages
{
    public partial class AutoEditFormRegister
    {
        private Exception Error { get; set; }
        private bool FileSelected { get; set; }
        private Base64RemoteImage Base64ImgRef { get; set; }
        private RegisterUserModel Model { get; set; } = new RegisterUserModel();
        private AutoEditForm<RegisterUserModel> Manager { get; set; }
        private ConsoleLogLevel LogLevel => Manager?.LogLevel ?? ConsoleLogLevel.Debug;

        private void HandleFieldChanged(FormFieldChangedEventArgs e)
        {
            if (LogLevel > ConsoleLogLevel.None)
            {
                Console.WriteLine($"Field value changed: FieldName={e.Field.FieldName} ; FieldId={e.FieldId} Value={e.Value} ; IsFile={e.IsFile}");
            }
            if (e.IsEmptyFile(nameof(RegisterUserModel.Photo)))
            {
                Error = null;
                FileSelected = false;
                Base64ImgRef?.SetDataUrl(null);
            }
        }

        private void HandleFileReaderResult(FileReaderResult result)
        {
            LogMessage($"{(result.Succeeded ? "Done" : "Failed")} reading file with method {result.Method}.");
            try
            {
                if (result.Succeeded)
                {
                    Error = null;
                    FileSelected = false;
                    if (!result.CompletedInScript)
                    {
                        switch (result.Method)
                        {
                            case ComponentModel.ViewAnnotations.FileReaderMethod.ReadAsDataURL:
                                if (result.InputName == nameof(RegisterUserModel.Photo))
                                {
                                    LogMessage($"File content length={result.Content.Length}");
                                    Base64ImgRef?.SetDataUrl(result.Content);
                                }
                                break;
                            case ComponentModel.ViewAnnotations.FileReaderMethod.ReadAsArrayBuffer:
                                if (result.InputName == nameof(RegisterUserModel.Photo))
                                {
                                    var bytes = result.ContentArrayAsByteArray();
                                    LogMessage($"File content length={bytes.Length} bytes");
                                    Base64ImgRef?.SetImage(bytes);
                                }
                                break;
                            default:
                                break;
                        }
                    }
                    else
                    {
                        FileSelected = true;
                        LogMessage("Operation successfully completed in JavaScript.");
                    }
                }
                else
                {
                    LogError(new Exception($"An error occurred during a file read operation: {result.Error}"));
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
        }

        private async Task HandleSubmitDone(FormManagerSubmitResult result)
        {
            Error = null;
            Manager.ProcessCustomServerResponse(result);
            if (result.Succeeded)
            {
                await Task.Delay(2000);
                NavigationManager.NavigateTo("/account/autoeditform", true);
            }
        }

        private void LogMessage(string message)
        {
            if (LogLevel > ConsoleLogLevel.None)
                Console.WriteLine(message);
        }

        private void LogError(Exception error)
        {
            if (LogLevel > ConsoleLogLevel.None)
                Console.WriteLine(error);

            Error = error;
            StateHasChanged();
        }
    }
}
