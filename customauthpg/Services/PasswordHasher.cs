namespace customauthpg.Services;

public class PasswordHasher
{
    public string HashPassword(string password)
    {
        return BC.HashPassword(password, BC.GenerateSalt(10));
    }

    public bool VerifyPassword(string password, string hash)
    {
        return BC.Verify(password, hash);
    }
}