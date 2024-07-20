using FluentValidation;
using IPMS.Business.Requests.Authentication;

namespace IPMS.API.Validators.Authentication
{
    public class ForgotPasswordRequestValidator : AbstractValidator<ForgotPasswordRequest>
    {
        public ForgotPasswordRequestValidator()
        {
            RuleFor(x => x.Email).EmailAddress();
        }
    }
}
