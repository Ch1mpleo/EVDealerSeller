using EVDealerSales.Models.Enums;

namespace EVDealerSales.Models.Entities
{

    // Báo giá
    public class Quote : BaseEntity
    {
        public Guid CustomerId { get; set; }
        public Customer Customer { get; set; }
        public Guid StaffId { get; set; }
        public User Staff { get; set; }
        public Guid VehicleId { get; set; }
        public Vehicle Vehicle { get; set; }
        public decimal QuotedPrice { get; set; }
        public decimal? FinalPrice { get; set; }

        public QuoteStatus Status { get; set; }

        public DateTime? ValidUntil { get; set; } 
        public string Remarks { get; set; }
    }
}
