using BlazorFormManager.Components;
using BlazorFormManager.Components.UI;
using BlazorFormManager.Components.Web;
using BlazorFormManager.Demo.Client.Models;
using BlazorFormManager.Demo.Client.Services;
using Carfamsoft.Model2View.Shared;
using Carfamsoft.Model2View.Shared.Collections;
using Carfamsoft.Model2View.Shared.Extensions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace BlazorFormManager.Demo.Client.Pages
{
    public partial class AutoTablePage
    {
        #region private fields / properties

        private readonly TableStyles Styles1 = new TableStyles
        {
            CssClass = "table table-sm table-dark table-hover",
            HeaderCssClass = "thead-light"
        };

        private readonly TableStyles Styles2 = new TableStyles
        {
            CssClass = "table table-sm table-hover",
            HeaderCssClass = "thead-dark"
        };

        private readonly TableStyles Styles3 = new TableStyles
        {
            CssClass = "table table-sm table-hover",
            ResponsiveCssClass = string.Empty
        };

        private InputSearchOptions SearchOptions { get; set; }

        private readonly HashSet<UpdateUserModel> AllUsers = new HashSet<UpdateUserModel>();
        private readonly HashSet<UpdateUserModel> CurrentUsers = new HashSet<UpdateUserModel>();
        [Inject] private PaginationHttpClient PaginationClient { get; set; }
        [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; }
        [Inject] private NavigationManager NavManager { get; set; }

        private IEnumerable<SelectOptionList> Options { get; set; }
        private PaginationHttpResult<AutoUpdateUserModel> ServerData1 { get; set; } = new PaginationHttpResult<AutoUpdateUserModel>();
        private PaginationHttpResult<AutoUpdateUserModel> ServerData2 { get; set; } = new PaginationHttpResult<AutoUpdateUserModel>();
        private IReadOnlyCollection<AutoUpdateUserModel> FilteredItems { get; set; }

        private bool _isAuthenticated;
        private bool _fetching;
        private bool _serverReady;
        private int _currentPage;
        // to remember last sorting
        private TableCellClickEventArgs _lastHeaderClick;

        private Exception Error { get; set; }
        private int UserCount { get; set; } = 100;
        private int UsersPerPage { get; set; } = 5;
        private string ReturnUrl => $"?ReturnUrl={NavManager.BaseUri}auto-table";
        private string LoginUrl => $"authentication/login{ReturnUrl}";
        private string RegisterUrl => $"authentication/register{ReturnUrl}";

        #endregion

        #region in-memory data demo

        private void InitializeUsers()
        {
            // We would normally fetch the data from the server or another store but
            // for the sake of simplicity, let's generate some in-memory dummy users.
            var totalUsers = UserCount;
            var users = AllUsers;

            users.Clear();

            for (int i = 1; i <= totalUsers; i++)
            {
                users.Add(new UpdateUserModel
                {
                    Email = $"user{i}@example.com",
                    FirstName = $"First {i}",
                    LastName = $"Last {i}",
                    PhoneNumber = $"+123 456 78{i:#000}"
                });
            }

            GotoPage(1);
        }

        private void GotoPage(int page)
        {
            var users = CurrentUsers;
            users.Clear();
            foreach (var user in AllUsers.Skip((page - 1) * UsersPerPage).Take(UsersPerPage))
            {
                users.Add(user);
            }
            StateHasChanged();
        }

        #endregion

        #region server-side fetched data

        private async Task<bool> IsAuthenticatedAsync()
        {
            var result = await AuthStateProvider.GetAuthenticationStateAsync().ConfigureAwait(false);
            return true == result?.User?.Identity?.IsAuthenticated;
        }

        protected override async Task OnInitializedAsync()
        {            
            InitializeUsers();
            PaginationClient.SetBaseUrl("api/account");

            SearchOptions = new InputSearchOptions(this, HandleSearchTextChanged)
            { 
                InputSize = ComponentSize.Sm,
                Placeholder = "Search the table",
            };

            _isAuthenticated = await IsAuthenticatedAsync();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (_isAuthenticated && !(_serverReady || _fetching))
            {
                _fetching = true;
                Options = await PaginationClient.Http.GetFromJsonAsync<IEnumerable<SelectOptionList>>("api/account/options");

                await FetchFromServer1Async(_currentPage = 1);
                await FetchFromServer2Async(_currentPage);
                
                _serverReady = true;
                _fetching = false;
            }

            await base.OnAfterRenderAsync(firstRender);
        }

        private Task HandleHeaderClicked(TableCellClickEventArgs e)
        {
            Console.WriteLine($"Header {e.Name} clicked. Sorting is " +
                $"{(e.SortEnabled ? "enabled" : "disabled")}.");

            _lastHeaderClick = e;
            
            if (e.SortEnabled)
                return FetchFromServer1Async(_currentPage);

            return Task.CompletedTask;
        }

        private Task HandleSearchTextChanged(InputSearchChangedEventArgs e)
        {
            return Task.Run(() => SearchTable());
        }

        private async Task FetchFromServer1Async(int page)
        {
            var result = await FetchPageAsync(page, true);
            if (result != null)
            {
                ServerData1 = result;

                if (SearchOptions.Text.IsNotBlank())
                {
                    SearchTable();
                }
                else
                {
                    StateHasChanged();
                }
            }
        }

        private async Task FetchFromServer2Async(int page)
        {
            var result = await FetchPageAsync(page);
            if (result != null)
            {
                ServerData2 = result;
                StateHasChanged();
            }
        }

        private async Task<PaginationHttpResult<AutoUpdateUserModel>> FetchPageAsync(int page, bool useLastHeaderClick = false)
        {
            try
            {
                _currentPage = page;
                const string requestUri = "users";
                PaginationHttpResult<AutoUpdateUserModel> result;

                if (useLastHeaderClick && 
                    _lastHeaderClick?.Sender is AutoTableBase<AutoUpdateUserModel> table &&
                    table.AllowMultiHeaderSorting)
                {
                    result = await PaginationClient.GetFromJsonAsync<AutoUpdateUserModel>(
                       requestUri
                       , page
                       , pageSize: UsersPerPage
                       , table.Headers.Where(hdr => !hdr.Hidden).ToArray());
                }
                else
                {
                    result = await PaginationClient.GetFromJsonAsync<AutoUpdateUserModel>(
                        requestUri
                        , page
                        , pageSize: UsersPerPage
                        , sorting: _lastHeaderClick?.SortAscending
                        , sortColumn: _lastHeaderClick?.Name);
                }

                Error = null;
                return result;
            }
            catch (AccessTokenNotAvailableException ex)
            {
                ex.Redirect();
            }
            catch (Exception ex)
            {
                Error = ex;
                StateHasChanged();
            }
            return null;
        }

        private void SearchTable()
        {
            FilteredItems = ServerData1?.Items?.SearchProperties(SearchOptions.Text, IncludeProperty);
            Console.WriteLine($"Filtered item count: {FilteredItems?.Count}");
            StateHasChanged();

            static bool IncludeProperty(string name)
            {
                return
                    name != nameof(AutoUpdateUserModel.AgeRange) &&
                    name != nameof(AutoUpdateUserModel.FavouriteWorkingDay);
            }
        }

        #endregion

        #region shared code

        private object GetCellData(string property, UpdateUserModel item)
        {
            switch (property)
            {
                // UpdateUserModel
                case nameof(UpdateUserModel.Email):
                    return (MarkupString)$"<a href=\"mailto:{item.Email}\">{item.Email}</a>";
                case nameof(UpdateUserModel.FirstName):
                    return item.FirstName;
                case nameof(UpdateUserModel.LastName):
                    return item.LastName;
                case nameof(UpdateUserModel.PhoneNumber):
                    return item.PhoneNumber;

                // AutoUpdateUserModel
                case nameof(AutoUpdateUserModel.FavouriteWorkingDay):
                    return ((AutoUpdateUserModel)item).FavouriteWorkingDay;
                case nameof(AutoUpdateUserModel.AgeRange):
                    return ((AutoUpdateUserModel)item).AgeRange;
                case nameof(AutoUpdateUserModel.FavouriteColor):
                    return ((AutoUpdateUserModel)item).FavouriteColor;
                case nameof(AutoUpdateUserModel.TwoFactorEnabled):
                    return ((AutoUpdateUserModel)item).TwoFactorEnabled ? "Yes" : "No";
                default:
                    break;
            }
            return null;
        }

        private void HandleHeadersSet(ICollection<TableCell> headers)
        {
            foreach (var hdr in headers)
            {
                if (hdr.Name == nameof(AutoUpdateUserModel.AgeRange) ||
                    hdr.Name == nameof(AutoUpdateUserModel.FavouriteWorkingDay))
                {
                    hdr.Hidden = true;
                }
                else if (hdr.Name == nameof(AutoUpdateUserModel.TwoFactorEnabled))
                {
                    hdr.NonClickable = true;
                }
            }
        }

        #endregion
    }
}
