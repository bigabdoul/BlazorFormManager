using System;
using System.Collections.Generic;
using System.Linq;

namespace Carfamsoft.Model2View.Shared.Collections
{
    /// <summary>
    /// Represents an object that indicates a series of related content exists across multiple pages.
    /// </summary>
    public class Pagination : PaginationBase
    {
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
        /// Gets the navigation links as a collection of integers.
        /// </summary>
        public IReadOnlyCollection<int> Links { get; protected set; }

        #endregion

        #region methods

        /// <summary>
        /// Initializes this <see cref="Pagination"/> instance with the specified parameters.
        /// </summary>
        /// <param name="onPageChange">The event callback to invoke when the current page or related property changes.</param>
        /// <param name="totalItemCount">The total number of items.</param>
        /// <param name="pageSize">The maximum number of items per page.</param>
        /// <param name="currentPage">A one-based integer representing the current page number.</param>
        /// <returns>A reference to the current <see cref="Pagination"/> instance.</returns>
        public Pagination Initialize(Action<int> onPageChange, int totalItemCount, int pageSize, int currentPage = 1)
        {
            OnPageChange = onPageChange;
            Update(totalItemCount, pageSize, currentPage);
            return this;
        }

        /// <inheritdoc/>
        protected override void InvokePageChanged(int value)
        {
            GenerateLinks();
            base.InvokePageChanged(value);
        }

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
                Links = Array.Empty<int>();
            }
        } 

        #endregion
    }
}
