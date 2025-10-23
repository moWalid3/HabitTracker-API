using HabitTracker.Api.Database;
using HabitTracker.Api.DTOs.Github;
using HabitTracker.Api.Entities;
using HabitTracker.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace HabitTracker.UnitTests.Services
{
    public class GithubAccessTokenServiceTests : IDisposable
    {
        private readonly AppDbContext dbContext;
        private readonly GithubAccessTokenService githubAccessTokenService;

        public GithubAccessTokenServiceTests()
        {
            DbContextOptions<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            dbContext = new AppDbContext(options);

            githubAccessTokenService = new(dbContext);
        }

        public void Dispose()
        {
            dbContext.Database.EnsureDeleted();
            dbContext.Dispose();
        }

        [Fact]
        public async Task StoreAsync_ShouldCreateNewToken_WhenUserDoesNotHaveOne()
        {
            // Arrange
            string userId = User.CreateNewId();

            StoreGithubAccessTokenDto dto = new()
            {
                AccessToken = "github-token",
                ExpiresInDays = 30,
            };

            // Act
            await githubAccessTokenService.StoreAsync(userId, dto);

            // Assert
            GithubAccessToken? token = await dbContext.GithubAccessTokens.FirstOrDefaultAsync(x => x.UserId == userId);

            Assert.NotNull(token);
            Assert.Equal(userId, token.UserId);
            Assert.Equal(dto.AccessToken, token.Token);
            Assert.True(token.ExpiresAtUtc > DateTime.UtcNow);
        }

        [Fact]
        public async Task StoreAsync_ShouldUpdateExistingToken_WhenUserHaveOne()
        {
            // Arrange
            string userId = User.CreateNewId();

            GithubAccessToken existingToken = new()
            {
                Id = GithubAccessToken.CreateNewId(),
                UserId = userId,
                Token = "github-token",
                ExpiresAtUtc = DateTime.UtcNow.AddDays(29),
                CreatedAtUtc = DateTime.UtcNow.AddDays(-1),
            };

            dbContext.GithubAccessTokens.Add(existingToken);
            await dbContext.SaveChangesAsync();
            dbContext.ChangeTracker.Clear();

            StoreGithubAccessTokenDto dto = new()
            {
                AccessToken = "new-github-token",
                ExpiresInDays = 30,
            };

            // Act
            await githubAccessTokenService.StoreAsync(userId, dto);

            // Assert
            GithubAccessToken? token = await dbContext.GithubAccessTokens.FirstOrDefaultAsync(x => x.UserId == userId);

            Assert.NotNull(token);
            Assert.Equal(existingToken.Id, token.Id);
            Assert.Equal(existingToken.UserId, token.UserId);
            Assert.NotEqual(existingToken.Token, token.Token);
            Assert.True(token.ExpiresAtUtc > existingToken.ExpiresAtUtc);
        }

        [Fact]
        public async Task GetAsync_ShouldReturnToken_WhenTokenExists()
        {
            // Arrange
            string userId = User.CreateNewId();
            string originalToken = "github-token";

            GithubAccessToken existingToken = new()
            {
                Id = GithubAccessToken.CreateNewId(),
                UserId = userId,
                Token = originalToken,
                ExpiresAtUtc = DateTime.UtcNow.AddDays(29),
                CreatedAtUtc = DateTime.UtcNow.AddDays(-1),
            };

            dbContext.GithubAccessTokens.Add(existingToken);
            await dbContext.SaveChangesAsync();
            dbContext.ChangeTracker.Clear();

            // Act
            string? token = await githubAccessTokenService.GetAsync(userId);

            // Assert
            Assert.NotNull(token);
            Assert.Equal(originalToken, token);
        }

        [Fact]
        public async Task GetAsync_ShouldReturnNull_WhenTokenDoesNotExist()
        {
            // Arrange
            string userId = User.CreateNewId();

            // Act
            string? token = await githubAccessTokenService.GetAsync(userId);

            // Assert
            Assert.Null(token);
        }

        [Fact]
        public async Task RevokeAsync_ShouldRemoveToken_WhenTokenExists()
        {
            // Arrange
            string userId = User.CreateNewId();

            GithubAccessToken existingToken = new()
            {
                Id = GithubAccessToken.CreateNewId(),
                UserId = userId,
                Token = "github-token",
                ExpiresAtUtc = DateTime.UtcNow.AddDays(29),
                CreatedAtUtc = DateTime.UtcNow.AddDays(-1),
            };

            dbContext.GithubAccessTokens.Add(existingToken);
            await dbContext.SaveChangesAsync();
            dbContext.ChangeTracker.Clear();

            // Act
            await githubAccessTokenService.RevokeAsync(userId);

            // Assert
            bool tokenExists = await dbContext.GithubAccessTokens.AnyAsync(x => x.UserId == userId);
            Assert.False(tokenExists);
        }

        [Fact]
        public async Task RevokeAsync_ShouldNotThrow_WhenTokenDoesNotExist()
        {
            // Arrange
            string userId = User.CreateNewId();

            // Act
            await githubAccessTokenService.RevokeAsync(userId);

            // Assert
            bool tokenExists = await dbContext.GithubAccessTokens.AnyAsync(x => x.UserId == userId);
            Assert.False(tokenExists);
        }
    }
}
