﻿using HabitTracker.Api.Database;
using HabitTracker.Api.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace HabitTracker.Api.Services
{
    public sealed class UserContext(
        IHttpContextAccessor httpContextAccessor,
        AppDbContext appDbContext,
        IMemoryCache memoryCache)
    {
        private const string CacheKeyPrefix = "users:id:";
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);

        public async Task<string?> GetUserIdAsync(CancellationToken cancellationToken = default)
        {
            string? identityId = httpContextAccessor.HttpContext?.User.GetIdentityId();

            if (identityId == null)
            {
                return null;
            }

            string cacheKey = $"{CacheKeyPrefix}{identityId}";

            string? userId = await memoryCache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.SetSlidingExpiration(CacheDuration);

                string? userId = await appDbContext.Users
                    .Where(u => u.IdentityId == identityId)
                    .Select(u => u.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                return userId;
            });

            return userId;
        }
    }
}
