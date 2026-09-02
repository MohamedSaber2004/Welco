using MediatR;
using Welco.Shared.Common.DTOs.Content;
using Welco.Shared.Results;

namespace Content.Services.API.Features.TradeShows.Queries.GetTradeShows
{
    public class GetTradeShowsQuery : IRequest<Result<List<TradeShowEventDto>>>
    {
        public bool? UpcomingOnly { get; set; }
    }
}
