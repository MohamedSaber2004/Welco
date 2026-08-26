using FluentValidation;

namespace Auth.Services.API.Features.Auth.Queries.GetUserProfile
{
    public class GetUserProfileQueryValidator : AbstractValidator<GetUserProfileQuery>
    {
        public GetUserProfileQueryValidator()
        {
        }
    }
}
