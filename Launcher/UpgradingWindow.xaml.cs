using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Launcher
{
    /// <summary>
    /// Interaction logic for UpgradingWindow.xaml
    /// </summary>
    public partial class UpgradingWindow : Window
    {
        public UpgradingWindow()
        {
            InitializeComponent();
            vm = this.DataContext as UpgradingViewModel;
        }

        public UpgradingViewModel vm;
    }
}
