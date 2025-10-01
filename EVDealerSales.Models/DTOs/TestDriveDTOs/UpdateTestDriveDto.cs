using EVDealerSales.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVDealerSales.Models.DTOs.TestDriveDTOs
{
    public class UpdateTestDriveDto
    {
        public Guid Id { get; set; }
        public DateTime ScheduledAt { get; set; }
        public TestDriveStatus Status { get; set; }
        public string? Notes { get; set; }
        public Guid? StaffId { get; set; }
    }
}
