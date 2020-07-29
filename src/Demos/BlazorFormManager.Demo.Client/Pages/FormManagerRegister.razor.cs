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
        private readonly RegisterUserModel Model = new RegisterUserModel();

        [Inject] NavigationManager NavigationManager { get; set; }

        private void HandleSubmitDone(FormManagerSubmitResult result)
        {
            if (result.Succeeded && result.XHR.IsJsonResponse)
            {
                try
                {
                    var model = JsonSerializer.Deserialize<RegisterUserModelResult>(
                        result.XHR.ResponseText, CaseInsensitiveJson);

                    if (!model.Success)
                    {
                        manager.SubmitResult = FormManagerSubmitResult.Failed(result, model.Error);
                    }
                    else if (!string.IsNullOrEmpty(model.Message))
                    {
                        manager.SubmitResult = FormManagerSubmitResult.Success(result, model.Message);
                        if (model.SignedIn)
                        {
                            NavigationManager.NavigateTo("/account/update", true);
                        }
                        StateHasChanged();
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
