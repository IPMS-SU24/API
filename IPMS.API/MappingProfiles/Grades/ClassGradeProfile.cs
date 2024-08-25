using AutoMapper;
using IPMS.Business.Models;
using IPMS.Business.Responses.ProjectSubmission;

namespace IPMS.API.MappingProfiles.Grades
{
    public class ClassGradeProfile : Profile
    {
        public ClassGradeProfile()
        {
            CreateMap<SubmissionGrade, ExportSubmissionGrade>();
            CreateMap<AssessmentGrade, ExportAssessmentGrade>();
            CreateMap<GetGradeResponse, ClassGradeDataRow>()
                .ForMember(dest=>dest.Contribute, src=>src.Ignore())
                .ForMember(dest=>dest.Final, src=>src.Ignore());
        }
    }
}
