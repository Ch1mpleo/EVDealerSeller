using EVDealerSales.Models.Commons;
using EVDealerSales.Models.DTOs.QuoteDTOs;
using EVDealerSales.Models.Enums;

namespace EVDealerSales.Services.Interfaces
{
    public interface IQuoteService
    {
        Task<QuoteResponseDto> CreateQuoteAsync(QuoteDto quoteDto);
        Task<QuoteResponseDto> GetQuoteByIdAsync(Guid id);
        Task<Pagination<QuoteListDto>> GetAllQuotesAsync(int pageNumber, int pageSize, string searchTerm = null);
        Task<Pagination<QuoteListDto>> GetQuotesByCustomerIdAsync(Guid staffId, int pageNumber, int pageSize);
        Task<QuoteResponseDto> UpdateQuoteAsync(Guid id, QuoteDto quoteDto);
        Task<bool> DeleteQuoteAsync(Guid id);
        Task<bool> UpdateQuoteStatusAsync(Guid id, QuoteStatus status);
        Task<bool> ProcessExpiredQuotesAsync();
    }
}
