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

        private void StartProcess(string musicId)
        {
            Process cmd = new Process();
            cmd.StartInfo.FileName = "C:\\Program Files\\Google\\Chrome\\Application\\chrome_proxy.exe";
            cmd.StartInfo.Arguments = getProcessUrlMusic(musicId);
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.StartInfo.UseShellExecute = false;
            cmd.Start();
        }

        private string getProcessUrlMusic(string musicId)
        {
            return $"--profile-directory=Default --app-id=cinhimbnkkaeohfgghhklpknlkffjgod --app-launch-url-for-shortcuts-menu-item=\"https://music.youtube.com/watch?v={musicId}\"";
        }

    }
}