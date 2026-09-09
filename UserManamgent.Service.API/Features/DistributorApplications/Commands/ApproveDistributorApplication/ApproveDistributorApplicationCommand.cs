using MediatR;
using Welco.Shared.Common.DTOs.UserManagement;
using Welco.Shared.Results;

namespace UserManamgent.Service.API.Features.DistributorApplications.Commands.ApproveDistributorApplication
{
    public class ApproveDistributorApplicationCommand : IRequest<Result<DistributorApplicationDto>>
    {
        public Guid Id { get; set; }
        public Guid? AccountManagerId { get; set; }
    }
}
