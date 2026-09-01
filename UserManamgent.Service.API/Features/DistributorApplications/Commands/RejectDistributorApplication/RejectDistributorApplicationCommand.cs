using MediatR;
using Welco.Shared.Common.DTOs.UserManagement;
using Welco.Shared.Results;

namespace UserManamgent.Service.API.Features.DistributorApplications.Commands.RejectDistributorApplication
{
    public class RejectDistributorApplicationCommand : IRequest<Result<DistributorApplicationDto>>
    {
        public Guid Id { get; set; }
        public string? Reason { get; set; }
    }
}
