using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using RGBSyncPlus.UI;

namespace RGBSyncPlus
{
    public class SimpleLogger
    {
        public string Log = "";
        public void Debug(object log)
        {
            Log = Log + "DEBUG: "+DateTime.Now+" : "+log + "\r\n";
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
        }
    }
}
