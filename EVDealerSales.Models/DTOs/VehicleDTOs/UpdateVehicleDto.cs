using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVDealerSales.Models.DTOs.VehicleDTOs
{
    public class UpdateVehicleDto
    {
        public Guid Id { get; set; }
        public string ModelName { get; set; }
        public string TrimName { get; set; }
        public int? ModelYear { get; set; }
        public decimal BasePrice { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
