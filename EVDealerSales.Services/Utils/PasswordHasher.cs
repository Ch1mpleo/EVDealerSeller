using System.Security.Cryptography;
using System.Text;

namespace EVDealerSales.Services.Utils
{
    public class PasswordHasher
    {
        private const int SaltSize = 16; // 128-bit salt

        public string? HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be null or empty.");

            // Generate a random salt using RandomNumberGenerator
            var salt = new byte[SaltSize];
            RandomNumberGenerator.Fill(salt);

            // Hash the password
            var hash = HashPasswordWithSalt(password, salt);

            // Combine the salt and hash into a single string
            return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
        }

        public bool VerifyPassword(string password, string? storedHash)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(storedHash))
                return false;

            var parts = storedHash.Split(':');
            if (parts.Length != 2)
                return false;

            var salt = Convert.FromBase64String(parts[0]);
            var hash = Convert.FromBase64String(parts[1]);

            // Hash the input password with the stored salt
            var hashToVerify = HashPasswordWithSalt(password, salt);

            // Compare the stored hash with the computed hash
            return CryptographicOperations.FixedTimeEquals(hash, hashToVerify);
        }

        private byte[] HashPasswordWithSalt(string password, byte[] salt)
        {
            using (var hmac = new HMACSHA256(salt))
            {
                return hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }

        public static string GenerateNumeric(int length = 6)
        {
            if (length <= 0) throw new ArgumentOutOfRangeException(nameof(length), "Length phải lớn hơn 0");
            var digits = new char[length];
            using var rng = RandomNumberGenerator.Create();
            var buffer = new byte[4];

            for (var i = 0; i < length; i++)
            {
                rng.GetBytes(buffer);
                var value = BitConverter.ToUInt32(buffer, 0);
                digits[i] = (char)('0' + value % 10);
            }

            return new string(digits);
        }

        /// <summary>
        ///     Gen OTP kết hợp chữ cái hoa, chữ cái thường và chữ số với độ dài cho trước (mặc định 8).
        /// </summary>
        public static string GenerateAlphanumeric(int length = 8)
        {
            if (length <= 0) throw new ArgumentOutOfRangeException(nameof(length), "Length phải lớn hơn 0");
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var result = new StringBuilder(length);
            using var rng = RandomNumberGenerator.Create();
            var buffer = new byte[4];

            for (var i = 0; i < length; i++)
            {
                rng.GetBytes(buffer);
                var value = BitConverter.ToUInt32(buffer, 0);
                result.Append(chars[(int)(value % (uint)chars.Length)]);
            }

            return result.ToString();
        }
    }
}
