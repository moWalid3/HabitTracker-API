using Microsoft.AspNetCore.Mvc;

namespace HabitTracker.Api.DTOs.Github
{
    public sealed record GithubUserProfileDto(
        string Login,
        string Name,
        [FromQuery(Name = "avatar_url")]
        Uri AvatarUrl,
        string Bio,
        [FromQuery(Name = "public_repos")]
        long PublicRepos,
        long Followers,
        long Following);
}
