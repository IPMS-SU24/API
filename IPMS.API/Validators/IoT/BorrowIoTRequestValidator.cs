using FluentValidation;
using IPMS.API.Common.Extensions;
using IPMS.Business.Common.Utils;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Requests.IoTComponent;

namespace IPMS.API.Validators.IoT
{
    public class BorrowIoTListRequestValidator : AbstractValidator<List<BorrowIoTModelRequest>>
    {
        public BorrowIoTListRequestValidator(IBorrowIoTService borrowIoTService, IHttpContextAccessor context, ICommonServices commonServices)
        {
            var modelValidator = new BorrowIoTRequestValidator(commonServices, borrowIoTService, context);
            RuleForEach(x => x).SetValidator(modelValidator);
        }
    }
    public class BorrowIoTRequestValidator : AbstractValidator<BorrowIoTModelRequest>
    {
        private readonly IBorrowIoTService _borrowIoTService;
        private readonly ICommonServices _commonServices;
        public BorrowIoTRequestValidator(ICommonServices commonServices, IBorrowIoTService borrowIoTService, IHttpContextAccessor context)
        {
            _borrowIoTService = borrowIoTService;
            _commonServices = commonServices;
            var leaderId = context.HttpContext.User.Claims.GetUserId();
            RuleFor(x => x).MustAsync(async (x, cancellationToken) => await _borrowIoTService.CheckIoTValid(x, leaderId)).WithMessage("IoT Component Not Valid");
        }
    }
}
