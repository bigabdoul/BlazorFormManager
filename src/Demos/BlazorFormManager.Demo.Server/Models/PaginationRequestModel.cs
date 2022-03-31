using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace BlazorFormManager.Demo.Server.Models
{
    public class PaginationRequestModel
    {
        #region static
        
        public static int MinPage { get; set; } = 1;
        public static int MaxPageSize { get; set; } = 100;
        public static int DefaultPageSize { get; set; } = 10; 

        #endregion

        public int? Page { get; set; }
        public int? PageSize { get; set; }
        public bool? Sort { get; set; }
        public string Column { get; set; }
        public string[] Columns { get; set; }
        public bool?[] Sorts { get; set; }
        public string Search { get; set; }
        public int TotalItemCount { get; private set; }

        public IQueryable<T> GetPage<T>(IQueryable<T> query, Expression<Func<T, string>> orderBy = null)
        {
            TotalItemCount = query.Count();
            if (orderBy != null) query = query.OrderBy(orderBy);

            var (page, pageSize) = GetValues();
            return query.Skip((page - 1) * pageSize).Take(pageSize);
        }

        public async Task<T[]> GetPageAsync<T>(IQueryable<T> query, Expression<Func<T, string>> orderBy = null)
        {
            TotalItemCount = await query.CountAsync();
            if (orderBy != null) query = query.OrderBy(orderBy);

            var (page, pageSize) = GetValues();
            return await query.Skip((page - 1) * pageSize).Take(pageSize).ToArrayAsync();
        }

        private (int page, int pageSize) GetValues()
        {
            var page = Page ?? MinPage;
            var pageSize = PageSize ?? DefaultPageSize;

            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = DefaultPageSize;
            else if (pageSize > MaxPageSize) pageSize = MaxPageSize;

            return (page, pageSize);
        }
    }
}
