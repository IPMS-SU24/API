using FluentValidation;
using IPMS.Business.Requests.Authentication;
using IPMS.DataAccess.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace IPMS.API.Validators.Authentication
{
    public class AddLecturerAccountValidator : AbstractValidator<AddLecturerAccountRequest>
    {
        public AddLecturerAccountValidator(UserManager<IPMSUser> userManager)
        {
            RuleFor(x => x.Phone)
                .MustAsync(async (x, cancellationToken) => await userManager.Users.Where(i => i.PhoneNumber == x).FirstOrDefaultAsync() == null)
                .WithMessage("Phone is existed!")
                .Matches(new Regex(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$")).WithMessage("Phone is not valid!");

            RuleFor(x => x.Email).EmailAddress()
                .MustAsync(async (x, cancellationToken) => await userManager.FindByEmailAsync(x) == null)
                .WithMessage("Email is existed!");
        }
    }
}
