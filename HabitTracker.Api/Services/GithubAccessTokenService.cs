using HabitTracker.Api.Database;
using HabitTracker.Api.DTOs.Github;
using HabitTracker.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace HabitTracker.Api.Services
{
    public sealed class GithubAccessTokenService(AppDbContext appDbContext)
    {
        public async Task StoreAsync(
            string userId,
            StoreGithubAccessTokenDto storeGithubAccessTokenDto,
            CancellationToken cancellationToken = default)
        {
            GithubAccessToken? existingAccessToken = await GetAccessTokenAsync(userId, cancellationToken);

            if (existingAccessToken != null)
            {
                existingAccessToken.Token = storeGithubAccessTokenDto.AccessToken;
                existingAccessToken.ExpiresAtUtc = DateTime.UtcNow.AddDays(storeGithubAccessTokenDto.ExpiresInDays);
            }
            else
            {
                GithubAccessToken githubAccessToken = new()
                {
                    Id = $"gh_{Guid.CreateVersion7()}",
                    UserId = userId,
                    Token = storeGithubAccessTokenDto.AccessToken,
                    CreatedAtUtc = DateTime.UtcNow,
                    ExpiresAtUtc = DateTime.UtcNow.AddDays(storeGithubAccessTokenDto.ExpiresInDays)
                };

                await appDbContext.GithubAccessTokens.AddAsync(githubAccessToken, cancellationToken);
            }

            await appDbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<string?> GetAsync(string userId, CancellationToken cancellationToken = default)
        {
            GithubAccessToken? githubAccessToken = await GetAccessTokenAsync(userId, cancellationToken);

            return githubAccessToken?.Token;
        }

        public async Task RevokeAsync(string userId, CancellationToken cancellationToken = default)
        {
            GithubAccessToken? githubAccessToken = await GetAccessTokenAsync(userId, cancellationToken);

            if (githubAccessToken == null)
            {
                return;
            }

            appDbContext.GithubAccessTokens.Remove(githubAccessToken);

            await appDbContext.SaveChangesAsync(cancellationToken);
        }

        private async Task<GithubAccessToken?> GetAccessTokenAsync(string userId, CancellationToken cancellationToken = default)
        {
            return await appDbContext.GithubAccessTokens.FirstOrDefaultAsync(g => g.UserId == userId, cancellationToken);
        }
    }
}
