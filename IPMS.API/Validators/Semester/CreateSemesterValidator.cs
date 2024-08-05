﻿using FluentValidation;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Requests.Admin;

namespace IPMS.API.Validators.IoT
{
    public class CreateSemesterValidator : AbstractValidator<CreateSemesterRequest>
    {
        public CreateSemesterValidator(IAdminService topicService, IHttpContextAccessor context)
        {
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.ShortName).NotEmpty();
            RuleFor(x => x.Description).NotEmpty();
            RuleFor(x => x.StartDate).NotEmpty();
            RuleFor(x => x.EndDate).NotEmpty();
            RuleFor(x => x.SyllabusId).NotEmpty();

            RuleFor(x => x).CustomAsync(async (x, validationContext, cancellationToken) =>
            {
                var validationResult = await topicService.CreateSemesterValidators(x);
                if (!validationResult.Result)
                {
                    validationContext.AddFailure(validationResult.Message);
                }
            });
        }
    }
}
