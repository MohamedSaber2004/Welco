using MediatR;
using Welco.Shared.Common.DTOs.Content;
using Welco.Shared.Results;

namespace Content.Services.API.Features.TradeShows.Commands.CreateTradeShow
{
    public class CreateTradeShowCommand : IRequest<Result<TradeShowEventDto>>
    {
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
