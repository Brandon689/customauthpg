
using BCrypt.Net;

namespace YourNamespace.Services
{
    public class PasswordHasher
    {
        public string HashPassword(string password)
        {
            return BC.HashPassword(password, BC.GenerateSalt(12));
        }

        public bool VerifyPassword(string password, string hash)
        {
            return BC.Verify(password, hash);
        }
    }
}