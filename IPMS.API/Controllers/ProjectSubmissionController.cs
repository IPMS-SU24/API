﻿using IPMS.API.Common.Attributes;
using IPMS.API.Common.Enums;
using IPMS.API.Common.Extensions;
using IPMS.API.Responses;
using IPMS.Business.Common.Enums;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Requests.ProjectSubmission;
using IPMS.Business.Responses.ProjectSubmission;
using Microsoft.AspNetCore.Mvc;

namespace IPMS.API.Controllers
{
    public class ProjectSubmissionController : ApiControllerBase
    {
        private readonly IProjectSubmissionService _projectSubmissionService;
        public ProjectSubmissionController(IProjectSubmissionService projectSubmissionService)
        {
            _projectSubmissionService = projectSubmissionService;
        }
        [EnumAuthorize(UserRole.Student)]
        [HttpPut]
        public async Task<IActionResult> UpdateProjectSubmissionName([FromBody] UpdateProjectSubmissionRequest request)
        {
            Guid currentUserId = HttpContext.User.Claims.GetUserId();
            var response = new IPMSResponse<bool>
            {
                Data = await _projectSubmissionService.UpdateProjectSubmission(request, currentUserId)

            };

            if (response.Data == false)
            {
                response.Status = ResponseStatus.BadRequest;
            }
            return GetActionResponse(response);
        }

        [EnumAuthorize(UserRole.Student)]
        [HttpGet("grades/{projectId}")]
        public async Task<IActionResult> GetGrade(Guid projectId)
        {
            Guid studentId = HttpContext.User.Claims.GetUserId();
            var response = new IPMSResponse<GetGradeResponse>
            {
                Data = await _projectSubmissionService.GetGradeAsync(studentId, projectId)

            };
            return GetActionResponse(response);
        }
        /***
         * Lecturer grade for submission
         * https://docs.google.com/spreadsheets/d/1nReV4aDfzD6r96pK9GTiDFUWI-cVFpX441ES8n855y8/edit?gid=0#gid=0
         * ***/
        [EnumAuthorize(UserRole.Lecturer)]
        [HttpPut("grade")]
        public async Task<IActionResult> GradeSubmission([FromBody] GradeSubmissionRequest request)
        {
            Guid currentUserId = HttpContext.User.Claims.GetUserId();
            await _projectSubmissionService.GradeSubmission(request, currentUserId);
            return GetActionResponse(new IPMSResponse<object>());
        }


        /// <summary>
        /// Get all submission in project of current user
        /// https://docs.google.com/spreadsheets/d/10eDAKGeT4Na1yPKZc3QiA6lA6ll7YHxKLWjGjOtLTNY/edit#gid=0
        /// </summary>
        [EnumAuthorize(UserRole.Student)]
        [HttpGet]
        public async Task<IActionResult> GetAllSubmission([FromQuery] GetAllSubmissionRequest request)
        {
            Guid currentUserId = HttpContext.User.Claims.GetUserId();
            var data = await _projectSubmissionService.GetAllSubmission(request, currentUserId);
            var response = await data.GetPaginatedResponse(page: request.Page, pageSize: request.PageSize);
            return GetActionResponse(response);
        }

        /// <summary>
        /// Get all classes need to grade of lecturer
        /// https://docs.google.com/spreadsheets/d/1uOCNgK3DZm9hK-3TQy3g12mwmV_ancrsMvyygVGxZeY/edit?gid=0#gid=0
        /// </summary>
        [EnumAuthorize(UserRole.Lecturer)]
        [HttpGet("classes-committee")]
        public async Task<IActionResult> GetClassesCommittee([FromQuery] GetClassesCommitteeRequest request)
        {
            Guid currentUserId = HttpContext.User.Claims.GetUserId();
            var data = await _projectSubmissionService.GetClassesCommittee(request, currentUserId);
            var response = await data.GetPaginatedResponse(page: request.Page, pageSize: request.PageSize);
            return GetActionResponse(response);
        }

        /// <summary>
        /// Get all classes need to grade of lecturer
        /// https://docs.google.com/spreadsheets/d/1uOCNgK3DZm9hK-3TQy3g12mwmV_ancrsMvyygVGxZeY/edit?gid=0#gid=0
        /// </summary>
        [EnumAuthorize(UserRole.Lecturer)]
        [HttpGet("final-assessment")]
        public async Task<IActionResult> GetFinalAssessment([FromQuery] GetFinalAssessmentRequest request)
        {
            Guid currentUserId = HttpContext.User.Claims.GetUserId();
            var data = await _projectSubmissionService.GetFinalAssessment(request, currentUserId);
            var response = await data.GetPaginatedResponse(page: request.Page, pageSize: request.PageSize);
            return GetActionResponse(response);
        }
    }
}
