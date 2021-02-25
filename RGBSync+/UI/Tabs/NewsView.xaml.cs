using SyncStudio.WPF.Helper;
using System.Windows;
using System.Windows.Controls;

namespace SyncStudio.WPF.UI.Tabs
{
    /// <summary>
    /// Interaction logic for NewsView.xaml
    /// </summary>
    public partial class NewsView : UserControl
    {
        private NewsViewModel vm => this.DataContext as NewsViewModel;
        public void OpenUrl(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            e.Parameter.ToString().NavigateToUrlInDefaultBrowser();
        }
        public NewsView()
        {
            InitializeComponent();
            vm.InitializeAsync();
        }

        private void CloseModal(object sender, RoutedEventArgs e)
        {
            vm.SelectedNewsItem = null;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            vm.SelectedNewsItem = ((Button)sender).DataContext as NewsViewModel.NewsItemViewModel;
        }

    }
}
