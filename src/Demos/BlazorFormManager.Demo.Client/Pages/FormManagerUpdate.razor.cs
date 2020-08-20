using BlazorFormManager.Components;
using BlazorFormManager.Demo.Client.Extensions;
using BlazorFormManager.Demo.Client.Models;
using BlazorFormManager.Demo.Client.Services;
using BlazorFormManager.Demo.Client.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using System;
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

        protected Base64RemoteImage RemoteImgRef;

        [Inject] private HttpClient Http { get; set; }
        [Inject] private IRequestHeadersProvider HeadersProvider { get; set; }

        #endregion

        #region overrides

        protected override async Task OnInitializedAsync()
        {
            try
            {
                await RetrieveUserInfoAsync();
                RequestHeaders = await HeadersProvider.CreateAsync();
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
                this.ProcessCustomServerResponse(result);
                if (result.UploadContainedFiles)
                    await RemoteImgRef?.RefreshAsync();
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

        #endregion
    }
}
