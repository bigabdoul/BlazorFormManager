using BlazorFormManager.Components;
using BlazorFormManager.Demo.Client.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace BlazorFormManager.Demo.Client.Pages
{
    [Authorize]
    public partial class FormManagerUpdate
    {
        #region fields

        private static readonly JsonSerializerOptions CaseInsensitiveJson = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        internal const string UNKNOWN_SUBMIT_ERROR =
            "We couldn't submit your data for an unknown reason. " +
            "If the problem persists, please contact your administrator";

        private const string SUCCESS_MESSAGE = 
            "Congratulations, your account has been successfully updated!";

        [Inject] private HttpClient Http { get; set; }
        [Inject] private IAccessTokenProvider TokenProvider { get; set; }

        #endregion

        #region overrides

        protected override async Task OnInitializedAsync()
        {
            // This isn't an unnecessary assignment; EditForm needs an initialized Model.
            // Do the initialization before calling any asynchronous method.
            Model = new UpdateUserModel();
            try
            {
                await RetrieveUserInfoAsync();
                await SetRequestHeadersAsync();
            }
            catch (AccessTokenNotAvailableException ex)
            {
                ex.Redirect();
            }
            catch (Exception ex)
            {
                SubmitResult = FormManagerSubmitResult.Failed(null, ex.ToString(), false);
            }
            await base.OnInitializedAsync();
        }

        protected override async Task HandleSubmitDoneAsync(FormManagerSubmitResult result)
        {
            if (result.Succeeded)
            {
                ProcessCustomServerResponse(result);
                if (result.UploadContainedFiles)
                    await remoteImgRef?.RefreshAsync();
            }
            await base.HandleSubmitDoneAsync(result);
        }

        #endregion

        #region helpers

        private async Task RetrieveUserInfoAsync()
        {
            var user = await Http.GetFromJsonAsync<UpdateUserModel>("api/account/info");
            if (user != null)
            {
                // Resetting the model causes the form to NOT submit when 
                // everything's fine. To overcome this weird behavior, invoke
                // the method SubmitFormAsync() on the submit button's @onclick 
                // event handler like so:

                // <button type="button" @onclick="SubmitFormAsync">Save</button>

                // Note that the button's type should be 'button' instead of 'submit'.
                // This avoids the risk of the form being submitted twice.
                Model = user;
            }
        }

        /// <summary>
        /// Custom server response handler.
        /// </summary>
        /// <param name="result">
        /// A reference to the <see cref="FormManagerBase{TModel}.SubmitResult"/> property.
        /// </param>
        private void ProcessCustomServerResponse(FormManagerSubmitResult result)
        {
            // Don't update the state because it will be done by the
            // form manager once execution of this method is finished.
            var xhr = result.XHR;
            if (xhr.IsJsonResponse)
            {
                if (IsDebug) Console.WriteLine($"Raw JSON result: {xhr.ResponseText}");

                try
                {
                    var postResult = JsonSerializer.Deserialize<PostFormHttpResult>(
                        xhr.ResponseText, 
                        CaseInsensitiveJson
                    );

                    if (postResult.Success)
                        SubmitResult = FormManagerSubmitResult.Success(result,
                            postResult.Message ?? SUCCESS_MESSAGE);
                    else if (true == xhr.ExtraProperties?.ContainsKey("error"))
                        SubmitResult = FormManagerSubmitResult.Failed(result,
                            xhr.ExtraProperties["error"]?.ToString());
                    else
                        SubmitResult = FormManagerSubmitResult.Failed(result,
                            postResult.GetErrorDetails() ?? UNKNOWN_SUBMIT_ERROR);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex);
                }
            }
            else if (IsDebug)
            {
                if (xhr.IsHtmlResponse)
                    Console.WriteLine($"HTML result: {xhr.ResponseText}");
                else
                    Console.WriteLine($"Raw result: {xhr.ResponseText}");
            }
        }

        /// <summary>
        /// Attempts to retrieve an access token and add the "authorization" request
        /// header and a few others that will be used to configure the XMLHttpRequest
        /// object when submitting the form via AJAX.
        /// </summary>
        /// <returns></returns>
        private async Task SetRequestHeadersAsync()
        {
            var tokenResponse = await TokenProvider.RequestAccessToken();
            if (tokenResponse.TryGetToken(out var token))
            {
                RequestHeaders = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
                {
                    { "authorization", $"Bearer {token.Value}" },
                    { "x-requested-with", "XMLHttpRequest" },
                    { "x-powered-by", "BlazorFormManager" },
                };
            }
            else
            {
                throw new InvalidOperationException("Could not get access token.");
            }
        }

        #endregion
    }
}
