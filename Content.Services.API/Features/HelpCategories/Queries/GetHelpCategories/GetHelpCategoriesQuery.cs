using MediatR;
using Welco.Shared.Common.DTOs.Content;
using Welco.Shared.Results;

namespace Content.Services.API.Features.HelpCategories.Queries.GetHelpCategories
{
    public class GetHelpCategoriesQuery : IRequest<Result<List<HelpCategoryDto>>>
    {
    }
}
