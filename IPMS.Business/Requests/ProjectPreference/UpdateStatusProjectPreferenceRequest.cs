using System;
namespace IPMS.Business.Requests.ProjectPreference
{
    public class UpdateProjectPreferenceStatusRequest
    {
        public List<ProjectPreferenceStatus> Projects { get; set; } = new();
    }

    public class ProjectPreferenceStatus { 
        public Guid ProjectId { get; set; }
        public bool IsPublished { get; set; }
    }

}
