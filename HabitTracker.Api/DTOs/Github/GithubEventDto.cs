namespace HabitTracker.Api.DTOs.Github
{
    public sealed record GithubEventDto(
        string Id,
        string Type,
        GithubEventActorDto Actor,
        GithubEventRepoDto Repo,
        GithubEventPayloadDto Payload,
        bool Public,
        DateTimeOffset CreatedAt);

    public sealed record GithubEventActorDto(
        long Id,
        string Login,
        string DisplayLogin,
        string GravatarId,
        Uri Url,
        Uri AvatarUrl);

    public sealed record GithubEventRepoDto(
        long Id,
        string Name,
        Uri Url);

    public sealed record GithubEventPayloadDto(string Action, ICollection<Commit> Commits);

    public sealed record Commit(
        string Sha,
        Author Author,
        string Message,
        bool Distinct,
        Uri Url);

    public sealed record Author(string Email, string Name);
}
