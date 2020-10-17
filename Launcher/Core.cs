using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedCode;

namespace Launcher
{
    public static class Core
    {
        static Core()
        {
            LauncherPrefs = new LauncherPrefs();
        }

        public static LauncherPrefs LauncherPrefs;
    }
}
