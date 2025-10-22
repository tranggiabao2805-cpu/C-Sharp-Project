using Microsoft.AspNetCore.Identity;
using System;
using System.Security.Cryptography;

namespace FastFoodOnline.Services
{
    public class Pbkdf2PasswordHasher<TUser> : IPasswordHasher<TUser> where TUser : class
    {
        private const int Iterations = 10000;
        private const int SaltSize = 16;
        private const int HashSize = 32;

        public string HashPassword(TUser user, string password)
        {
            // Tạo salt
            var saltBytes = RandomNumberGenerator.GetBytes(SaltSize);
            var salt = Convert.ToBase64String(saltBytes);

            // Hash với PBKDF2 (HMACSHA256)
            using var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, Iterations, HashAlgorithmName.SHA256);
            var hash = Convert.ToBase64String(pbkdf2.GetBytes(HashSize));

            // Lưu hash và salt chung vào 1 string (vd: "hash:salt")
            return $"{hash}:{salt}";
        }

        public PasswordVerificationResult VerifyHashedPassword(TUser user, string hashedPassword, string providedPassword)
        {
            // Tách hash và salt
            var parts = hashedPassword.Split(':');
            if (parts.Length != 2)
                return PasswordVerificationResult.Failed;

            var hash = parts[0];
            var salt = parts[1];
            var saltBytes = Convert.FromBase64String(salt);

            using var pbkdf2 = new Rfc2898DeriveBytes(providedPassword, saltBytes, Iterations, HashAlgorithmName.SHA256);
            var newHash = Convert.ToBase64String(pbkdf2.GetBytes(HashSize));

            return newHash == hash ? PasswordVerificationResult.Success : PasswordVerificationResult.Failed;
        }
    }
}
