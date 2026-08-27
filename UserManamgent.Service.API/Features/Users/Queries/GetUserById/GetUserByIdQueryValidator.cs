using FluentValidation;
using Welco.Shared.Localization;

namespace UserManamgent.Service.API.Features.Users.Queries.GetUserById
{
    public class GetUserByIdQueryValidator : AbstractValidator<GetUserByIdQuery>
    {
        public GetUserByIdQueryValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage(LocalizationKeys.UserManagement.UserIdRequired);
        }
    }
}
