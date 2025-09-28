using EVDealerSales.Models.Enums;

namespace EVDealerSales.Models.DTOs.UserDTOs
{
    public class CreateEmployeeDto
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Password { get; set; }
        public RoleType Role { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
