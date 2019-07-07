using SharpShell.Helpers;
using SharpShell.Interop;
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace SharpShell.Pidl
{
    public static class NativePidl
    {
        private static IShellFolder desktopFolder = null;
        public static IShellFolder DesktopFolder
        {
            get
            {
                if (desktopFolder == null) Shell32.SHGetDesktopFolder(out desktopFolder);
                return desktopFolder;
            }
        }

        // SHSimpleIDListFromPath
        public static PidlData PIDListFromPath(string path)
        {
            var desktopFolder = DesktopFolder;
            var handle = new PidlHandle(desktopFolder == null ? IntPtr.Zero
                : GetShellFolderChildrenPIDL(desktopFolder, path));

            return new PidlData { Handle = handle };
        }

        public static IntPtr GetShellFolderChildrenPIDL(IShellFolder parentFolder, string displayName)
        {
            var bindCtx = PtrCreateBindCtx();

            uint pchEaten = 0;
            SFGAO pdwAttributes = SFGAO.NONE;
            IntPtr ppidl;
            parentFolder.ParseDisplayName(IntPtr.Zero, null, displayName, ref pchEaten, out ppidl, ref pdwAttributes);

            return ppidl;
        }

        [DllImport("shell32.dll", EntryPoint = "SHGetPathFromIDListW")]
        public static extern int // bool 
            SHGetPathFromIDList(IntPtr pidl, [MarshalAs(UnmanagedType.LPTStr)] StringBuilder pszPath);

        [DllImport("shell32.dll", ExactSpelling = true)]
        public static extern int SHOpenFolderAndSelectItems(
            IntPtr pidlFolder,
            uint cidl,
            [In, MarshalAs(UnmanagedType.LPArray)] IntPtr[] apidl,
            uint dwFlags);

        [DllImport("shell32.dll", EntryPoint = "SHParseDisplayNameW", CharSet = CharSet.Unicode)]
        public static extern void
            SHParseDisplayName([MarshalAs(UnmanagedType.LPWStr)] string name,
                    IBindCtx bindingContext,
                    [Out()] out IntPtr pidl, uint sfgaoIn, [Out()] out uint psfgaoOut);

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr
            ILCreateFromPath([MarshalAs(UnmanagedType.LPTStr)] string pszPath);

        [DllImport("ole32.dll", EntryPoint = "CreateBindCtx")]
        static extern int CreateBindCtx(int reserved, out IBindCtx ppbc);

        public static IBindCtx PtrCreateBindCtx()
        {
            IBindCtx result;
            Marshal.ThrowExceptionForHR(CreateBindCtx(0, out result));
            return result;
        }

        static void ReleaseComObject(params object[] comObjs)
        {
            foreach (object obj in comObjs)
            {
                if (obj != null && Marshal.IsComObject(obj))
                    Marshal.ReleaseComObject(obj);
            }
        }

        //PCIDLIST_ABSOLUTE pIdL = ILCreateFromPath(pszPath);
        //PIDLIST_ABSOLUTE pIdL = NULL;
        //SFGAOF out;
        //hr = SHParseDisplayName(pszPath,NULL,&pIdL, SFGAO_FILESYSTEM, &out);
        //if (SUCCEEDED(hr))
        //{
        //    hr = SHCreateItemFromIDList(pIdL, IID_PPV_ARGS(ppsi));
        //}

    }

}
