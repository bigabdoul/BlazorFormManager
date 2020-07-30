using BlazorFormManager.Components;
using BlazorFormManager.Debugging;
using BlazorFormManager.Demo.Client.Models;
using Microsoft.AspNetCore.Components;
using System;
using System.Text.Json;

namespace BlazorFormManager.Demo.Client.Pages
{
    public partial class FormManagerRegister
    {
        private static readonly JsonSerializerOptions CaseInsensitiveJson =
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        // make these fields protected to avoid compiler warnings
        protected FormManager<RegisterUserModel> manager;

        private ConsoleLogLevel LogLevel => manager?.LogLevel ?? ConsoleLogLevel.None;
        private RegisterUserModel Model = new RegisterUserModel();

        [Inject] NavigationManager NavigationManager { get; set; }

        private void HandleSubmitDone(FormManagerSubmitResult result)
        {
            // Succeeded means the server responded with a success status code.
            // But we still have to find out how the action really came out.
            if (result.Succeeded && result.XHR.IsJsonResponse)
            {
                try
                {
                    // Since the response is in JSON format, let's parse it and investigate.
                    var response = JsonSerializer.Deserialize<RegisterUserModelResult>(
                        result.XHR.ResponseText, CaseInsensitiveJson);

                    if (!response.Success)
                    {
                        manager.SubmitResult = FormManagerSubmitResult.Failed(result, response.Error);
                    }
                    else if (!string.IsNullOrEmpty(response.Message))
                    {
                        manager.SubmitResult = FormManagerSubmitResult.Success(result, response.Message);
                        if (response.SignedIn)
                        {
                            NavigationManager.NavigateTo("/account/update", true);
                        }
                        else
                        {
                            // invalidate the form by setting a new model
                            Model = new RegisterUserModel();
                            StateHasChanged();
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine(ex);
                }
            }
        }
    }
}
