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
        public static IDictionary<string, string[]> ToDictionary(this ValidationResult validationResult)
        {
            return validationResult.Errors
              .GroupBy(x => x.PropertyName)
              .ToDictionary(
                g => g.Key,
                g => g.Select(x => x.ErrorMessage).ToArray()
              );
        }
    }
}
