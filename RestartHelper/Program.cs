using Newtonsoft.Json;
using RestartHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RestartHelper
{
    class Program
    {
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("Kernel32")]
        private static extern IntPtr GetConsoleWindow();

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;
        static void Main(string[] args)
        {
            AppSettings settings = JsonConvert.DeserializeObject<AppSettings>(File.ReadAllText("AppSettings.json"));
            if (settings.ShowHelperConsole == false)
            {
                IntPtr hwnd;
                hwnd = GetConsoleWindow();
                ShowWindow(hwnd, SW_HIDE);
            }
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Title = "JackNet RGB Sync is restarting... ";
            Console.WriteLine("JackNet RGB Sync Restart Helper");
            for (int i = 2; i > 0; i = i - 1)
            {
                Console.WriteLine("Waiting {0} seconds...", i);
                Thread.Sleep(1000);
            }
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Starting App...");
            Process.Start("RGBSync+.exe");
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
