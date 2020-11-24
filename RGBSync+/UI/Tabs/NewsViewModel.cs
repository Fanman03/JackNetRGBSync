using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleLed;


namespace RGBSyncPlus.UI.Tabs
{
    public class NewsViewModel : BaseViewModel
    {
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
            NewsItems = new ObservableCollection<NewsItemViewModel>(NewsManager.GetStories().Select(x =>
                new NewsItemViewModel
                {
                    Author=x.Author,
                    Content = x.Body,
                    Date = x.Date,
                    Title = x.Title,
                    Images = x.Images != null ? new ObservableCollection<string>(x.Images) : null,
                    Videos = x.Videos !=null ? new ObservableCollection<string>(x.Videos) : null
                }));

            SelectedNewsItem = NewsItems.Last();
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
