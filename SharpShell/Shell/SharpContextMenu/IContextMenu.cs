// MIT License (MIT)
// Copyright (c) 2013 Steven Kirk
// https://github.com/grokys/gong-shell

using SharpShell.Interop;
using System;
using System.Runtime.InteropServices;
using System.Text;

#pragma warning disable 1591

//namespace GongSolutions.Shell.Interop
namespace SharpShell.SharpContextMenu
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct CMINVOKECOMMANDINFO_ByIndex
    {
        public int cbSize;
        public int fMask;
        public IntPtr hwnd;
        public int iVerb;
        public string lpParameters;
        public string lpDirectory;
        public int nShow;
        public int dwHotKey;
        public IntPtr hIcon;
    }

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("000214e4-0000-0000-c000-000000000046")]
    public interface IContextMenu
    {
        [PreserveSig]
        HResult QueryContextMenu(IntPtr hMenu, uint indexMenu, int idCmdFirst,
                                 int idCmdLast, CMF uFlags);

        int InvokeCommand(IntPtr pici);  // ref CMINVOKECOMMANDINFO

        [PreserveSig]
        HResult GetCommandString(int idcmd, GCS uflags, int reserved,
            [MarshalAs(UnmanagedType.LPStr)] StringBuilder commandstring,
            int cch);
    }

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("000214f4-0000-0000-c000-000000000046")]
    public interface IContextMenu2 : IContextMenu
    {
        [PreserveSig]
        new HResult QueryContextMenu(IntPtr hMenu, uint indexMenu,
            int idCmdFirst, int idCmdLast,
            CMF uFlags);

        int InvokeCommand(ref CMINVOKECOMMANDINFO_ByIndex pici);

        [PreserveSig]
        new HResult GetCommandString(int idcmd, GCS uflags, int reserved,
            [MarshalAs(UnmanagedType.LPStr)] StringBuilder commandstring,
            int cch);

        [PreserveSig]
        HResult HandleMenuMsg(uint uMsg, IntPtr wParam, IntPtr lParam);
    }

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("bcfce0a0-ec17-11d0-8d10-00a0c90f2719")]
    public interface IContextMenu3 : IContextMenu2
    {
        [PreserveSig]
        new HResult QueryContextMenu(IntPtr hMenu, uint indexMenu, int idCmdFirst,
                             int idCmdLast, CMF uFlags);

        [PreserveSig]
        new int InvokeCommand(IntPtr pici); // ref CMINVOKECOMMANDINFO pici

        [PreserveSig]
        new HResult GetCommandString(int idcmd, GCS uflags, int reserved,
            [MarshalAs(UnmanagedType.LPStr)] StringBuilder commandstring,
            int cch);

        [PreserveSig]
        new HResult HandleMenuMsg(uint uMsg, IntPtr wParam, IntPtr lParam);

        [PreserveSig]
        HResult HandleMenuMsg2(uint uMsg, IntPtr wParam, IntPtr lParam,
            ref IntPtr plResult);
    }
}
