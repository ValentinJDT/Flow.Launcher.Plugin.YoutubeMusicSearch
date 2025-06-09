using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using YouTubeMusicAPI.Client;
using YouTubeMusicAPI.Models;
using YouTubeMusicAPI.Models.Search;

namespace Flow.Launcher.Plugin.YoutubeMusicSearch
{
    public class YoutubeMusicSearch : IPlugin
    {
        private PluginInitContext _context;

        private YouTubeMusicClient client;

        public void Init(PluginInitContext context)
        {
            _context = context;
            client = new YouTubeMusicClient();
        }

        public List<Result> Query(Query query)
        {

            if (query.Search == "")
            {
                Result result = new Result();
                result.Title = "Type any keyword to search musics...";
                result.IcoPath = "YoutubeMusic.ico";
                result.AutoCompleteText = "";
                return new List<Result>() { result };
            }

            IEnumerable<SongSearchResult> searchResults = Task.Run(async () => await client.SearchAsync<SongSearchResult>(query.Search)).Result;

            List<Result> results = new();

            if (searchResults.Count() >= 1)
            {
                foreach (var songResult in searchResults)
                {
                    Result result = new Result();

                    Thumbnail thumbnail =  songResult.Thumbnails.First();

                    result.Title = songResult.Name;
                    result.SubTitle = String.Join(", ", songResult.Artists.Select(person => person.Name));
                    result.IcoPath = thumbnail.Url;

                    result.Action = context =>
                    {
                        ProcessUtils.CloseWindowWhoContains("YouTube Music -");
                        StartProcess(songResult.Id);
                        return true;
                    };

                    results.Add(result);
                }
            } else
            {
                Result result = new Result();
                result.Title = "No music found";
                result.IcoPath = "YoutubeMusic.ico";
                result.AutoCompleteText = "";
                results.Add(result);
            }

            return results;
        }

        private async void StartProcess(string musicId)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo {
                FileName = "C:\\Program Files\\Google\\Chrome\\Application\\chrome_proxy.exe",
                Arguments = getProcessUrlMusic(musicId),
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                UseShellExecute = false,
            };

            using(Process process = new Process { StartInfo = startInfo }) {
                process.Start();

                // Asynchronously wait for the process to be ready for input
                await Task.Run(() => process.WaitForInputIdle());

                await Task.Delay(800);

                ProcessUtils.MinimizeWindow("YouTube Music -");
            }
        }

        private string getProcessUrlMusic(string musicId)
        {
            return $"--profile-directory=Default --app-id=cinhimbnkkaeohfgghhklpknlkffjgod --app-launch-url-for-shortcuts-menu-item=\"https://music.youtube.com/watch?v={musicId}\"";
        }

    }
}