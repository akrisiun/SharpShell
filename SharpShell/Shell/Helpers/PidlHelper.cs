using SharpShell.Interop;
using SharpShell.Pidl;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SharpShell.Helpers
{
    public interface IHandle
    {
        bool IsEmpty { get; }
        string Path { get; }
        IntPtr Handle { get; }
    }

    // IntPtr pidl helpers

    public struct PidlHandle : IHandle
    {
        public IntPtr pidl;

        public PidlHandle(IntPtr? pidl = null)
        {
            this.pidl = pidl ?? Empty.pidl;
        }

        public bool IsEmpty { get { return pidl == Empty.pidl; } }
        public string Path { [DebuggerStepThrough]  get { return IsEmpty ? null : PidlManager.GetPidlPath(pidl); } }
        IntPtr IHandle.Handle { get {return pidl; }}

        public static PidlHandle Empty = new PidlHandle { pidl = IntPtr.Zero };
    }

    public struct PidlFolderData
    {
        public IntPtr folderTmpPtr;
        public IShellFolder folderTmp;
        public IntPtr pidlTmp;
        public IntPtr currentAbsolutePidl;
        public IShellFolder currentFolder;

        public static IntPtr DesktopFolderPidl;
        public static IShellFolder DesktopFolder;

        static PidlFolderData() {
            Shell32.SHGetDesktopFolder(out DesktopFolder);
            Shell32.SHGetSpecialFolderLocation(IntPtr.Zero, CSIDL.CSIDL_DESKTOP,
                ref DesktopFolderPidl);
        }
    }

    public static class PidlHelper
    {
        public static IShellFolder GetFolderImpl(this IntPtr pidl,
            ref PidlFolderData data, SBSP wFlags)
        {
            IShellFolder result = null;

            int hr = 0;
            IntPtr pidlTmp = IntPtr.Zero;
            var folderTmp = data.folderTmp;
            var currentAbsolutePidl = data.currentAbsolutePidl;
            var folderTmpPtr = data.folderTmpPtr;
            var currentFolder = data.currentFolder;
            
            IShellFolder desktopFolder = PidlFolderData.DesktopFolder;
            var desktopFolderPidl = PidlFolderData.DesktopFolderPidl;

            //  Check to see if we have a desktop pidl, relative pidl or absolite pidl.
            if (Shell32.ILIsEqual(pidl, desktopFolderPidl))
            {
                //  The provided PIDL is the desktop folder.
                pidlTmp = Shell32.ILClone(desktopFolderPidl);
                folderTmp = desktopFolder;
            }
            else if ((wFlags & SBSP.SBSP_RELATIVE) != 0)
            {
                // SBSP_RELATIVE - pidl is relative from the current folder
                if ((hr = currentFolder.BindToObject(pidl, IntPtr.Zero,
                    ref Shell32.IID_IShellFolder,
                    out folderTmpPtr)) != WinError.S_OK)
                    return result; // hr;

                pidlTmp = Shell32.ILCombine(currentAbsolutePidl, pidl);

                folderTmp = (IShellFolder)Marshal.GetObjectForIUnknown(folderTmpPtr);
            }
            else
            {
                // SBSP_ABSOLUTE - pidl is an absolute pidl (relative from desktop)
                PidlClone pidlClone = PidlClone.Of(pidl);
                folderTmp = GetPidlCloneFolder(pidlClone);

                pidlTmp = pidlClone.Handle;
            }

            if (folderTmp == null)
            {
                Shell32.ILFree(pidlTmp);
                return result; // WinError.E_FAIL;
            }

            result = folderTmp;
            return result;
        }

        public static IShellFolder GetPidlCloneFolder(PidlClone pidlClone)
        {
            int hr = 0;
            IShellFolder desktopFolder = PidlFolderData.DesktopFolder;
            var pidlTmp = pidlClone.Handle;

            IntPtr folderTmpPtr = IntPtr.Zero;
            if ((hr = desktopFolder.BindToObject(pidlTmp, IntPtr.Zero,
                ref Shell32.IID_IShellFolder,
                out folderTmpPtr)) != WinError.S_OK)
                return null;

            return Marshal.GetObjectForIUnknown(folderTmpPtr) as IShellFolder;
        }

    }

    public struct PidlClone : IHandle
    {
        PidlHandle hClone;
        private PidlClone(IntPtr h) { hClone = new PidlHandle(h); }

        public static PidlClone Of(IntPtr pidl)
        {
            IntPtr clone = Shell32.ILClone(pidl);
            return new PidlClone(clone);
        }

        public void ILFree() {
            if (!hClone.IsEmpty) 
                Shell32.ILFree(hClone.pidl);
            hClone = PidlHandle.Empty;
        }
        
        public bool IsEmpty { get { return hClone.IsEmpty; } }
        public string Path { get { return hClone.Path; } }
        // IHandle.
        public IntPtr Handle { get { return hClone.pidl; } }
    }
}
