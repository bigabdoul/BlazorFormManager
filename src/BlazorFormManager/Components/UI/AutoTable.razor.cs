using BlazorFormManager.Components.Web;
using Carfamsoft.Model2View.Annotations;
using Carfamsoft.Model2View.Shared.Extensions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace BlazorFormManager.Components.UI
{
    /// <summary>
    /// Represents a table component that automatically renders its 
    /// header columns and rows using metadata retrieved from a model.
    /// </summary>
    /// <typeparam name="TItem">
    /// The type of items rendered as rows and from which metadata is retrieved.
    /// </typeparam>
    public abstract class AutoTableBase<TItem> : ComponentBase
    {
        #region fields

        private static readonly ConcurrentDictionary<string, PropertyInfo> propertyCache = new();

        #endregion
        
        #region constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoTableBase{TItem}"/> class.
        /// </summary>
        protected AutoTableBase()
        {
        } 

        #endregion

        #region properties

        /// <summary>
        /// Gets a collection of grouped <see cref="FormDisplayAttribute"/> class instances.
        /// </summary>
        protected IReadOnlyCollection<FormDisplayGroupMetadata>? Metadata { get; private set; }

        /// <summary>
        /// Gets a collection of <see cref="TableCell"/> items used to render the table header.
        /// </summary>
        public IReadOnlyCollection<TableCell> Headers { get; private set; } = new HashSet<TableCell>(); 
        
        #endregion

        #region parameters

        /// <summary>
        /// Gets or sets the items rendered as rows.
        /// </summary>
        [Parameter] public IReadOnlyCollection<TItem>? Items { get; set; }

        /// <summary>
        /// Gets or sets a collection of items that have been filtered.
        /// </summary>
        [Parameter] public IReadOnlyCollection<TItem>? FilteredItems { get; set; }

        /// <summary>
        /// Gets or sets the render fragment when items are being loaded.
        /// </summary>
        [Parameter] public RenderFragment? Loading { get; set; }

        /// <summary>
        /// Gets or sets the render fragment when no items have been retrieved.
        /// </summary>
        [Parameter] public RenderFragment? Empty { get; set; }

        /// <summary>
        /// A template consisting of a single &lt;thead>&lt;tr>&lt;/tr>&lt;/thead>
        /// and one or more &lt;th>&lt;/th> HTML elements.
        /// </summary>
        [Parameter] public RenderFragment? TableHeader { get; set; }

        /// <summary>
        /// Gets or sets a render fragment used to build a table body row.
        /// </summary>
        [Parameter] public RenderFragment<TItem>? TableBodyRow { get; set; }

        /// <summary>
        /// Gets or sets a function used to retrieve the data of an individual table cell.
        /// </summary>
        [Parameter] public Func<string, TItem, object>? GetCellData { get; set; }

        /// <summary>
        /// Gets or sets the CSS styles to apply to the table.
        /// </summary>
        [Parameter] public TableStyles Styles { get; set; } = new TableStyles();

        /// <summary>
        /// Indicates whether the table headers are clickable.
        /// </summary>
        [Parameter] public bool EnableHeaderClick { get; set; }

        /// <summary>
        /// Indicates whether sorting on the headers is enabled.
        /// </summary>
        [Parameter] public bool EnableSorting { get; set; }
        
        /// <summary>
        /// Indicates whether this table allows sorting on multiple headers.
        /// </summary>
        [Parameter] public bool AllowMultiHeaderSorting { get; set; }

        /// <summary>
        /// Indicates whether the <see cref="Headers"/> should be rendered if 
        /// <see cref="TableHeader"/> is undefined.
        /// </summary>
        [Parameter] public bool NoHeader { get; set; }

        /// <summary>
        /// Gets or sets the configuration options for an embedded input search text box.
        /// </summary>
        [Parameter] public InputSearchOptions? SearchOptions { get; set; }

        /// <summary>
        /// Gets or sets an event callback delegate used to handle notifications
        /// when the <see cref="Headers"/> property value changes. This is an
        /// opportunity to modify the collection, including adding or removing.
        /// </summary>
        [Parameter] public EventCallback<ICollection<TableCell>> OnHeadersSet { get; set; }

        /// <summary>
        /// Gets or sets an event callback delegate invoked when a table header has been clicked.
        /// </summary>
        [Parameter] public EventCallback<TableCellClickEventArgs> OnHeaderClicked { get; set; }

        #region pagination support

        /// <summary>
        /// Gets or sets the total items for the <see cref="DataPager"/> component.
        /// </summary>
        [Parameter] public int TotalItemCount { get; set; }

        /// <summary>
        /// Gets or sets the number of items to display on each page.
        /// </summary>
        [Parameter] public int ItemsPerPage { get; set; } = 5;
        /*
        /// <summary>
        /// Gets or sets a one-based integer representing the current page number.
        /// </summary>
        [Parameter] public int CurrentPage { get; set; }
        */
        /// <summary>
        /// Gets or sets an event callback delegate that is invoked when the current
        /// navigation page of the <see cref="DataPager"/> component changes.
        /// </summary>
        [Parameter] public EventCallback<int> OnPageChange { get; set; }

        #endregion

        #endregion

        #region methods

        /// <summary>
        /// Use user-provided method or reflection to retrieve the property's value.
        /// </summary>
        /// <param name="propertyName">The item's property name whose value to retrieve.</param>
        /// <param name="item">The item for which to retrieve the specified property's value.</param>
        /// <returns></returns>
        protected virtual object? HandleGetCellData(string propertyName, TItem item)
        {
            if (GetCellData != null)
                return GetCellData.Invoke(propertyName, item);

            if (!propertyCache.TryGetValue(propertyName, out var property))
            {
                property = item?.GetType().GetProperty(propertyName);

                if (property != null)
                    propertyCache.TryAdd(propertyName, property);
            }

            return property?.GetValue(item, null);
        }

        /// <inheritdoc/>
        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();
            await ExtractMetadataAsync();
        }

        /// <summary>
        /// Overrides the existing <see cref="Headers"/> collection with the specified one.
        /// </summary>
        /// <param name="collection">The collection of <see cref="TableCell"/> elements to set.</param>
        public virtual void SetHeaders(IEnumerable<TableCell> collection)
        {
            if (collection == null) 
                throw new ArgumentNullException(nameof(collection));

            var list = new List<TableCell>();

            foreach (var hdr in collection)
                list.Add(hdr);

            if (OnHeadersSet.HasDelegate)
                OnHeadersSet.InvokeAsync(list);

            Headers = list.AsReadOnly();

            StateHasChanged();
        }

        #endregion

        #region helpers

        private async Task ExtractMetadataAsync()
        {
            if (Metadata == null)
            {
                var model = Items != null ? Items.FirstOrDefault() : default;

                if (!EqualityComparer<TItem?>.Default.Equals(model, default) && 
                    model.ExtractMetadata(out var result))
                {
                    Metadata = result;

                    if (await ExtractHeadersAsync() && EnableHeaderClick)
                        CreateHeaderClickEventHandlers();

                    if (TotalItemCount == 0)
                        TotalItemCount = Items!.Count();

                    StateHasChanged();
                }
            }
        }

        private async Task<bool> ExtractHeadersAsync()
        {
            if (Headers.Count == 0 && Metadata!.Count > 0)
            {
                var hdrs = new List<TableCell>();

                foreach (var group in Metadata)
                {
                    foreach (AutoInputMetadata autoInput in group.Items)
                    {
                        var attr = autoInput.Attribute;
                        var text = attr.Prompt ?? autoInput.GetDisplayName();

                        if (text.IsBlank())
                            text = autoInput.PropertyAsDisplayName();

                        var hdr = new TableCell
                        {
                            Icon = attr.Icon,
                            Text = text,
                            Description = attr.Description,
                            Name = attr.GetProperty().Name,
                        };

                        hdrs.Add(hdr);
                    }
                }
                
                if (OnHeadersSet.HasDelegate)
                    await OnHeadersSet.InvokeAsync(hdrs);

                Headers = hdrs.AsReadOnly();
                return true;
            }

            return false;
        }

        private void CreateHeaderClickEventHandlers()
        {
            foreach (var hdr in Headers)
            {
                if (hdr.Hidden || hdr.NonClickable) continue;

                hdr.OnClick = EventCallback.Factory.Create<MouseEventArgs>(this,
                    async e => await HandleHeaderClickAsync(e, hdr));

                if (string.IsNullOrWhiteSpace(hdr.CssClass))
                    hdr.CssClass = "clickable";
            }
        }

        private async Task HandleHeaderClickAsync(MouseEventArgs e, TableCell header)
        {
            if (!EnableHeaderClick) 
                return;

            if (EnableSorting)
            {
                if (AllowMultiHeaderSorting)
                {
                }
                else
                {
                    ClearSortingExceptFor(header);
                }

                if (header.SortAscending == null)
                {
                    // sort ascending
                    header.SortAscending = true;
                }
                else if (header.SortAscending.Value == true)
                {
                    // sort descending
                    header.SortAscending = false;
                }
                else
                {
                    // no sorting
                    header.SortAscending = null;
                }
            }
            
            if (OnHeaderClicked.HasDelegate)
            {
                await OnHeaderClicked.InvokeAsync(new TableCellClickEventArgs(
                    header.Name!,
                    header.SortAscending,
                    EnableSorting,
                    e,
                    this));
            }
            
            StateHasChanged();
        }

        private void ClearSortingExceptFor(TableCell header)
        {
            foreach (var hdr in Headers)
            {
                if (!Equals(header, hdr)) hdr.SortAscending = null;
            }
        }

        #endregion
    }
}
