using MediatR;
using Welco.Shared.Common.DTOs.Content;
using Welco.Shared.Results;
namespace Content.Services.API.Features.OemServices.Queries.GetOemServices
{
    public class GetOemServicesQuery : IRequest<Result<List<OemServiceDto>>> { }
}
