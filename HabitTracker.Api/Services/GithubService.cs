using HabitTracker.Api.DTOs.Github;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Net.Http.Headers;

namespace HabitTracker.Api.Services
{
    public sealed class GithubService(IHttpClientFactory httpClientFactory, ILogger<GithubService> logger)
    {
        private static readonly JsonSerializerSettings JsonSettings = new()
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy(),
            },
        };

        public async Task<GithubUserProfileDto?> GetUserProfileAsync(
            string accessToken,
            CancellationToken cancellationToken = default)
        {
            using HttpClient httpClient = CreateGithubClient(accessToken);

            HttpResponseMessage response = await httpClient.GetAsync("user", cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Failed to get Github user profile. Status code: {StatusCode}", response.StatusCode);
                return null;
            }

            string content = await response.Content.ReadAsStringAsync(cancellationToken);

            return JsonConvert.DeserializeObject<GithubUserProfileDto>(content, JsonSettings);
        }

        public async Task<IReadOnlyList<GithubEventDto>?> GetUserEventsAsync(
            string username,
            string accessToken,
            CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrEmpty(username);

            using HttpClient httpClient = CreateGithubClient(accessToken);

            HttpResponseMessage response = await httpClient.GetAsync(
                $"users/{username}/events?per_page=100",
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Failed to get Github user events. Status code: {StatusCode}", response.StatusCode);
                return null;
            }

            string content = await response.Content.ReadAsStringAsync(cancellationToken);

            return JsonConvert.DeserializeObject<List<GithubEventDto>>(content);
        }

        private HttpClient CreateGithubClient(string accessToken)
        {
            HttpClient httpClient = httpClientFactory.CreateClient("github");

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            return httpClient;
        }
    }
}
