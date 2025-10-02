using EVDealerSales.BO.DTOs.QuoteDTOs;
using EVDealerSales.BO.Enums;
using EVDealerSales.Services.Utils;

namespace EVDealerSales.Services.Interfaces
{
    public interface IQuoteService
    {
        Task<QuoteResponseDto> CreateQuoteAsync(QuoteDto quoteDto);
        Task<QuoteResponseDto> GetQuoteByIdAsync(Guid id);
        Task<Pagination<QuoteListDto>> GetAllQuotesAsync(int pageNumber, int pageSize, string searchTerm = null);
        Task<List<QuoteListDto>> GetAllQuotesNoPaginAsync();
        Task<Pagination<QuoteListDto>> GetQuotesByCustomerIdAsync(Guid staffId, int pageNumber, int pageSize);
        Task<QuoteResponseDto> UpdateQuoteAsync(Guid id, QuoteDto quoteDto);
        Task<bool> DeleteQuoteAsync(Guid id);
        Task<bool> UpdateQuoteStatusAsync(Guid id, QuoteStatus status);
        Task<bool> ProcessExpiredQuotesAsync();
    }
}
