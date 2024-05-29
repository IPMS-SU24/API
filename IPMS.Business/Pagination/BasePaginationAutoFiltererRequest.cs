using AutoFilterer.Attributes;
using AutoFilterer.Types;
using Microsoft.AspNetCore.Mvc;

namespace IPMS.Business.Pagination
{
    public class BasePaginationAutoFiltererRequest : OrderableFilterBase
    {
        [IgnoreFilter]
        [FromQuery(Name = "page")]
        public int Page { get; set; } = 1;
        [IgnoreFilter]
        [FromQuery(Name = "pageSize")]
        public int PageSize { get; set; } = 10;
    }
}
