using SimpleLed;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;


namespace RGBSyncStudio.UI.Tabs
{
    public class NewsViewModel : TabViewModel
    {
        public override async Task InitializeAsync()
        {
            NewsItems = new ObservableCollection<NewsItemViewModel>((await NewsManager.GetStoriesAsync()).ToList().Select(x =>
                new NewsItemViewModel
                {
                    Author = x.Author,
                    Content = x.Body,
                    Date = x.Date,
                    Title = x.Title,
                    Images = x.Images != null ? new ObservableCollection<string>(x.Images) : null,
                    Videos = x.Videos != null ? new ObservableCollection<string>(x.Videos) : null
                }));

            await base.InitializeAsync();
        }

        private ObservableCollection<NewsItemViewModel> newsItems;

        public ObservableCollection<NewsItemViewModel> NewsItems
        {
            get => newsItems;
            set => SetProperty(ref newsItems, value);
        }

        private NewsItemViewModel selectedNewsItem;

        public NewsItemViewModel SelectedNewsItem
        {
            get => selectedNewsItem;
            set => SetProperty(ref selectedNewsItem, value);
        }

        public NewsViewModel()
        {


            // SelectedNewsItem = NewsItems.Last();
        }

        public class NewsItemViewModel : BaseViewModel
        {
            private string author;
            private string title;
            private string content;
            private DateTime date;
            private ObservableCollection<string> images;
            private ObservableCollection<string> videos;

            public string Author
            {
                get => author;
                set => SetProperty(ref author, value);
            }

            public string Title
            {
                get => title;
                set => SetProperty(ref title, value);
            }

            public string Content
            {
                get => content;
                set => SetProperty(ref content, value);
            }

            public DateTime Date
            {
                get => date;
                set => SetProperty(ref date, value);
            }

            public ObservableCollection<string> Images
            {
                get => images;
                set => SetProperty(ref images, value);
            }

            public ObservableCollection<string> Videos
            {
                get => videos;
                set => SetProperty(ref videos, value);
            }
        }
    }
}
