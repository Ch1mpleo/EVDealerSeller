using EVDealerSales.Models.Enums;

namespace EVDealerSales.Models.DTOs.QuoteDTOs
{
    public class QuoteDto
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public Guid VehicleId { get; set; }
        public decimal QuotedPrice { get; set; }
        public decimal? FinalPrice { get; set; }
        public QuoteStatus Status { get; set; }
        public DateTime? ValidUntil { get; set; }
        public string Remarks { get; set; }
    }
}
