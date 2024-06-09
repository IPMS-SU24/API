using AutoFilterer.Attributes;
using AutoFilterer.Enums;
using IPMS.Business.Pagination;
using Microsoft.AspNetCore.Mvc;

namespace IPMS.Business.Requests.IoTComponent
{
    public class GetIoTComponentRequest : BasePaginationAutoFiltererRequest
    {

        [ToLowerContainsComparison]
        [StringFilterOptions(StringFilterOption.Contains, StringComparison.InvariantCultureIgnoreCase)]
        [FromQuery(Name = "searchValue")]
        public string? Name {  get; set; }
    }
}
