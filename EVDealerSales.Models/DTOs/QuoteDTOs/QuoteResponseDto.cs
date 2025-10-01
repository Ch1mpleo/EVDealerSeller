using EVDealerSales.Models.Enums;

namespace EVDealerSales.Models.DTOs.QuoteDTOs
{
    public class QuoteResponseDto
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; }
        public Guid StaffId { get; set; }
        public string StaffName { get; set; }
        public Guid VehicleId { get; set; }
        public string VehicleModel { get; set; }
        public decimal QuotedPrice { get; set; }
        public decimal? FinalPrice { get; set; }
        public QuoteStatus Status { get; set; }
        public DateTime? ValidUntil { get; set; }
        public string Remarks { get; set; }
    }
}
