using MediatR;
using Welco.Shared.Common.DTOs.Content;
using Welco.Shared.Results;

namespace Content.Services.API.Features.FAQs.Queries.GetFAQs
{
    public class GetFAQsQuery : IRequest<Result<List<FAQItemDto>>>
    {
    }
}
