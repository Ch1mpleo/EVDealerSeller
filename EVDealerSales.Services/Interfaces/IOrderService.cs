using EVDealerSales.BO.DTOs.OrderDTOs;

namespace EVDealerSales.Services.Interfaces
{
    public interface IOrderService
    {
        Task<OrderResponseDto> CreateOrderAsync(OrderDto dto);
        Task<OrderResponseDto?> GetOrderByIdAsync(Guid id);
        Task<List<ListOrderDto>> GetAllOrdersAsync();
        Task<bool> DeleteOrderAsync(Guid id);
    }
}
