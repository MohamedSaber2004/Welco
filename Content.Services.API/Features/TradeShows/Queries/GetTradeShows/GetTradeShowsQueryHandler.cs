using MediatR;
using Microsoft.EntityFrameworkCore;
using Welco.Shared.Common.DTOs.Content;
using Welco.Shared.Domain.Models;
using Welco.Shared.Localization;
using Welco.Shared.Persistance;
using Welco.Shared.Results;

namespace Content.Services.API.Features.TradeShows.Queries.GetTradeShows
{
    public class GetTradeShowsQueryHandler : IRequestHandler<GetTradeShowsQuery, Result<List<TradeShowEventDto>>>
    {
        private readonly WelcoDbContext _db;
        public GetTradeShowsQueryHandler(WelcoDbContext db) => _db = db;

        public async Task<Result<List<TradeShowEventDto>>> Handle(GetTradeShowsQuery request, CancellationToken ct)
        {
            var q = _db.TradeShowEvents.AsNoTracking().Where(x => !x.IsDeleted);
            if (request.UpcomingOnly == true) q = q.Where(x => x.StartDate >= DateTime.UtcNow.Date);
            var list = await q.OrderBy(x => x.StartDate).Select(x => new TradeShowEventDto
            {
                Id = x.Id,
                Name = x.Name,
                Location = x.Location,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                CreatedAt = x.CreatedAt
            }).ToListAsync(ct);
            return Result<List<TradeShowEventDto>>.Success(list, LocalizationKeys.TradeShow.ListFetched);
        }
    }
}
