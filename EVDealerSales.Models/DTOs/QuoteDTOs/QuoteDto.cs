namespace EVDealerSales.Models.DTOs.QuoteDTOs
{
    public class QuoteDto
    {
        virtual public int Id { get; set; }
        virtual public string CustomerName { get; set; }
        virtual public string CustomerEmail { get; set; }
        virtual public string VehicleModel { get; set; }
        virtual public decimal Price { get; set; }

    }
}
