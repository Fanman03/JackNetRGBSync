using QRCoder;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;
using SyncStudio.WPF.UI;

namespace SyncStudio.WPF
{
    public class SimpleLogger
    {
        public List<LogEntry> Log = new List<LogEntry>();

        public void Debug(object log, [CallerMemberName] string cmn = "")
        {
            Log.Add(new LogEntry(log.ToString(), "Debug", null, cmn));
        }

        public void Info(object log, [CallerMemberName] string cmn = "")
        {
            Log.Add(new LogEntry(log.ToString(), "Info", null, cmn));
        }

        public void CrashWindow(Exception ex, [CallerMemberName] string callerMemberName = "")
        {
            CrashWindow crashWindow = new CrashWindow();
            crashWindow.errorName.Text = ex.GetType().ToString();
            crashWindow.message.Text = ex.Message;

            crashWindow.stackTrace.Text = ex.StackTrace;
            crashWindow.Show();

            Log.Add(new LogEntry(ex.GetType().ToString(), "Crash", ex));
            string guid = crashWindow.SendReport(ex);
            string url = "https://api.rgbsync.com/crashlogs/viewReport/?guid=" + guid;
            crashWindow.ErrorReportUrl = url;


            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            Bitmap qrCodeImage = qrCode.GetGraphic(20);

            crashWindow.qrcode.Source = BitmapToImageSource(qrCodeImage);
        }

        private BitmapImage BitmapToImageSource(Bitmap bitmap)
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

        public class LogEntry
        {
            public string Log { get; set; }
            public DateTime Time { get; set; }
            public string Caller { get; set; }
            public string LogType { get; set; }
            public Exception Exception { get; set; }
            public LogEntry(string log, string logType = "Info", Exception exception = null, [CallerMemberName] string callerMemberName = "")
            {
                Log = log;
                Time = DateTime.Now;
                Caller = callerMemberName;
                LogType = logType;

                if (exception != null)
                {
                    Exception = exception;
                }
            }
        }
    }
}
