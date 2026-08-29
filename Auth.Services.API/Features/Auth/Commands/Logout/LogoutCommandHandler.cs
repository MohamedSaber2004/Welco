using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Domain.Models;
using Welco.Shared.Localization;
using Welco.Shared.Results;

namespace Auth.Services.API.Features.Auth.Commands.Logout
{
    public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result<string>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LogoutCommandHandler(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Result<string>> Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            var refreshRepo = _unitOfWork.GetRepository<UserRefreshToken, Guid>();

            // 1. If a specific refresh token is provided, revoke it
            if (!string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                var tokenEntity = await refreshRepo.GetFirstAsync(r => r.Token == request.RefreshToken && !r.IsRevoked, cancellationToken);
                if (tokenEntity != null)
                {
                    tokenEntity.Revoke();
                    refreshRepo.Update(tokenEntity);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }
            }

            // 2. If authenticated user identity is present, revoke active tokens for that user
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrWhiteSpace(userIdClaim) && Guid.TryParse(userIdClaim, out var userId))
            {
                var activeTokens = await refreshRepo.GetAllListAsync(r => r.UserId == userId && !r.IsRevoked, cancellationToken);
                if (activeTokens.Any())
                {
                    foreach (var token in activeTokens)
                    {
                        token.Revoke();
                        refreshRepo.Update(token);
                    }
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }
            }

            return Result<string>.Success(string.Empty, LocalizationKeys.Auth.LogoutSuccess);
        }
    }
}
