using EVDealerSales.Models.DTOs.OrderItemDTOs;
using EVDealerSales.Models.Enums;

namespace EVDealerSales.Models.DTOs.OrderDTOs
{
    public class OrderDto
    {
        public Guid QuoteId { get; set; }
        public DateTime OrderDate { get; set; }
        public OrderStatus Status { get; set; }
        public string DiscountType { get; set; }
        public decimal? DiscountValue { get; set; }
        public string DiscountNote { get; set; }
        public decimal SubtotalAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public OrderItemDto Items { get; set; } // Add this property
    }
}
