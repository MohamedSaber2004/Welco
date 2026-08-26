using System.Net;
using Microsoft.EntityFrameworkCore;
using Welco.Shared.Localization;

namespace Welco.Shared.Results
{
    public class PaginatedResult<T> : Result<IReadOnlyList<T>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;

        public PaginatedResult()
        {
            Data = new List<T>();
        }

        public PaginatedResult(
            bool isSuccess,
            IReadOnlyList<T>? data,
            int totalCount,
            int pageNumber,
            int pageSize,
            string message = LocalizationKeys.ActionResults.Ok,
            int statusCode = (int)HttpStatusCode.OK,
            List<string>? errors = null)
            : base(isSuccess, statusCode, message, data, errors)
        {
            TotalCount = totalCount;
            PageNumber = pageNumber <= 0 ? 1 : pageNumber;
            PageSize = pageSize <= 0 ? 10 : pageSize;
            TotalPages = (int)Math.Ceiling(totalCount / (double)PageSize);
            Data = data ?? new List<T>();
        }

        public static PaginatedResult<T> Success(
            IReadOnlyList<T> data,
            int totalCount,
            int pageNumber,
            int pageSize,
            string message = LocalizationKeys.ActionResults.Ok)
        {
            return new PaginatedResult<T>(true, data, totalCount, pageNumber, pageSize, message, (int)HttpStatusCode.OK);
        }

        public static new PaginatedResult<T> Failure(string error, int statusCode = (int)HttpStatusCode.BadRequest)
        {
            return new PaginatedResult<T>(false, new List<T>(), 0, 1, 10, error, statusCode, new List<string> { error });
        }

        public static new PaginatedResult<T> Failure(
            List<string> errors,
            string message = LocalizationKeys.ExceptionMessages.Validation,
            int statusCode = (int)HttpStatusCode.BadRequest)
        {
            return new PaginatedResult<T>(false, new List<T>(), 0, 1, 10, message, statusCode, errors);
        }

        public static async Task<PaginatedResult<T>> CreateAsync(
            IQueryable<T> source,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var count = await source.CountAsync(cancellationToken);

            pageNumber = pageNumber <= 0 ? 1 : pageNumber;
            pageSize = pageSize <= 0 ? 10 : pageSize;

            var items = await source
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return Success(items, count, pageNumber, pageSize);
        }
    }
}
