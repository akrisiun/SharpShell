using SharpShell.Interop;
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace SharpShell.SharpContextMenu
{
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("43826d1e-e718-42ee-bc55-a1e261c37bfe")]
    public interface IShellItem
    {
        IntPtr BindToHandler(IntPtr pbc,
            [MarshalAs(UnmanagedType.LPStruct)]Guid bhid,
            [MarshalAs(UnmanagedType.LPStruct)]Guid riid);

        [PreserveSig]
        HResult GetParent(out IShellItem ppsi);

        IntPtr GetDisplayName(SIGDN sigdnName);

        SFGAO GetAttributes(SFGAO sfgaoMask);

        int Compare(IShellItem psi, SICHINT hint);
    }
}
