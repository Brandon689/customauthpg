using customauthpg.Models;
using customauthpg.Repositories;
using Microsoft.Extensions.Options;

namespace customauthpg.Tests.Repositories
{
    public class UserRepositoryTests : IDisposable
    {
        private readonly UserRepository _userRepository;
        private readonly ConnectionStrings _connectionStrings;

        public UserRepositoryTests()
        {
            // You'll need to set up a test database and provide its connection string here
            _connectionStrings = new ConnectionStrings
            {
                DefaultConnection = "Host=localhost;Port=55432;Database=testdb;Username=postgres;Password=mysecretpassword"
            };

            var options = Options.Create(_connectionStrings);
            _userRepository = new UserRepository(options);
        }

        [Fact]
        public async Task CreateUser_ShouldReturnTrue_WhenUserIsCreated()
        {
            // Arrange
            var user = new User
            {
                Username = "testuser",
                Email = "testuser@example.com",
                PasswordHash = "hashedpassword",
                Role = "User"
            };

            // Act
            var result = await _userRepository.CreateUser(user);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task GetUserByUsername_ShouldReturnUser_WhenUserExists()
        {
            // Arrange
            var username = "existinguser";
            var user = new User
            {
                Username = username,
                Email = "existinguser@example.com",
                PasswordHash = "hashedpassword",
                Role = "User"
            };
            await _userRepository.CreateUser(user);

            // Act
            var result = await _userRepository.GetUserByUsername(username);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(username, result.Username);
        }

        [Fact]
        public async Task GetUserByUsername_ShouldReturnNull_WhenUserDoesNotExist()
        {
            // Arrange
            var nonExistentUsername = "nonexistentuser";

            // Act
            var result = await _userRepository.GetUserByUsername(nonExistentUsername);

            // Assert
            Assert.Null(result);
        }

        public void Dispose()
        {
            // Clean up the test database here if needed
        }
    }
}