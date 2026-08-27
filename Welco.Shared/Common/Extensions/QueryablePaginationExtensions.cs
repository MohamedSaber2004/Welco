using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Welco.Shared.Localization;
using Welco.Shared.Results;

namespace Welco.Shared.Common.Extensions
{
    public static class QueryablePaginationExtensions
    {
        public static async Task<PaginatedResult<T>> ToPaginatedListAsync<T>(
            this IQueryable<T> source,
            int pageNumber,
            int pageSize,
            string message = LocalizationKeys.ActionResults.Ok,
            CancellationToken cancellationToken = default)
        {
            var count = await source.CountAsync(cancellationToken);
            pageNumber = pageNumber <= 0 ? 1 : pageNumber;
            pageSize = pageSize <= 0 ? 10 : pageSize;

            var items = await source
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return PaginatedResult<T>.Success(items, count, pageNumber, pageSize, message);
        }

        public static async Task<PaginatedResult<TDestination>> ToPaginatedListAsync<TSource, TDestination>(
            this IQueryable<TSource> source,
            Expression<Func<TSource, TDestination>> selector,
            int pageNumber,
            int pageSize,
            string message = LocalizationKeys.ActionResults.Ok,
            CancellationToken cancellationToken = default)
        {
            var count = await source.CountAsync(cancellationToken);
            pageNumber = pageNumber <= 0 ? 1 : pageNumber;
            pageSize = pageSize <= 0 ? 10 : pageSize;

            var items = await source
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(selector)
                .ToListAsync(cancellationToken);

            return PaginatedResult<TDestination>.Success(items, count, pageNumber, pageSize, message);
        }
    }
}
