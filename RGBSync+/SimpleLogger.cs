using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using QRCoder;
using RGBSyncPlus.UI;

namespace RGBSyncPlus
{
    public class SimpleLogger
    {
        public string Log = "";
        public void Debug(object log)
        {
            Log = Log + "DEBUG: " + DateTime.Now + " : " + log + "\r\n";
        }

        public void Info(object log)
        {
            Log = Log + "LOG: " + DateTime.Now + " : " + log + "\r\n";
        }

        public void CrashWindow(Exception ex, [CallerMemberName] string callerMemberName = "")
        {
            CrashWindow crashWindow = new CrashWindow();
            crashWindow.errorName.Text = ex.GetType().ToString();
            crashWindow.message.Text = ex.Message;

            crashWindow.stackTrace.Text = ex.StackTrace;
            crashWindow.Show();


            Log = Log + "Crash: " + DateTime.Now + " : " + ex.GetType() + "\r\n";
            Log = Log + "Crash: " + DateTime.Now + " : " + ex.Message + "\r\n";
            Log = Log + "Crash: " + DateTime.Now + " : " + ex.StackTrace + "\r\n";

            string guid = crashWindow.SendReport();
            string url = "https://api.rgbsync.com/crashlogs/viewReport/?guid=" + guid;
            crashWindow.ErrorReportUrl = url;


            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            Bitmap qrCodeImage = qrCode.GetGraphic(20);

            crashWindow.qrcode.Source = BitmapToImageSource(qrCodeImage);
        }

        BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }
    }
}
