using BlazorFormManager.Components;
using BlazorFormManager.Debugging;
using BlazorFormManager.Demo.Client.Models;
using BlazorFormManager.Demo.Client.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace BlazorFormManager.Demo.Client.Pages
{
    public partial class AutoEditFormUpdate
    {
        private static IEnumerable<SelectOptionList> _options;
        private AutoEditForm<AutoUpdateUserModel> Manager { get; set; }
        private IDictionary<string, object> RequestHeaders { get; set; }
        private AutoUpdateUserModel Model { get; set; }
        private ConsoleLogLevel LogLevel => Manager?.LogLevel ?? ConsoleLogLevel.None;
        [Inject] private HttpClient Http { get; set; }
        [Inject] private IRequestHeadersProvider HeadersProvider { get; set; }

        [Parameter] public string ModelId { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                Model = await Http.GetFromJsonAsync<AutoUpdateUserModel>($"api/account/info/{ModelId}");
                RequestHeaders = await HeadersProvider.CreateAsync();
                if (_options == null) _options = await Http.GetFromJsonAsync<IEnumerable<SelectOptionList>>("api/account/options");
            }
            catch (AccessTokenNotAvailableException ex)
            {
                ex.Redirect();
            }
            catch (Exception ex)
            {
                if (Manager != null) Manager.SubmitResult = FormManagerSubmitResult.Failed(null, ex.ToString(), false);
            }
            await base.OnInitializedAsync();
        }

        private IEnumerable<SelectOption> GetOptions(string propertyName) 
            => _options?.Where(opt => opt.PropertyName == propertyName).FirstOrDefault()?.Items;
    }
}