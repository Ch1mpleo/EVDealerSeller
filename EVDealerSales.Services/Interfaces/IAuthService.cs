using EVDealerSales.BO.DTOs.AuthDTOs;
using Microsoft.Extensions.Configuration;

namespace EVDealerSales.Services.Interfaces
{
    public interface IAuthService
    {
        Task<UserDto?> RegisterUserAsync(UserRegistrationDto registrationDto);

        Task<LoginResponseDto?> LoginAsync(LoginRequestDto loginDto, IConfiguration configuration);

        Task<bool> LogoutAsync(Guid userId);
    }
}
