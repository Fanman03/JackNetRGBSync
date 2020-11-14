﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SourceChord.FluentWPF.Utility
{
    sealed class VersionInfos
    {
        public static VersionInfo Windows7 { get { return new VersionInfo(6, 1, 7600); } }
        public static VersionInfo Windows7_SP1 { get { return new VersionInfo(6, 1, 7601); } }

        public static VersionInfo Windows8 { get { return new VersionInfo(6, 2, 9200); } }
        public static VersionInfo Windows8_1 { get { return new VersionInfo(6, 3, 9600); } }

        public static VersionInfo Windows10 { get { return new VersionInfo(10, 0, 10240); } }
        public static VersionInfo Windows10_1511 { get { return new VersionInfo(10, 0, 10586); } }
        public static VersionInfo Windows10_1607 { get { return new VersionInfo(10, 0, 14393); } }
        public static VersionInfo Windows10_1703 { get { return new VersionInfo(10, 0, 15063); } }
        public static VersionInfo Windows10_1709 { get { return new VersionInfo(10, 0, 16299); } }
        public static VersionInfo Windows10_1803 { get { return new VersionInfo(10, 0, 17134); } }
        public static VersionInfo Windows10_1809 { get { return new VersionInfo(10, 0, 17763); } }
        public static VersionInfo Windows10_1903 { get { return new VersionInfo(10, 0, 18362); } }
    }
}
