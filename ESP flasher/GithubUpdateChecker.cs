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
            _client = new GitHubClient(new Octokit.ProductHeaderValue("ESPFlasher"));
        }

        public async Task<Version> GetLatestVersionAsync()
        {
            try
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
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error occurred while fetching the latest version.", ex);
            }
        }

        public async Task DisplayLatestReleaseInfoAsync()
        {
            try
            {
                var release = await _client.Repository.Release.GetLatest(_owner, _repo);
                Console.WriteLine(
                    "The latest release is tagged at {0} and is named {1}",
                    release.TagName,
                    release.Name);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to fetch the latest release info: {ex.Message}");
            }
        }
    }

}