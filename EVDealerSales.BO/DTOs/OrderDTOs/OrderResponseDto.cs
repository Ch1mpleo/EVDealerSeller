using EVDealerSales.BO.DTOs.OrderItemDTOs;
using EVDealerSales.BO.Enums;

namespace EVDealerSales.BO.DTOs.OrderDTOs
{
    public class OrderResponseDto
    {
        public Guid Id { get; set; }
        public Guid QuoteId { get; set; }
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; }
        public Guid StaffId { get; set; }
        public string StaffName { get; set; }
        public DateTime OrderDate { get; set; }
        public OrderStatus Status { get; set; }
        public string DiscountType { get; set; }
        public decimal? DiscountValue { get; set; }
        public string DiscountNote { get; set; }
        public decimal SubtotalAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public OrderItemDto Items { get; set; }
    }
}
