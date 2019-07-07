using SharpShell.Helpers;
using SharpShell.Interop;
using System;
using System.Drawing;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SharpShell.Pidl
{
    public class PidlData : IDisposable
    {
        public PidlHandle Handle { get; set; }
        public static PidlData FromPath(string path) { return NativePidl.PIDListFromPath(path); }

        public string Path { [DebuggerStepThrough]  get { return Handle.IsEmpty ? null : PidlManager.GetPidlPath(Handle.pidl); } }
        public SHFILEINFO? ShFileInfo
        {
            get { return !Handle.IsEmpty ? (SHFILEINFO?)PidlManager.GetShFileInfo(Handle.pidl) : null; }
        }

        public string DisplayName { get { return Handle.IsEmpty ? null : PidlManager.GetPidlDisplayName(Handle.pidl); } }

        public Icon CreateOverlayIcon()
        {
            if (Handle.IsEmpty)
                return null;
            IntPtr hIcon = CreateOverlayIconPtr();
            return hIcon != IntPtr.Zero ? Icon.FromHandle(hIcon) : null;
        }

        public IntPtr CreateOverlayIconPtr()
        { 
            var fileInfo = new SHFILEINFO();
            Shell32.SHGetFileInfo(Handle.pidl, 0, out fileInfo, (uint)Marshal.SizeOf(fileInfo),
                SHGFI.SHGFI_ICON | SHGFI.SHGFI_SMALLICON | SHGFI.SHGFI_ADDOVERLAYS | SHGFI.SHGFI_PIDL 
                );
            return fileInfo.hIcon;
        }

        public void Dispose()
        {
            if (!Handle.IsEmpty)
            {
                Marshal.FreeCoTaskMem(Handle.pidl);
                Handle = PidlHandle.Empty;
            }
        }
    }
}
