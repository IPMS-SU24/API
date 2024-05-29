using Microsoft.OpenApi.Attributes;
using System.ComponentModel;

namespace IPMS.Business.Common.Enums
{
    public enum AssessmentStatus
    {
        [Display("Not Yet")]
        NotYet = 0,
        [Display("In Progress")]
        InProgress = 1,
        [Display("Done")]
        Done = 2,
        [Display("Expired")]
        Expired = -1
    }
}
