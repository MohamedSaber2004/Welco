using MediatR;
using Microsoft.EntityFrameworkCore;
using Welco.Shared.Common.DTOs.UserManagement;
using Welco.Shared.Common.Extensions;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Domain.Models;
using Welco.Shared.Localization;
using Welco.Shared.Results;

namespace UserManamgent.Service.API.Features.DistributorApplications.Queries.GetDistributorApplications
{
    public class GetDistributorApplicationsQueryHandler : IRequestHandler<GetDistributorApplicationsQuery, PaginatedResult<DistributorApplicationDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetDistributorApplicationsQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PaginatedResult<DistributorApplicationDto>> Handle(GetDistributorApplicationsQuery request, CancellationToken cancellationToken)
        {
            var repo = _unitOfWork.GetRepository<DistributorApplication, Guid>();
            var query = repo.GetAll(a => !a.IsDeleted).Include(a => a.Country).AsNoTracking();

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var term = request.SearchTerm.Trim().ToLower();
                query = query.Where(a =>
                    a.CompanyName.ToLower().Contains(term) ||
                    a.ContactPerson.ToLower().Contains(term) ||
                    a.ContactEmail.ToLower().Contains(term));
            }

            if (request.Status.HasValue)
            {
                query = query.Where(a => a.Status == request.Status.Value);
            }

            return await query.OrderByDescending(a => a.CreatedAt).ToPaginatedListAsync(a => new DistributorApplicationDto
            {
                Id = a.Id,
                CompanyName = a.CompanyName,
                CountryId = a.CountryId,
                CountryNameEn = a.Country != null ? a.Country.NameEn : null,
                SalesVolumeBand = a.SalesVolumeBand,
                Website = a.Website,
                ContactPerson = a.ContactPerson,
                ContactEmail = a.ContactEmail,
                Status = a.Status.ToString(),
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt
            }, request.PageNumber, request.PageSize, LocalizationKeys.DistributorApplication.ListFetched, cancellationToken);
        }
    }
}
