using EVDealerSales.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace EVDealerSales.Models.DTOs.DeliveryDTOs
{
    public class DeliveryDto
    {
        [Required]
        public Guid OrderId { get; set; }

        public DateTime? PlannedDate { get; set; }
        public DateTime? ActualDate { get; set; }

        public DeliveryStatus? Status { get; set; } = DeliveryStatus.Scheduled;
    }
}
