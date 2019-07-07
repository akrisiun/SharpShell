using System;
using System.IO;

namespace SharpShell
{
    public static class FileSystem
    {
        public const int MAX_PATH = 260;

        public static string CurrentDirectory
        {
            get { return Directory.GetCurrentDirectory(); } // Environment.CurrentDirectory; } 
            set { System.IO.Directory.SetCurrentDirectory(value); }
        }

        public static OperatingSystem OS { get { return Environment.OSVersion; } }
        public static bool Is64Bit { get { return Environment.Is64BitProcess; } }

        public static double WindowsVersion
        {
            get
            { 
            OperatingSystem OS = Environment.OSVersion;
            return OS.Platform != PlatformID.Win32NT ? 0.0
                 : OS.Version.Major + ((double)OS.Version.Minor * 0.01);
            }
        }

        public static bool IsWindows8OrAbove { get { return WindowsVersion >= 6.02; } } // ((OS.Version.Major == 6 && OS.Version.Minor >= 2) || (OS.Version.Major > 6)));
        public static bool IsWindows1O { get { return WindowsVersion >= 6.02; } }

    }
}