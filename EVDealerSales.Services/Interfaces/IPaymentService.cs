using EVDealerSales.BO.DTOs.PaymentDTOs;

namespace EVDealerSales.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<PaymentResponseDto> CreatePaymentAsync(PaymentDto dto);
        Task<PaymentResponseDto?> GetPaymentByIdAsync(Guid id);
        Task<List<ListPaymentDto>> GetAllPaymentsAsync();
        Task<bool> DeletePaymentAsync(Guid id);
    }
}