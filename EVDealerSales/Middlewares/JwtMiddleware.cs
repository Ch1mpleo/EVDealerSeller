using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EVDealerSales.WebMVC.Middlewares
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<JwtMiddleware> _logger;
        private readonly IConfiguration _configuration;

        public JwtMiddleware(RequestDelegate next, ILogger<JwtMiddleware> logger, IConfiguration configuration)
        {
            _next = next;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Log the incoming request
            _logger.LogInformation("Request: {Method} {Path}", context.Request.Method, context.Request.Path);

            var token = context.Request.Cookies["AuthToken"]; // Retrieve token from cookie

            if (!string.IsNullOrEmpty(token))
            {
                try
                {
                    // Validate the token and attach user claims to the context
                    var claims = ValidateToken(token);
                    context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));
                }
                catch (SecurityTokenExpiredException)
                {
                    // Handle token expiration and refresh
                    var refreshToken = context.Request.Cookies["RefreshToken"];
                    if (!string.IsNullOrEmpty(refreshToken))
                    {
                        try
                        {
                            var newTokens = await RefreshTokenAsync(refreshToken);
                            context.Response.Cookies.Append("AuthToken", newTokens.AccessToken, new CookieOptions
                            {
                                HttpOnly = true,
                                Secure = true,
                                SameSite = SameSiteMode.Strict,
                                Expires = DateTime.UtcNow.AddMinutes(30)
                            });
                            context.Response.Cookies.Append("RefreshToken", newTokens.RefreshToken, new CookieOptions
                            {
                                HttpOnly = true,
                                Secure = true,
                                SameSite = SameSiteMode.Strict,
                                Expires = DateTime.UtcNow.AddDays(7)
                            });

                            // Re-validate the new token
                            var claims = ValidateToken(newTokens.AccessToken);
                            context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError("Token refresh failed: {Message}", ex.Message);
                            context.Response.Cookies.Delete("AuthToken");
                            context.Response.Cookies.Delete("RefreshToken");
                            context.Response.Redirect("/Auth/Login");
                            return;
                        }
                    }
                    else
                    {
                        _logger.LogWarning("No refresh token available. Redirecting to login.");
                        context.Response.Redirect("/Auth/Login");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("Token validation failed: {Message}", ex.Message);
                }
            }

            await _next(context); // Pass the request to the next middleware
        }

        private IEnumerable<Claim> ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]);

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            return jwtToken.Claims;
        }

        private async Task<(string AccessToken, string RefreshToken)> RefreshTokenAsync(string refreshToken)
        {
            // Simulate calling the refresh token API
            // Replace this with your actual service call
            await Task.Delay(100); // Simulate async call
            return ("newAccessToken", "newRefreshToken");
        }
    }
}
