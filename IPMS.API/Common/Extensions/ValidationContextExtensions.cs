using FluentValidation;
using FluentValidation.Results;
namespace IPMS.API.Common.Extensions
{
    public static class ValidationContextExtensions
    {
        public static void AddBusinessFailure<T>(this ValidationContext<T> validationContext, string message)
        {
            validationContext.AddFailure(new ValidationFailure("Business Error", message));
        }
    }
}
