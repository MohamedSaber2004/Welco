using FluentValidation;
using Welco.Shared.Localization;

namespace UserManamgent.Service.API.Features.Users.Queries.GetUsers
{
    public class GetUsersQueryValidator : AbstractValidator<GetUsersQuery>
    {
        public GetUsersQueryValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThanOrEqualTo(1).WithMessage(LocalizationKeys.UserManagement.PageNumberPositive);

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 50).WithMessage(LocalizationKeys.UserManagement.PageSizeRange);

            RuleFor(x => x.UserType)
                .IsInEnum().WithMessage(LocalizationKeys.UserManagement.UserTypeRequired)
                .When(x => x.UserType.HasValue);
        }
    }
}
