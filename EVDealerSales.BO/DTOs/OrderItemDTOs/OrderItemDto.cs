namespace EVDealerSales.BO.DTOs.OrderItemDTOs
{
    public class OrderItemDto
    {
        public Guid Id { get; set; }
        public Guid VehicleId { get; set; }

        public string? Name { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
    }
}
