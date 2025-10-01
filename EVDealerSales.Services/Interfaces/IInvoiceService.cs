using EVDealerSales.BO.DTOs.InvoiceDTOs;

namespace EVDealerSales.Services.Interfaces
{
    public interface IInvoiceService
    {
        Task<InvoiceResponseDto> CreateInvoiceAsync(InvoiceDto dto);
        Task<InvoiceResponseDto?> GetInvoiceByIdAsync(Guid id);
        Task<List<ListInvoiceDto>> GetAllInvoicesAsync();
        Task<bool> DeleteInvoiceAsync(Guid id);
    }
}