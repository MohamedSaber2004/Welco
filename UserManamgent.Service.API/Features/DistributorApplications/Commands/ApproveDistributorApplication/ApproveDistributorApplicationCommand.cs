using MediatR;
using Welco.Shared.Common.DTOs.UserManagement;
using Welco.Shared.Results;

namespace UserManamgent.Service.API.Features.DistributorApplications.Commands.ApproveDistributorApplication
{
    public class ApproveDistributorApplicationCommand : IRequest<Result<DistributorApplicationDto>>
    {
        public Guid Id { get; set; }
        public int TierLevel { get; set; } = 1;
        public Guid? AccountManagerId { get; set; }
    }
}
