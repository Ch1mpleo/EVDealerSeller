namespace EVDealerSales.BO.DTOs.AuthDTOs
{
    public class LoginRequestDto
    {
        // Using 'required' to ensure these properties must be set during object initialization
        public required string Email { get; set; }
        public required string Password { get; set; }
    }
}
