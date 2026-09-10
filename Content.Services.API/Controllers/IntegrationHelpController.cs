using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Welco.Shared.Common.Attributes;
using Welco.Shared.Common.DTOs.Integration;
using Welco.Shared.Common.Extensions;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Controllers;
using Welco.Shared.Domain.Models;
using Welco.Shared.Results;

namespace Content.Services.API.Controllers
{
        [ServiceAuth]
    [ApiController]
    [IntegrationRouteName("Help")]
    [Route("api/integration/help")]
    public class IntegrationHelpController : AppControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public IntegrationHelpController(IMediator mediator, IUnitOfWork unitOfWork) : base(mediator)
        {
            _unitOfWork = unitOfWork;
        }

                [HttpGet("articles")]
        public async Task<IActionResult> GetArticles(CancellationToken ct)
        {
            var repo = _unitOfWork.GetRepository<HelpArticle, Guid>();
            var articles = await repo.GetAll(a => !a.IsDeleted)
                .Include(a => a.Category)
                .OrderBy(a => a.Title)
                .Select(a => new ExternalHelpArticleDto
                {
                    Id = a.Id,
                    CategoryId = a.CategoryId,
                    CategoryName = a.Category != null ? a.Category.Name : null,
                    Title = a.Title,
                    Body = a.Body,
                    Slug = a.Slug
                })
                .AsNoTracking()
                .ToListAsync(ct);

            return ToActionResult(Result<List<ExternalHelpArticleDto>>.Success(articles));
        }

                [HttpGet("faqs")]
        public async Task<IActionResult> GetFaqs(CancellationToken ct)
        {
            var repo = _unitOfWork.GetRepository<FAQItem, Guid>();
            var faqs = await repo.GetAll(f => !f.IsDeleted)
                .OrderBy(f => f.SortOrder)
                .Select(f => new ExternalFAQDto
                {
                    Id = f.Id,
                    Question = f.Question,
                    Answer = f.Answer,
                    SortOrder = f.SortOrder
                })
                .AsNoTracking()
                .ToListAsync(ct);

            return ToActionResult(Result<List<ExternalFAQDto>>.Success(faqs));
        }
    }
}
