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

namespace RGBSyncPlus.UI
{
    /// <summary>
    /// Interaction logic for DeleteLayerDialog.xaml
    /// </summary>
    public partial class DeleteLayerDialog : Window
    {
        public DeleteLayerDialog(string groupName)
        {
            InitializeComponent();
//            GroupName.Text = " " + groupName + " ";
        }

        private void YesBtnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            this.Close();
        }

        private void NoBtnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }
    }
}
