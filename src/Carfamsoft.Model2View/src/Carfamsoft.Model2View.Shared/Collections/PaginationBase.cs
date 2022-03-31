using System;

namespace Carfamsoft.Model2View.Shared.Collections
{
    /// <summary>
    /// Represents the base class for pagination-capable classes.
    /// </summary>
    public abstract class PaginationBase
    {
        #region fields

        private int _currentPage = 1;
        private int _pageSize = 5;
        private int _totalItemCount;

        #endregion

        #region constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="PaginationBase"/> class.
        /// </summary>
        protected PaginationBase()
        {
        }

        #endregion

        #region properties

        /// <summary>
        /// Whether to show page numbers. Default is true.
        /// </summary>
        public virtual bool ShowPageNumbers { get; set; } = true;
        
        /// <summary>
        /// Whether to show the label CurrentPage / PageCount if 
        /// <see cref="ShowPageNumbers"/> is false. Default is true.
        /// </summary>
        public virtual bool ShowPageLabel { get; set; } = true;

        /// <summary>
        /// Whether to show first and last buttons. Default is true.
        /// </summary>
        public virtual bool ShowFirstLast { get; set; } = true;

        /// <summary>
        /// Gets or sets the number of navigation buttons shown.
        /// </summary>
        public virtual int VisibleLinkCount { get; set; } = 10;

        /// <summary>
        /// Gets or sets a one-based integer representing the current page number
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Value cannot be less than 1.</exception>
        public virtual int CurrentPage
        {
            get => _currentPage;
            set
            {
                if (_currentPage != value)
                {
                    ValidateCurrentPage(value);
                    InvokePageChanged(value);
                }
            }
        }

        /// <summary>
        /// Gets or sets the maximum number of items per page.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Value cannot be less than 1.</exception>
        public virtual int PageSize
        {
            get => _pageSize;
            set
            {
                if (_pageSize != value)
                {
                    ValidatePageSize(value);
                    InvokePageSizeChanged(value);
                }
            }
        }

        /// <summary>
        /// Gets or sets the total number of items.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Value cannot be less than 0.</exception>
        public virtual int TotalItemCount
        {
            get => _totalItemCount;
            set
            {
                if (_totalItemCount != value)
                {
                    ValidateTotalItemCount(value);
                }
            }
        }

        /// <summary>
        /// Gets or sets the event callback to invoke when the current page or related property changes.
        /// </summary>
        public virtual Action<int> OnPageChange { get; set; }

        /// <summary>
        /// Gets or sets the event callback to invoke when the page size or related property changes.
        /// </summary>
        public virtual Action<int> OnPageSizeChange { get; set; }

        /// <summary>
        /// Gets the number of pages.
        /// </summary>
        public virtual int PageCount => GetPageCount(TotalItemCount, PageSize);

        /// <summary>
        /// Indicates whether there are more than one page.
        /// </summary>
        public bool IsMultiPage => PageCount > 1;

        /// <summary>
        /// Indicates whether the specified page is the current.
        /// </summary>
        /// <param name="page">The page number to check.</param>
        /// <returns></returns>
        public bool IsActive(int page) => page == CurrentPage;

        /// <summary>
        /// Indicates whether a previous page is available.
        /// </summary>
        public bool HasPrevious => CurrentPage > 1 && PageCount > 1;

        /// <summary>
        /// Indicates whether a next page is available.
        /// </summary>
        public bool HasNext => CurrentPage < PageCount;

        #endregion

        /// <summary>
        /// Update the corresponding properties with the specified parameters.
        /// </summary>
        /// <param name="totalItemCount">The total number of items.</param>
        /// <param name="pageSize">The maximum number of items per page.</param>
        /// <param name="currentPage">A one-based integer representing the current page number.</param>
        public virtual void Update(int totalItemCount, int pageSize, int currentPage = 1)
        {
            ValidateTotalItemCount(totalItemCount);
            ValidatePageSize(pageSize);
            ValidateCurrentPage(currentPage);
        }

        #region helpers

        /// <summary>
        /// Invokes the <see cref="OnPageSizeChange"/> callback delegate.
        /// </summary>
        /// <param name="value">The new page size.</param>
        protected virtual void InvokePageSizeChanged(int value)
        {
            OnPageSizeChange?.Invoke(value);
        }

        /// <summary>
        /// Invokes the <see cref="OnPageChange"/> callback delegate.
        /// </summary>
        /// <param name="value">The new page number.</param>
        protected virtual void InvokePageChanged(int value)
        {
            OnPageChange?.Invoke(value);
        }

        /// <summary>
        /// Makes sure that the current page is not less than 1.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <exception cref="ArgumentOutOfRangeException">When <paramref name="value"/> is less than 1.</exception>
        protected virtual void ValidateCurrentPage(int value)
        {
            /*
            if (value < 1)
                throw new ArgumentOutOfRangeException(nameof(CurrentPage),
                    $"{nameof(CurrentPage)} cannot be less than 1.");
            */
            _currentPage = value;
        }

        /// <summary>
        /// Makes sure the page size is not less than 1.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <exception cref="ArgumentOutOfRangeException">When <paramref name="value"/> is less than 1.</exception>
        protected virtual void ValidatePageSize(int value)
        {
            if (value < 1)
                throw new ArgumentOutOfRangeException(nameof(PageSize),
                    $"{nameof(PageSize)} cannot be less than 1.");

            _pageSize = value;
        }

        /// <summary>
        /// Makes sure that the total item count is not less than 0.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <exception cref="ArgumentOutOfRangeException">When <paramref name="value"/> is less than 0.</exception>
        protected virtual void ValidateTotalItemCount(int value)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(TotalItemCount),
                    $"{nameof(TotalItemCount)} cannot be less than 0.");

            _totalItemCount = value;
        }

        /// <summary>
        /// Calculates the total number of pages.
        /// </summary>
        /// <param name="totalItemCount">The total number of items.</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <returns></returns>
        protected int GetPageCount(int totalItemCount, int pageSize)
        {
            return Convert.ToInt32(Math.Ceiling((double)totalItemCount / pageSize));
        }

        #endregion

    }
}
