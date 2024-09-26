

using System.ComponentModel.DataAnnotations;

namespace IPMS.Business.Common.Enums
{
    public enum AssessmentStatus
    {
        [Display(Name = "Not Yet")]
        NotYet = 0,
        [Display(Name = "In Progress")]
        InProgress = 1,
        [Display(Name = "Done")]
        Done = 2,
        [Display(Name = "Expired")]
        Expired = -1,
        [Display(Name = "Not Available")]
        NotAvailable = -99
    }
}
