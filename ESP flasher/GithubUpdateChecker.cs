using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace ESP_Flasher
{
    public class GithubUpdateChecker
    {
        private readonly string _apiUrl;

        public GithubUpdateChecker(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Path cannot be null or empty.");

            _apiUrl = $"https://api.github.com/repos/{path}/releases/latest";
        }

        public async Task<Version> GetLatestVersionAsync()
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "ESP-Flasher-Update-Checker");

            try
            {
                var rawResponse = await httpClient.GetStringAsync(_apiUrl);

                var response = JsonSerializer.Deserialize<Root>(rawResponse);

                if (response == null || string.IsNullOrWhiteSpace(response.tag_name))
                    throw new InvalidOperationException("Failed to fetch the latest release information from the repository.");

                var versionString = response.tag_name.TrimStart('v');

                if (!Version.TryParse(versionString, out var latestVersion))
                    throw new InvalidOperationException("The latest tag is not a valid version.");

                return latestVersion;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to fetch or parse version information from GitHub.", ex);
            }
        }


        public class Asset
        {
            public string url { get; set; }
            public int id { get; set; }
            public string node_id { get; set; }
            public string name { get; set; }
            public string label { get; set; }
            public Uploader uploader { get; set; }
            public string content_type { get; set; }
            public string state { get; set; }
            public int size { get; set; }
            public int download_count { get; set; }
            public DateTime created_at { get; set; }
            public DateTime updated_at { get; set; }
            public string browser_download_url { get; set; }
        }

        public class Author
        {
            public string login { get; set; }
            public int id { get; set; }
            public string node_id { get; set; }
            public string avatar_url { get; set; }
            public string gravatar_id { get; set; }
            public string url { get; set; }
            public string html_url { get; set; }
            public string followers_url { get; set; }
            public string following_url { get; set; }
            public string gists_url { get; set; }
            public string starred_url { get; set; }
            public string subscriptions_url { get; set; }
            public string organizations_url { get; set; }
            public string repos_url { get; set; }
            public string events_url { get; set; }
            public string received_events_url { get; set; }
            public string type { get; set; }
            public string user_view_type { get; set; }
            public bool site_admin { get; set; }
        }

        public class Root
        {
            public string url { get; set; }
            public string assets_url { get; set; }
            public string upload_url { get; set; }
            public string html_url { get; set; }
            public int id { get; set; }
            public Author author { get; set; }
            public string node_id { get; set; }
            public string tag_name { get; set; }
            public string target_commitish { get; set; }
            public string name { get; set; }
            public bool draft { get; set; }
            public bool prerelease { get; set; }
            public DateTime created_at { get; set; }
            public DateTime published_at { get; set; }
            public List<Asset> assets { get; set; }
            public string tarball_url { get; set; }
            public string zipball_url { get; set; }
            public string body { get; set; }
        }

        public class Uploader
        {
            public string login { get; set; }
            public int id { get; set; }
            public string node_id { get; set; }
            public string avatar_url { get; set; }
            public string gravatar_id { get; set; }
            public string url { get; set; }
            public string html_url { get; set; }
            public string followers_url { get; set; }
            public string following_url { get; set; }
            public string gists_url { get; set; }
            public string starred_url { get; set; }
            public string subscriptions_url { get; set; }
            public string organizations_url { get; set; }
            public string repos_url { get; set; }
            public string events_url { get; set; }
            public string received_events_url { get; set; }
            public string type { get; set; }
            public string user_view_type { get; set; }
            public bool site_admin { get; set; }
        }


    }
}
