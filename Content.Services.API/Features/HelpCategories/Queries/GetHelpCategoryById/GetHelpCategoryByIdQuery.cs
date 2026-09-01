using MediatR;
using Welco.Shared.Common.DTOs.Content;
using Welco.Shared.Results;

namespace Content.Services.API.Features.HelpCategories.Queries.GetHelpCategoryById
{
    public class GetHelpCategoryByIdQuery : IRequest<Result<HelpCategoryDto>>
    {
        public Guid Id { get; set; }
    }
}
