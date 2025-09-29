namespace EVDealerSales.Models.DTOs.VehicleDTOs
{
    public class CreateVehicleDto
    {
        public string ModelName { get; set; }
        public string TrimName { get; set; }
        public int? ModelYear { get; set; }
        public decimal BasePrice { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
