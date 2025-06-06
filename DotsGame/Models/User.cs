using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotsGame.Models
{
    public class User
    {
        private const string Salt = "D0t5G@m3_S@lt_V2";

        public int Id { get; set; }
        public string Login { get; set; }
        public string PasswordHash { get; set; }
        public string Email { get; set; }
        public string AvatarPath { get; set; } = "";

        [InverseProperty("User")]
        public virtual ICollection<GameHistory> Games { get; set; } = new List<GameHistory>();

        public void SetPassword(string password)
        {
            PasswordHash = HashPassword(password);
        }

        public bool VerifyPassword(string password)
        {
            return PasswordHash == HashPassword(password);
        }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var saltedPassword = Salt + password;
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
            return Convert.ToBase64String(bytes);
        }
    }
}