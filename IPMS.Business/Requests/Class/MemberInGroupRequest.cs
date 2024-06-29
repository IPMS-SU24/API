﻿using AutoFilterer.Attributes;
using AutoFilterer.Enums;
using AutoFilterer.Types;
using IPMS.DataAccess.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace IPMS.Business.Requests.Class
{
    public class MemberInGroupRequest : FilterBase
    {
        [JsonIgnore]
        public MemberGroupFilterer? Students => new MemberGroupFilterer
        {
            ProjectId = GroupFilter,
            ClassId = ClassId,
        };
        [IgnoreFilter]
        public Guid ClassId { get; set; }
        [IgnoreFilter]
        public ICollection<Guid>? GroupFilter { get; set; }
        [CompareTo(nameof(IPMSUser.UserName),nameof(IPMSUser.FullName),CombineWith = CombineType.Or)]
        [ToLowerContainsComparison]
        [StringFilterOptions(StringFilterOption.Contains, StringComparison.InvariantCultureIgnoreCase)]
        public string? SearchValue { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
    public class MemberGroupFilterer : FilterBase
    {
        [ArraySearchFilter]
        public ICollection<Guid>? ProjectId { get; set; }
        public Guid ClassId { get; set; }
    }
}
