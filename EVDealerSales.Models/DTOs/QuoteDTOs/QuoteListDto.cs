using EVDealerSales.Models.Enums;

namespace EVDealerSales.Models.DTOs.QuoteDTOs
{
    public class QuoteListDto
    {
        public Guid Id { get; set; }
        public string CustomerName { get; set; }
        public string VehicleModel { get; set; }
        public QuoteStatus Status { get; set; }
        public DateTime? ValidUntil { get; set; }
    }
}
