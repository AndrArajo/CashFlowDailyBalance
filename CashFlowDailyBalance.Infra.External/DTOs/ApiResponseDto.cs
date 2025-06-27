using CashFlowDailyBalance.Domain.Entities;

namespace CashFlowDailyBalance.Infra.External.DTOs
{
    public class ApiResponseDto<T>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
        public int StatusCode { get; set; }
        public string[]? Errors { get; set; }
    }

    public class PaginatedDataDto<T>
    {
        public List<T> Items { get; set; } = new();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }
    }

    public class TransactionDto
    {
        public int Id { get; set; }
        public string? Description { get; set; }
        public decimal Amount { get; set; }
        public int Type { get; set; }
        public string? Origin { get; set; }
        public DateTime TransactionDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? MessageId { get; set; }

        public Transaction ToTransaction()
        {
            return new Transaction(
                Amount,
                (Domain.Enums.TransactionType)Type,
                TransactionDate,
                Description,
                Origin)
            {
                Id = Id,
                CreatedAt = CreatedAt,
                MessageId = MessageId
            };
        }
    }
} 