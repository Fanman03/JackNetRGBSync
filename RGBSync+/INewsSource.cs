using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RGBSyncPlus
{
    public interface INewsSource
    {
        List<NewsStory> GetLatestStories();
    }

    public class NewsStory
    {
        public string Ident { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Body { get; set; }
        public List<string> Images { get; set; }
        public List<string> Videos { get; set; }
        public string Url { get; set; }
        public DateTime Date { get; set; }
    }

    public class RGBSyncStories : INewsSource
    {
        public List<NewsStory> GetLatestStories()
        {
            string address = "https://rgbsync.com/api/news/getPosts.php?limit=5";
            WebClient client = new WebClient();
            string reply = client.DownloadString(address);
            List<RGBSyncNewsStory> stories = JsonConvert.DeserializeObject<List<RGBSyncNewsStory>>(reply);
            return stories.Select(x => new NewsStory
            {
Author=x.author,
Body = x.content,
Date = DateTimeOffset.FromUnixTimeSeconds(x.date).DateTime,
Ident = x.guid,
Images = !string.IsNullOrWhiteSpace(x.image_url) ? new List<string> { x.image_url } : null,
Title = x.title,
Url = x.url,
Videos = !string.IsNullOrWhiteSpace(x.video_url) ? new List<string>{x.video_url} : null,
            }).ToList();
        }



        public class RGBSyncNewsStory
        {
            public int date { get; set; }
            public string guid { get; set; }
            public string title { get; set; }
            public string author { get; set; }
            public string content { get; set; }
            public string video_url { get; set; }
            public string image_url { get; set; }
            public string url { get; set; }
            public int _id { get; set; }
        }

    }

    public static class NewsManager
    {
        public static List<INewsSource> NewsSources = new List<INewsSource>();

        static NewsManager()
        {
            NewsSources.Add(new RGBSyncStories());
        }

        public static List<NewsStory> GetStories()
        {
            List<NewsStory> stories=new List<NewsStory>();
            foreach (INewsSource newsSource in NewsSources)
            {
                stories.AddRange(newsSource.GetLatestStories());
            }

            return stories.OrderByDescending(x => x.Date).ToList();
        }
    }
}
