using System;

namespace Carfamsoft.Model2View.Shared.Collections
{
    /// <summary>
    /// Represents an object that indicates a series of related content exists across multiple pages.
    /// </summary>
    public sealed class ItemsPagination : PaginationBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ItemsPagination"/> class.
        /// </summary>
        public ItemsPagination()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemsPagination"/>
        /// class using the specified parameters.
        /// </summary>
        /// <param name="totalItemCount">The total number of items.</param>
        /// <param name="page">A one-based integer representing the current page number.</param>
        /// <param name="pageSize">The maximum number of items per page.</param>
        public ItemsPagination(int totalItemCount, int? page = null, int pageSize = 10)
        {
            Update(totalItemCount, pageSize, page ?? 1);
        }

        /// <inheritdoc/>
        public override void Update(int totalItemCount, int pageSize, int currentPage = 1)
        {
            //if (currentPage < 1) currentPage = 1;
            
            var totalPages = GetPageCount(totalItemCount, pageSize);
            var adjusted = Math.Min(VisibleLinkCount, totalPages);
            var half = (int)Math.Floor(adjusted / 2d);
            var start = Math.Max(currentPage - half, 1);
            var finish = Math.Min(currentPage + half, totalPages);

            if (start <= 1) { start = 1; finish = adjusted; }
            if (finish >= totalPages) { start = totalPages - adjusted; }
            if (start <= 1) { start = 1; }

            StartPage = start;
            EndPage = finish;

            base.Update(totalItemCount, pageSize, currentPage);
        }

        /// <summary>
        /// Gets the start page number.
        /// </summary>
        public int StartPage { get; private set; }

        /// <summary>
        /// Gets the end page number.
        /// </summary>
        public int EndPage { get; private set; }
    }
}
