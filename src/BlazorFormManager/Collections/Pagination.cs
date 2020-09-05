using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BlazorFormManager.Collections
{
    /// <summary>
    /// Represents an object that indicates a series of related content exists across multiple pages.
    /// </summary>
    public class Pagination
    {
        #region fields
        
        private int _currentPage;
        private int _pageSize;
        private int _totalItems;

        #endregion

        #region constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="Pagination"/> class.
        /// </summary>
        public Pagination()
        {
        }

        #endregion

        #region properties

        /// <summary>
        /// Gets or sets a one-based integer representing the current page number
        /// </summary>
        public int CurrentPage
        {
            get => _currentPage; 
            set
            {
                if (_currentPage != value)
                {
                    if (value < 1)
                        throw new ArgumentOutOfRangeException(nameof(CurrentPage),
                            $"{nameof(CurrentPage)} cannot be less than 1.");
                    SetPage(value);
                }
            }
        }

        /// <summary>
        /// Gets or sets the maximum number of items per page.
        /// </summary>
        public int PageSize
        {
            get => _pageSize;
            set
            {
                if (_pageSize != value)
                {
                    if (value < 1)
                        throw new ArgumentOutOfRangeException(nameof(PageSize), 
                            $"{nameof(PageSize)} cannot be less than 1.");
                    _pageSize = value;
                    SetPage(CurrentPage);
                }
            }
        }

        /// <summary>
        /// Gets or sets the total number of items.
        /// </summary>
        public int TotalItems
        {
            get => _totalItems;
            set
            {
                if (_totalItems != value)
                {
                    if (value < 0)
                        throw new ArgumentOutOfRangeException(nameof(TotalItems),
                            $"{nameof(TotalItems)} cannot be less than 0.");

                    _totalItems = value;
                    SetPage(CurrentPage);
                }
            }
        }

        /// <summary>
        /// Gets or sets the event callback to invoke when the current page or related property changes.
        /// </summary>
        public EventCallback<int> OnPageChange { get; set; }

        /// <summary>
        /// Calculates the number of pages.
        /// </summary>
        public int PageCount => Convert.ToInt32(Math.Ceiling((double)TotalItems / PageSize));

        /// <summary>
        /// Indicates whether there are more than one page.
        /// </summary>
        public bool IsMultiPage => PageCount > 1;

        /// <summary>
        /// Gets the navigation links as a collection of integers.
        /// </summary>
        public IReadOnlyCollection<int> Links { get; protected set; }

        #endregion

        #region methods

        /// <summary>
        /// Initializes this <see cref="Pagination"/> instance with the specified parameters.
        /// </summary>
        /// <param name="onPageChange">The event callback to invoke when the current page or related property changes.</param>
        /// <param name="pageSize">The maximum number of items per page.</param>
        /// <param name="currentPage">A one-based integer representing the current page number.</param>
        /// <param name="totalItems">The total number of items.</param>
        public void Initialize(EventCallback<int> onPageChange, int pageSize, int currentPage, int totalItems)
        {
            PageSize = pageSize;
            TotalItems = totalItems;
            OnPageChange = onPageChange;
            SetPage(currentPage);
        }

        /// <summary>
        /// Indicates whether the specified page is the current.
        /// </summary>
        /// <param name="page">The page number to check.</param>
        /// <returns></returns>
        public bool IsActive(int page) => page == CurrentPage;

        /// <summary>
        /// Generates a collection of integers representing the navigation links.
        /// </summary>
        protected virtual void GenerateLinks()
        {
            if (IsMultiPage)
            {
                var count = PageCount;
                var pages = new[] { 1, 2 }
                    .Concat(Enumerable.Range(CurrentPage - 2, 5))
                    .Concat(new[] { count - 1, count })
                    .Where(n => n >= 1 && n <= count)
                    .Distinct();
                Links = new List<int>(pages).AsReadOnly();
            }
            else
            {
                Links = new int[0];
            }
        } 

        #endregion

        #region helpers

        private void SetPage(int page)
        {
            _currentPage = page;
            GenerateLinks();

            if (OnPageChange.HasDelegate)
                OnPageChange.InvokeAsync(page);
        }

        #endregion
    }
}
