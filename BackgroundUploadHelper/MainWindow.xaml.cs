using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BackgroundUploadHelper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static string imageLocalPath = Directory.GetCurrentDirectory() + "\\CustomTheme\\base.png";
        public static string presetPath = Directory.GetCurrentDirectory() + "\\CustomTheme\\presets";
        public static string imagePath;
        public bool restartMainExe;
        public string mainExe = "RGBSync+";
        public MainWindow()
        {
            InitializeComponent();

                var converter = new ImageSourceConverter();
                PreviewImage.ImageSource = (ImageSource)converter.ConvertFromString("pack://application:,,,/default-bg.png");


            Process[] processes = Process.GetProcessesByName(mainExe);
            if (processes.Length == 0)
            {
                restartMainExe = false;
            }
            else
            {
                foreach (var process in processes)
                {
                    process.Kill();
                }
                restartMainExe = true;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (restartMainExe)
            {
                Process[] processes = Process.GetProcessesByName(mainExe);
                if (processes.Length == 0)
                {
                    string ExeName = mainExe + ".exe";
                    Process.Start(ExeName);
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (restartMainExe)
            {
                Process[] processes = Process.GetProcessesByName(mainExe);
                if (processes.Length == 0)
                {
                    string ExeName = mainExe + ".exe";
                    Process.Start(ExeName);
                }
            }
        }

        private void ImagePanel_Drop(object sender, DragEventArgs e)
        {

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                string ext = System.IO.Path.GetExtension(files[0]);
                if (ext == ".png" || ext == ".jpg" || ext == ".jpeg")
                {
                    RegisterFileName(files[0]);
                }  else
                {
                    MessageBox.Show("The file you selected is not a .png or .jpg image file. Please try again with a file of the correct type.","Error!",MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ChooseFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg";
            openFileDialog.Title = "Pick background image.";
            openFileDialog.InitialDirectory = presetPath;
            if (openFileDialog.ShowDialog() == true)
            {
                RegisterFileName(openFileDialog.FileName);
            }
        }

        public void RegisterFileName(string path)
        {
            BitmapImage bimage = new BitmapImage();
            bimage.BeginInit();
            bimage.UriSource = new Uri(path, UriKind.Absolute);
            bimage.EndInit();
            PreviewImage.ImageSource = bimage;
            imagePath = path;
        }

        private void UploadFileButton_Click(object sender, RoutedEventArgs e)
        {
            if(imagePath != null)
            {
                try
                {
                    File.Copy(imagePath, imageLocalPath, true);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            } else
            {
                MessageBox.Show("No file selected.", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }
    }
}
