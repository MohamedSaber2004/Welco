using MediatR;
using Microsoft.AspNetCore.Identity;
using Welco.Shared.Common.DTOs.Providers;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Domain.Models;
using Welco.Shared.Enums;
using Welco.Shared.Localization;
using Welco.Shared.Results;
using ProviderEntity = Welco.Shared.Domain.Models.Provider;

namespace Provider.Services.API.Features.Providers.Commands.CreateProvider
{
    public class CreateProviderCommandHandler : IRequestHandler<CreateProviderCommand, Result<ProviderDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICurrentUserService _currentUserService;

        public CreateProviderCommandHandler(
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _currentUserService = currentUserService;
        }

        public async Task<Result<ProviderDto>> Handle(CreateProviderCommand request, CancellationToken cancellationToken)
        {
            var email = request.Email.Trim();

            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
            {
                return Result<ProviderDto>.Conflict(LocalizationKeys.UserManagement.UserAlreadyExists);
            }

            var providerRepo = _unitOfWork.GetRepository<ProviderEntity, Guid>();

            if (!string.IsNullOrWhiteSpace(request.CommercialRegistrationNumber))
            {
                var crnExists = await providerRepo.ExistsAsync(
                    p => !p.IsDeleted && p.CommercialRegistrationNumber != null
                        && p.CommercialRegistrationNumber.ToLower() == request.CommercialRegistrationNumber.Trim().ToLower(),
                    cancellationToken);

                if (crnExists)
                {
                    return Result<ProviderDto>.Conflict(LocalizationKeys.Provider.CommercialRegistrationNumberAlreadyExists);
                }
            }

            var currentUserId = _currentUserService.UserId != Guid.Empty
                ? _currentUserService.UserId.ToString()
                : "System";

            await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                var user = new ApplicationUser
                {
                    FullName = !string.IsNullOrWhiteSpace(request.ContactPersonName)
                        ? request.ContactPersonName.Trim()
                        : request.CommercialName.Trim(),
                    Email = email,
                    UserName = email,
                    PhoneNumber = request.Phone,
                    UserType = UserType.Provider,
                    Language = AppLanguage.En,
                    IsActive = true,
                    EmailConfirmed = true
                };

                user.MarkAsCreated(currentUserId);

                var createResult = await _userManager.CreateAsync(user, request.Password);
                if (!createResult.Succeeded)
                {
                    var errors = createResult.Errors.Select(e => e.Description).ToList();
                    await _unitOfWork.RollbackAsync(cancellationToken);
                    return Result<ProviderDto>.BadRequest(
                        errors.FirstOrDefault() ?? LocalizationKeys.ExceptionMessages.BadRequest,
                        errors);
                }

                await _userManager.AddToRoleAsync(user, UserType.Provider.ToString());

                var provider = ProviderEntity.Create(
                    request.CommercialName,
                    request.CompanyName,
                    request.CommercialRegistrationNumber,
                    request.ContactPersonName,
                    request.ContactPersonPhone,
                    request.Phone,
                    email,
                    request.Address,
                    request.Notes,
                    request.ImageName,
                    user.Id,
                    currentUserId);

                await providerRepo.AddAsync(provider, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitAsync(cancellationToken);

                return Result<ProviderDto>.Created(ToDto(provider), LocalizationKeys.Provider.Created);
            }
            catch
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
                throw;
            }
        }

        private static ProviderDto ToDto(ProviderEntity provider)
        {
            return new ProviderDto
            {
                Id = provider.Id,
                CommercialName = provider.CommercialName,
                CompanyName = provider.CompanyName,
                CommercialRegistrationNumber = provider.CommercialRegistrationNumber,
                ContactPersonName = provider.ContactPersonName,
                ContactPersonPhone = provider.ContactPersonPhone,
                Phone = provider.Phone,
                Email = provider.Email,
                Address = provider.Address,
                Notes = provider.Notes,
                ImageName = provider.ImageName,
                OwnerUserId = provider.OwnerUserId,
                IsActive = provider.IsActive,
                CreatedAt = provider.CreatedAt,
                UpdatedAt = provider.UpdatedAt
            };
        }
    }
}
