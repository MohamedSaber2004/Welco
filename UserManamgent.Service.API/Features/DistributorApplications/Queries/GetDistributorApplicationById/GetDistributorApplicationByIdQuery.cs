using MediatR;
using Welco.Shared.Common.DTOs.UserManagement;
using Welco.Shared.Results;

namespace UserManamgent.Service.API.Features.DistributorApplications.Queries.GetDistributorApplicationById
{
    public class GetDistributorApplicationByIdQuery : IRequest<Result<DistributorApplicationDto>>
    {
        public Guid Id { get; set; }

        public GetDistributorApplicationByIdQuery() { }

        public GetDistributorApplicationByIdQuery(Guid id)
        {
            Id = id;
        }
    }
}
