namespace EVDealerSales.Models.DTOs.OrderDTOs
{
    public class ListOrderDto
    {
        public Guid Id { get; set; }
        public string CustomerName { get; set; }
        public string StaffName { get; set; }
        public string VehicleModel { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
