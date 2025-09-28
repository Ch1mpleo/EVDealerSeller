using EVDealerSales.Models.Enums;

namespace EVDealerSales.Models.DTOs.UserDTOs
{
    public class GetEmployeeDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public RoleType Role { get; set; }
        public bool IsActive { get; set; }
    }
}
