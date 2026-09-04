using MediatR;
using Welco.Shared.Common.DTOs.Content;
using Welco.Shared.Results;

namespace Content.Services.API.Features.SupportContact.Queries.GetSupportContact
{
    public class GetSupportContactQuery : IRequest<Result<SupportContactDto>>
    {
    }
}
