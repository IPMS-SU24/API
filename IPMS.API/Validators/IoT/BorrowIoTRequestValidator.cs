using FluentValidation;
using IPMS.API.Common.Extensions;
using IPMS.Business.Common.Utils;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Requests.IoTComponent;

namespace IPMS.API.Validators.IoT
{
    public class BorrowIoTListRequestValidator : AbstractValidator<List<IoTModelRequest>>
    {
        public BorrowIoTListRequestValidator(IBorrowIoTService borrowIoTService, IHttpContextAccessor context)
        {
            var modelValidator = new BorrowIoTRequestValidator(borrowIoTService, context);
            RuleForEach(x => x).SetValidator(modelValidator);
        }
    }
    public class BorrowIoTRequestValidator : AbstractValidator<IoTModelRequest>
    {
        private readonly IBorrowIoTService _borrowIoTService;
        public BorrowIoTRequestValidator(IBorrowIoTService borrowIoTService, IHttpContextAccessor context)
        {
            _borrowIoTService = borrowIoTService;
            var leaderId = context.HttpContext.User.Claims.GetUserId();
            RuleFor(x => x).MustAsync(async (x, cancellationToken) => await _borrowIoTService.CheckIoTValid(x, leaderId)).WithMessage("IoT Component Not Valid");
        }
    }
}
