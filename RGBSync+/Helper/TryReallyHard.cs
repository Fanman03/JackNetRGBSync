using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RGBSyncStudio.Helper
{
    public static class TryReallyHard
    {
        public static void ToRun(Action thisCode, int tryTimes = 10, int andPauseBetweenRetries = 1000)
        {
            bool success = false;
            int attempts = 0;
            while (!success && attempts < tryTimes)
            {
                try
                {
                    thisCode();
                }
                catch
                {
                    attempts++;
                    Thread.Sleep(andPauseBetweenRetries);
                }
            }
        }

        public static T ToRun<T>(Func<T> thisCode, int tryTimes = 10, int andPauseBetweenRetries = 1000)
        {
            int attempts = 0;
            while (true)
            {
                try
                {
                    return thisCode();
                }
                catch
                {
                    attempts++;
                    if (attempts < tryTimes)
                    {
                        Thread.Sleep(andPauseBetweenRetries);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }
    }
}
