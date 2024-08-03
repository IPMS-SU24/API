
namespace IPMS.Business.Responses.Admin
{
    public class GetSyllabusDetailResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public string Description { get; set; }
        public List<SysAssessmentInfo> AssessmentInfos { get; set; } = new();
        public List<SysSemesterInfo> SemesterInfos { get; set; } = new();
    }

    public class SysAssessmentInfo { 
        public Guid AssessmentId { get; set; }
        public string Name { get; set; }
        public int Order { get; set; }
        public decimal Percentage { get; set; }
    }

    public class SysSemesterInfo
    {
        public Guid SemesterId { get; set; }
        public string Name { get; set; }
    }

}
