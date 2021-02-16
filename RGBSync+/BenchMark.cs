using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RGBSyncStudio
{
    public static class BenchMarkProvider
    {
        public static Dictionary<string, BenchMarkDetails> BenchMarks = new Dictionary<string, BenchMarkDetails>();

        public static int FramePointer = 0;
        public static int Steps = 32;

        public static void Output()
        {
            System.Diagnostics.Debug.WriteLine(
                "Key".PadLeft(50, ' ') + " " +
                ("avg").PadRight(30) + " " +
                ("avg ms").PadRight(15) + " " +
                ("peak").PadRight(30) + " " +
                ("peak ms").PadRight(15) + " " +
                ("total").PadRight(30) + " " +
                ("total ms").PadRight(15) + " " +
                ("#"));
            foreach (var benchMark in BenchMarks)
            {
                System.Diagnostics.Debug.WriteLine(
                    benchMark.Key.PadLeft(50,' ')+" "+
                    (benchMark.Value.AverageTime.ToString()).PadRight(30) + " " +
                    (benchMark.Value.AverageTime.TotalMilliseconds.ToString()).PadRight(15) + " " +
                    (benchMark.Value.LongestTime.ToString()).PadRight(30) + " " +
                    (benchMark.Value.LongestTime.TotalMilliseconds.ToString()).PadRight(15) + " " +
                    (benchMark.Value.TotalTime.ToString()).PadRight(30) + " " +
                    (benchMark.Value.TotalTime.TotalMilliseconds.ToString()).PadRight(15) + " " +
                    (benchMark.Value.NumberOfTimesRun)

                );
            }

            System.Diagnostics.Debug.WriteLine("-----------------------------");
        }
    }



    public class BenchMarkDetails
    {
        public TimeSpan FrameTime { get; set; } = TimeSpan.Zero;
        public string Key { get; set; }
        public int NumberOfTimesRun { get; set; } = 0;
        public TimeSpan ShortestTime { get; set; } = TimeSpan.MaxValue;
        public TimeSpan LongestTime { get; set; } = TimeSpan.Zero;
        public TimeSpan TotalTime { get; set; } = TimeSpan.Zero;
        public int[] History { get; set; } = new int[512];
        public double[] FrameHistory { get; set; } = new double[512];
        public TimeSpan[] FrameHistorySpan { get; set; } = new TimeSpan[512];
        public TimeSpan AverageTime => NumberOfTimesRun == 0 ? TimeSpan.Zero : TimeSpan.FromMilliseconds(TotalTime.TotalMilliseconds / NumberOfTimesRun);
        public Color Color { get; set; } = Color.White;
    }

    public class BenchMark : IDisposable
    {
        public string Key { get; set; }
        public DateTime StartTime { get; set; }
        //public BenchMark([CallerMemberName] string key = null)
        //{
        //    Key = key;
        //    StartTime = DateTime.Now;
        //}

        public BenchMark(string txt = "", [CallerMemberName] string key = null)
        {
            Key = key;
            if (!string.IsNullOrWhiteSpace(txt))
            {
                Key = txt + ": " + key;
            }

            StartTime = DateTime.Now;
        }

        public void Dispose()
        {
            TimeSpan timeRun = (DateTime.Now - StartTime);

            if (BenchMarkProvider.BenchMarks.ContainsKey(Key))
            {
                var cb = BenchMarkProvider.BenchMarks[Key];

                cb.NumberOfTimesRun++;

                if (timeRun < cb.ShortestTime) cb.ShortestTime = timeRun;
                if (timeRun > cb.LongestTime) cb.LongestTime = timeRun;

                cb.TotalTime = cb.TotalTime + timeRun;
            }
            else
            {
                try
                {
                    BenchMarkProvider.BenchMarks.Add(Key, new BenchMarkDetails()
                    {
                        Key = this.Key,
                        NumberOfTimesRun = 1,
                        TotalTime = timeRun,
                        ShortestTime = timeRun,
                        LongestTime = timeRun,
                        History = new int[BenchMarkProvider.Steps],
                        FrameHistory = new double[BenchMarkProvider.Steps],
                        FrameHistorySpan = new TimeSpan[BenchMarkProvider.Steps],
                    });
                }
                catch
                {
                }
            }

            BenchMarkProvider.BenchMarks[Key].FrameTime = BenchMarkProvider.BenchMarks[Key].FrameTime + timeRun;
            BenchMarkProvider.BenchMarks[Key].History[BenchMarkProvider.FramePointer % BenchMarkProvider.Steps] = (int)timeRun.TotalMilliseconds;
        }
    }
}
