using IPMS.API.Responses;
using X.PagedList;

namespace IPMS.API.Common.Extensions
{
    public static partial class QueryExtensions
    {
        public static async Task<PaginationResponse<IEnumerable<T>>> GetPaginatedResponse<T>(this IEnumerable<T> source, int page = 1, int pageSize = 10)
        {
            var paginatedResult = await source.ToPagedListAsync(page, pageSize);
            return new PaginationResponse<IEnumerable<T>>
            {
                PageSize = pageSize,
                TotalPage = paginatedResult.PageCount,
                CurrentPage = page,
                Data = paginatedResult
            };
        }
    }
}