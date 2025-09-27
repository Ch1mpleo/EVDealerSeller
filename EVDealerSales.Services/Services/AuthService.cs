using EVDealerSales.Models.DTOs.AuthenDTOs;
using EVDealerSales.Models.Entities;
using EVDealerSales.Models.Enums;
using EVDealerSales.Models.Interfaces;
using EVDealerSales.Services.Interfaces;
using EVDealerSales.Services.Utils;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace EVDealerSales.Services.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger _logger;

        public AuthService(IUnitOfWork unitOfWork, ILogger logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto loginDto, IConfiguration configuration)
        {
            try
            {
                _logger.Information($"Attempting login for {loginDto?.Email}");

                if (loginDto == null || string.IsNullOrWhiteSpace(loginDto.Email) || string.IsNullOrWhiteSpace(loginDto.Password))
                {
                    _logger.Warning("Login failed: missing email or password.");
                    throw new ArgumentException("Email and password are required.");
                }

                var user = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email && u.IsActive);

                if (user == null)
                {
                    _logger.Warning($"Login failed: user {loginDto.Email} not found or inactive.");
                    throw new KeyNotFoundException("User not found or inactive.");
                }

                var passwordHasher = new PasswordHasher();
                if (!passwordHasher.VerifyPassword(loginDto.Password, user.Password))
                {
                    _logger.Warning($"Login failed: invalid password for {loginDto.Email}.");
                    throw new UnauthorizedAccessException("Invalid credentials.");
                }

                var jwtToken = JwtUtils.GenerateJwtToken(
                    user.Id,
                    user.Email,
                    user.Role.ToString(),
                    configuration,
                    TimeSpan.FromHours(8)
                );

                var response = new LoginResponseDto
                {
                    Token = jwtToken
                };

                _logger.Information($"Login successful for {user.Email}");
                return response;
            }
            catch (Exception ex)
            {
                _logger.Error($"Login error for {loginDto?.Email}: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> LogoutAsync(Guid userId)
        {
            try
            {
                _logger.Information($"User with ID {userId} logged out");

                // In a real JWT-based system, you might:
                // 1. Add the token to a blacklist/revocation store
                // 2. Update user's last logout timestamp
                // 3. Revoke refresh tokens if used

                // For now, since JWT tokens are stateless, we'll just log the action
                // and consider the logout successful
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Logout error for user {userId}: {ex.Message}");
                return false;
            }
        }

        public async Task<UserDto?> RegisterUserAsync(UserRegistrationDto registrationDto)
        {
            try
            {
                _logger.Information("Registering new user");

                //if (await UserExistsAsync(registrationDto.Email))
                //{
                //    _loggerService.Warn($"Email {registrationDto.Email} already registered.");
                //    throw ErrorHelper.Conflict("Email have been used.");
                //}

                var hashedPassword = new PasswordHasher().HashPassword(registrationDto.Password);

                var user = new User
                {
                    FullName = registrationDto.FullName,
                    Email = registrationDto.Email,
                    Phone = registrationDto.Phone,
                    Role = RoleType.DealerStaff,
                    Password = hashedPassword ?? throw new Exception("Password hashing failed."),
                    IsActive = true
                };

                await _unitOfWork.Users.AddAsync(user);
                await _unitOfWork.SaveChangesAsync();
                _logger.Information($"User {user.Email} registered successfully");

                var userDto = new UserDto
                {
                    FullName = user.FullName,
                    Email = user.Email,
                    Phone = user.Phone,
                    Role = user.Role,
                    IsActive = user.IsActive
                };

                return userDto;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error creating account: {ex.Message}");
                throw;
            }
        }
    }
}
