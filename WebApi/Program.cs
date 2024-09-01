using customauthpg.Models;
using customauthpg.Repositories;
using customauthpg.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure services
builder.Services.AddSingleton<DatabaseInitializer>();
builder.Services.AddSingleton<JwtService>();
builder.Services.AddSingleton<UserRepository>();
builder.Services.AddSingleton<PasswordHasher>();

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<ConnectionStrings>(builder.Configuration.GetSection("ConnectionStrings"));

var app = builder.Build();

app.UseHttpsRedirection();

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        var dbInitializer = services.GetRequiredService<DatabaseInitializer>();
        await dbInitializer.InitializeAsync();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while initializing the database.");
    }
}

// Define a sample endpoint
app.MapGet("/", () => "Hello, Minimal API!");

// Add your API endpoints here
// For example:
// app.MapPost("/login", async (UserRepository repo, JwtService jwt, PasswordHasher hasher, LoginRequest request) => {
//     // Implement login logic here
// });

app.Run();
