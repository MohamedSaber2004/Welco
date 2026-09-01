using MediatR;
using Welco.Shared.Common.DTOs.Content;
using Welco.Shared.Results;

namespace Content.Services.API.Features.HelpCategories.Commands.UpdateHelpCategory
{
    public class UpdateHelpCategoryCommand : IRequest<Result<HelpCategoryDto>>
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Icon { get; set; }
    }
}
