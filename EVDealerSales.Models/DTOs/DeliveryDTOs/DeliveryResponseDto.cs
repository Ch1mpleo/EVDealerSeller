using EVDealerSales.Models.Enums;

namespace EVDealerSales.Models.DTOs.DeliveryDTOs
{
    public class DeliveryResponseDto
    {
        public Guid Id { get; set; }

        public Guid OrderId { get; set; }
        public string? CustomerName { get; set; }

        public DateTime? PlannedDate { get; set; }
        public DateTime? ActualDate { get; set; }
        public DeliveryStatus Status { get; set; }
    }
}
