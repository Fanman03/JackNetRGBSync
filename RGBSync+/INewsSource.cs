using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Tweetinvi;
using Tweetinvi.Models;
using TweetSharp;

namespace RGBSyncStudio
{
    public interface INewsSource
    {
        List<NewsStory> GetLatestStories();
        Task<List<NewsStory>> GetLatestStoriesAsync();
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

    public class TwitterStories : INewsSource
    {
        private TwitterClient userClient;
        public TwitterStories()
        {
            userClient = new TwitterClient("LdKYMVl67TYnXCfY7JP9uMWpv", "RNjdMgKTKmnga700z61CNFvtzLItzGF09Yg2VljLwo5BWI0F35", "19777997-tNpHpDmL40JS4QJ5miFAdh4FPhU8W6IBqOq7SBWef", "eMSkBMBfELXTKiCvYi96l4Fz0BT3Ac5nsY0qWmx4U6jVs");

            
        }
        public List<NewsStory> GetLatestStories()
        {
            Debug.WriteLine("Getting Twitter");
            
            var temp = userClient.Timelines.GetUserTimelineAsync("rgbsync").Result;
            
            Debug.WriteLine("Got Twitter");

            return temp.Select(Convert).ToList();
        }

        public async Task<List<NewsStory>> GetLatestStoriesAsync()
        {
            Debug.WriteLine("Getting Twitter");

            var temp = await userClient.Timelines.GetUserTimelineAsync("rgbsync");

            Debug.WriteLine("Got Twitter");

            return temp.Select(Convert).ToList();
        }

        private NewsStory Convert(ITweet x)
        {
            return new NewsStory
            {
                Author = "RGBSync Twitter",
                Body = x.Text,
                Date = x.CreatedAt.ToLocalTime().Date,
                Ident = x.TweetDTO.Id.ToString(),
                Images = x.Media.Select(xx => xx.MediaURL).ToList(),
                Url = x.Url,
                Title = x.Text.Split('.', '!', ',','?').First()
            };
        }

    }

    public class RGBSyncStories : INewsSource
    {
        public List<NewsStory> GetLatestStories()
        {
            try
            {
                string address = "https://api.rgbsync.com/news/getPosts/?limit=5";
                WebClient client = new WebClient();
                string reply = client.DownloadString(address);
                List<RGBSyncNewsStory> stories = JsonConvert.DeserializeObject<List<RGBSyncNewsStory>>(reply);
                return stories.Select(x => new NewsStory
                {
                    Author = x.author,
                    Body = x.content,
                    Date = DateTimeOffset.FromUnixTimeSeconds(x.date).DateTime,
                    Ident = x.guid,
                    Images = !string.IsNullOrWhiteSpace(x.image_url) ? new List<string> { x.image_url } : null,
                    Title = x.title,
                    Url = x.url,
                    Videos = !string.IsNullOrWhiteSpace(x.video_url) ? new List<string> { x.video_url } : null,
                }).ToList();
            }
            catch
            {
                return new List<NewsStory>();
            }
        }

        public async Task<List<NewsStory>> GetLatestStoriesAsync()
        {
            return GetLatestStories();
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
            NewsSources.Add(new TwitterStories());
            NewsSources.Add(new RGBSyncStories());
        }

        public static List<NewsStory> GetStories()
        {
            List<NewsStory> stories = new List<NewsStory>();
            foreach (INewsSource newsSource in NewsSources)
            {
                stories.AddRange(newsSource.GetLatestStories());
            }

            return stories.OrderByDescending(x => x.Date).ToList();
        }

        public static async Task<List<NewsStory>> GetStoriesAsync()
        {
            List<NewsStory> stories = new List<NewsStory>();
            foreach (INewsSource newsSource in NewsSources)
            {
                stories.AddRange(await newsSource.GetLatestStoriesAsync());
            }

            return stories.OrderByDescending(x => x.Date).ToList();
        }
    }
}
