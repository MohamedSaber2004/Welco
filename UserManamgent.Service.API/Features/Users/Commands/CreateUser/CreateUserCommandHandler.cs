using MediatR;
using Microsoft.AspNetCore.Identity;
using Welco.Shared.Common.DTOs.UserManagement;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Domain.Models;
using Welco.Shared.Enums;
using Welco.Shared.Localization;
using Welco.Shared.Results;

namespace UserManamgent.Service.API.Features.Users.Commands.CreateUser
{
    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<UserDto>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICurrentUserService _currentUserService;

        public CreateUserCommandHandler(
            UserManager<ApplicationUser> userManager,
            ICurrentUserService currentUserService)
        {
            _userManager = userManager;
            _currentUserService = currentUserService;
        }

        public async Task<Result<UserDto>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return Result<UserDto>.Conflict(LocalizationKeys.UserManagement.UserAlreadyExists);
            }

            var currentUserId = _currentUserService.UserId != Guid.Empty
                ? _currentUserService.UserId.ToString()
                : "System";

            var user = new ApplicationUser
            {
                FullName = request.FullName,
                Email = request.Email,
                UserName = request.Email,
                PhoneNumber = request.PhoneNumber,
                UserType = request.UserType,
                Language = AppLanguage.En,
                ProfilePictureName = request.ProfilePictureName,
                CompanyId = request.CompanyId,
                IsActive = request.IsActive,
                EmailConfirmed = true
            };

            user.MarkAsCreated(currentUserId);

            var createResult = await _userManager.CreateAsync(user, request.Password);
            if (!createResult.Succeeded)
            {
                var errors = createResult.Errors.Select(e => e.Description).ToList();
                return Result<UserDto>.BadRequest(
                    errors.FirstOrDefault() ?? LocalizationKeys.ExceptionMessages.BadRequest,
                    errors);
            }

            var roleName = request.UserType.ToString();
            await _userManager.AddToRoleAsync(user, roleName);

            var userDto = new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber,
                ProfilePictureName = user.ProfilePictureName,
                UserType = user.UserType,
                CompanyId = user.CompanyId,
                Language = user.Language,
                IsActive = user.IsActive,
                IsEmailConfirmed = user.EmailConfirmed,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                Roles = new List<string> { roleName }
            };

            return Result<UserDto>.Created(userDto, LocalizationKeys.UserManagement.UserCreated);
        }
    }
}
