using System.Security.Claims;

namespace HabitTracker.Api.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static string? GetIdentityId(this ClaimsPrincipal claimsPrincipal)
        {
            string? identityId = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);

            return identityId;
        }
    }
}
