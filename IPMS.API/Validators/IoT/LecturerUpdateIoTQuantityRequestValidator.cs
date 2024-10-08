﻿using FluentValidation;
using IPMS.API.Common.Extensions;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Requests.IoTComponent;

namespace IPMS.API.Validators.IoT
{
    public class LecturerUpdateIoTQuantityRequestValidator : AbstractValidator<ReviewBorrowIoTComponentsRequest>
    {
        public LecturerUpdateIoTQuantityRequestValidator(IBorrowIoTService _borrowIoTService, IHttpContextAccessor context)
        {
            var lecturerId = context.HttpContext.User.Claims.GetUserId();
            RuleFor(x => x.ProjectId).NotEmpty();
            RuleFor(x => x.CreatedAt).NotEmpty();
            RuleFor(x => x).CustomAsync(async (x, validationContext, cancellationToken) =>
            {
                var validationResult = await _borrowIoTService.ReviewBorrowIoTComponentsValidators(x, lecturerId);
                if (!validationResult.Result)
                {
                    validationContext.AddBusinessFailure(validationResult.Message);
                }
            });
        }
    }
}
