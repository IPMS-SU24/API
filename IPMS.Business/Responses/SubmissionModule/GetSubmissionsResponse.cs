
namespace IPMS.Business.Responses.SubmissionModule
{
    public class GetSubmissionsResponse
    {
        public DateTime SubmitDate { get; set; }
        public string DownloadUrl { get; set; }
        public int GroupNum { get; set; }
        public decimal? Grade { get; set; }
        public Guid SubmissionId { get; set; }
        public Guid? GroupId { get; set; }
    }
}
