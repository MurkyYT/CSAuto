using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Murky.Utils
{
    public static class Github
    {
        public static string GetLatestStringTag(string user, string repository)
        {
            string url = $"https://api.github.com/repos/{user}/{repository}/releases/latest";
            string webInfo = GetWebInfo(url);
            return webInfo.Split(new string[] { "\"tag_name\":\"" }, StringSplitOptions.None)[1].Split('"')[0];
        }
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public static async Task<GithubTag> GetLatestTagAsyncBySemver(string user, string repository)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            string url = $"https://api.github.com/repos/{user}/{repository}/tags";
            string webInfo = GetWebInfo(url);
            GithubTag[] tags = GetTags(webInfo);
            return GetMostRecentTagAsyncBySemver(tags);
        }
        public static async Task<GithubTag> GetMostRecentTagAsyncByCommitDate(string user, string repository)
        {
            string url = $"https://api.github.com/repos/{user}/{repository}/tags";
            string webInfo = GetWebInfo(url);
            GithubTag[] tags = GetTags(webInfo);
            return await GetMostRecentTagAsyncByCommitDate(tags);
        }

        private static GithubTag GetMostRecentTagAsyncBySemver(GithubTag[] tags)
        {
            GithubTag recent = tags[tags.Length-1];
            for (int i = tags.Length-1; i >= 0; i--)
            {
                if (CompareIntVersions(tags[i].Name, recent.Name) == 1)
                {
                    Log.WriteLine($"{tags[i].Name} > {recent.Name}");
                    recent = tags[i];
                }
            }
            return recent;
        }
        static int CompareIntVersions(string First, string Second)
        {
            if (int.TryParse(First.Replace(".", ""), out int a) == false ||
                int.TryParse(Second.Replace(".", ""), out int b) == false)
                return -2;
            var IntVersions = new List<int[]>
            {
                Array.ConvertAll(First.Split('.'), int.Parse),
                Array.ConvertAll(Second.Split('.'), int.Parse)
            };
            if ((IntVersions[0][0] > IntVersions[1][0]) || //Major ver
                (IntVersions[0][1] > IntVersions[1][1]) || // Minor ver
                (IntVersions[0][2] > IntVersions[1][2]) // Build ver
                 )
            {
                return 1;
            }
            else if ((IntVersions[0][0] < IntVersions[1][0]) || //Major ver
                (IntVersions[0][1] < IntVersions[1][1]) || // Minor ver
                (IntVersions[0][2] < IntVersions[1][2]))
            {
                return -1;
            }
            else return 0;
        }

        private static string GetWebInfo(string url)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.UserAgent = "[any words that is more than 5 characters]";
            HttpWebResponse response = (HttpWebResponse)req.GetResponse();
            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                string webInfo = reader.ReadToEnd();
                return webInfo;
            }
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        private static async Task<GithubTag> GetMostRecentTagAsyncByCommitDate(GithubTag[] tags)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            if (tags == null || tags.Length == 0)
                return null;
            GithubCommit[] commits = new GithubCommit[tags.Length];
            for (int i = 0; i < commits.Length; i++)
                commits[i] = new GithubCommit(GetWebInfo(tags[i].CommitURL), tags[i].ZipballUrl);
            GithubCommit recent = commits[0];
            for (int i = 0; i < commits.Length; i++)
            {
                if (commits[i].CommitDate > recent.CommitDate)
                {
                    recent = commits[i];
                }
            }
            GithubTag res = null;
            for (int i = 0; i < tags.Length && res == null; i++)
            {
                if (tags[i].ZipballUrl == recent.TagURL)
                    res = tags[i];
            }
            return res;
        }

        private static GithubTag[] GetTags(string webInfo)
        {
            int amount = CountTags(webInfo);
            GithubTag[] tags = new GithubTag[amount];
            for (int i = 0; i < amount; i++)
                tags[i] = GetTagAt(i, webInfo);
            return tags;
        }

        private static GithubTag GetTagAt(int index, string webInfo)
        {
            string[] split = webInfo.Split(new string[] { "\"name\":\"" }, StringSplitOptions.None);
            string tagSTR = null;
            for (int i = 0; i < split.Length && tagSTR == null; i++)
            {
                if ((i - 1) == index)
                    tagSTR = split[i].Split(new string[] { "},{" }, StringSplitOptions.None)[0];
            }
            return new GithubTag(tagSTR);
        }

        private static int CountTags(string allTags)
        {
            if (allTags == null)
                return 0;
            int count = 0;
            int a = 0;
            while ((a = allTags.IndexOf($"name", a)) != -1)
            {
                a += "name".Length;
                count++;
            }
            return count;
        }
    }
    public class GithubCommit
    {
        private string commitString;
        public GithubCommit(string commitString,string tagUrl)
        {
            this.TagURL = tagUrl;
            this.commitString = commitString;
            CommitDate = GetCommitDate();
            CommiterName = GetCommiterName();
            CommiterEmail = GetCommiterEmail();
        }

        private string GetCommiterEmail()
        {
            return commitString.Split(new string[] { "\"email\":\"" }, StringSplitOptions.None)[1].Split('"')[0];
        }

        private string GetCommiterName()
        {
            return commitString.Split(new string[] { "\"name\":\"" }, StringSplitOptions.None)[1].Split('"')[0];
        }

        private DateTime GetCommitDate()
        {
            return DateTime.Parse(commitString.Split(new string[] { "\"date\":\"" }, StringSplitOptions.None)[1].Split('"')[0]);
        }

        public DateTime CommitDate { get; set; }
        public string CommiterName { get; set; }
        public string CommiterEmail { get; set; }
        public string TagURL { get; set; }
    }
    public class GithubTag
    {
        private string tagString;
        public GithubTag(string tagString)
        {
            this.tagString = tagString;
            Name = GetName();
            ZipballUrl = GetZipballUrl();
            TarballUrl = GetTarballUrl();
            CommitSHA = GetCommitSHA();
            CommitURL = GetCommitURL();
            NodeID = GetNodeID();
        }

        private string GetNodeID()
        {
            return tagString.Split(new string[] { "\"node_id\":\"" }, StringSplitOptions.None)[1].Split('"')[0];
        }

        private string GetCommitURL()
        {
            return tagString.Split(new string[] { "\"url\":\"" }, StringSplitOptions.None)[1].Split('"')[0];
        }

        private string GetCommitSHA()
        {
            return tagString.Split(new string[] { "\"sha\":\"" }, StringSplitOptions.None)[1].Split('"')[0];
        }

        private string GetTarballUrl()
        {
            return tagString.Split(new string[] { "\"tarball_url\":\"" }, StringSplitOptions.None)[1].Split('"')[0];
        }

        private string GetZipballUrl()
        {
            return tagString.Split(new string[] { "\"zipball_url\":\"" }, StringSplitOptions.None)[1].Split('"')[0];
        }

        private string GetName()
        {
            return tagString.Split('"')[0];
        }

        public string Name { get; set; }
        public string ZipballUrl { get; set; }
        public string TarballUrl { get; set; }
        public string CommitSHA { get; set; }
        public string CommitURL { get; set; }
        public string NodeID { get; set; }
    }
}
