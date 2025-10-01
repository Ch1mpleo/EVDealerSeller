using EVDealerSales.Models.DTOs.DeliveryDTOs;
using EVDealerSales.Models.Enums;

namespace EVDealerSales.Services.Interfaces
{
    public interface IDeliveryService
    {
        Task<DeliveryResponseDto> CreateDeliveryAsync(DeliveryDto dto);
        Task<DeliveryResponseDto?> GetDeliveryByIdAsync(Guid id);
        Task<List<ListDeliveryDto>> GetAllDeliveriesAsync();

        Task<bool> UpdateDeliveryAsync(Guid id, DeliveryDto dto);
        Task<bool> UpdateDeliveryStatusAsync(Guid id, DeliveryStatus status);
        Task<bool> DeleteDeliveryAsync(Guid id);
    }
}
