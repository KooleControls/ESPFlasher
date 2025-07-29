using System;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Octokit;

namespace ESP_Flasher
{

    public class GithubUpdateChecker
    {
        private readonly GitHubClient _client;
        private readonly string _owner;
        private readonly string _repo;

        public GithubUpdateChecker(string owner, string repo)
        {
            _owner = owner;
            _repo = repo;

            // Initialize the Octokit GitHubClient
            _client = new GitHubClient(new Octokit.ProductHeaderValue(repo));
        }

        public async Task<Version> GetLatestVersionAsync()
        {
            // Fetch the latest release from the GitHub repository
            var release = await _client.Repository.Release.GetLatest(_owner, _repo);

            if (release == null || string.IsNullOrWhiteSpace(release.TagName))
                throw new InvalidOperationException("Failed to fetch the latest release information.");

            // Trim 'v' prefix from the tag name if present
            var versionString = release.TagName.TrimStart('v');

            if (!Version.TryParse(versionString, out var latestVersion))
                throw new InvalidOperationException($"The latest tag '{release.TagName}' is not a valid version.");

            return latestVersion;
        }    
    }
}