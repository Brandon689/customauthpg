using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using YourNamespace.Services;
using YourNamespace.Repositories;
using YourNamespace.Models;

namespace YourNamespace;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        // Configure services
        builder.Services.AddSingleton<DatabaseInitializer>();
        builder.Services.AddSingleton<JwtService>();
        builder.Services.AddSingleton<UserRepository>();
        builder.Services.AddSingleton<PasswordHasher>();

        var host = builder.Build();

        using (var scope = host.Services.CreateScope())
        {
            var services = scope.ServiceProvider;

            try
            {
                var dbInitializer = services.GetRequiredService<DatabaseInitializer>();
                await dbInitializer.InitializeAsync();

                var userRepository = services.GetRequiredService<UserRepository>();
                var jwtService = services.GetRequiredService<JwtService>();
                var passwordHasher = services.GetRequiredService<PasswordHasher>();

                // Demo registration
                Console.WriteLine("Demonstrating user registration:");
                var newUser = new User
                {
                    Username = "testuser",
                    Email = "testuser@example.com",
                    PasswordHash = passwordHasher.HashPassword("password123"),
                    Role = "User"
                };

                bool registrationResult = await userRepository.CreateUser(newUser);
                Console.WriteLine($"User registration result: {registrationResult}");

                // Demo login
                Console.WriteLine("\nDemonstrating user login:");
                var user = await userRepository.GetUserByUsername("testuser");
                if (user != null && passwordHasher.VerifyPassword("password123", user.PasswordHash))
                {
                    var token = jwtService.GenerateToken(user);
                    Console.WriteLine($"Login successful. Token: {token}");
                }
                else
                {
                    Console.WriteLine("Login failed.");
                }
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occurred while running the application.");
            }
        }

        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }
}
