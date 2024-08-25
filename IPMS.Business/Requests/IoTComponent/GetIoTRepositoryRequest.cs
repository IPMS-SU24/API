using AutoFilterer.Attributes;
using AutoFilterer.Enums;
using AutoFilterer.Types;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text.Json.Serialization;

namespace IPMS.Business.Requests.IoTComponent
{
    public class GetIoTRepositoryRequest : FilterBase
    {
        public IoTComponentFilter Component => new IoTComponentFilter
        {
            SearchValue = SearchValue
        };

        public string? SearchValue { get; set; }
        [IgnoreFilter]
        [FromQuery(Name = "page")]
        public int Page { get; set; } = 1;
        [IgnoreFilter]
        [FromQuery(Name = "pageSize")]
        public int PageSize { get; set; } = 10;
    }

    public class IoTComponentFilter : FilterBase
    {
        [CompareTo(nameof(DataAccess.Models.IoTComponent.Name), nameof(DataAccess.Models.IoTComponent.Description), CombineWith = CombineType.Or)]
        [ToLowerContainsComparison]
        [StringFilterOptions(StringFilterOption.Contains, StringComparison.InvariantCultureIgnoreCase)]
        [BindNever]
        public string? SearchValue { get; init; }
    }
}
