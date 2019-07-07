// MIT License (MIT)
// Copyright (c) 2013 Steven Kirk
// https://github.com/grokys/gong-shell

using System;
using System.Runtime.InteropServices;
using System.Text;
using SharpShell.Interop;
using Shell.SharpContextMenu;

//namespace GongSolutions.Shell.Interop
namespace SharpShell.SharpContextMenu
{
    #region Struct

    [Flags()]
    public enum HResult : int
    {
        DRAGDROP_S_CANCEL = 0x00040101,
        DRAGDROP_S_DROP = 0x00040100,
        DRAGDROP_S_USEDEFAULTCURSORS = 0x00040102,
        DATA_S_SAMEFORMATETC = 0x00040130,
        S_OK = WinError.S_OK,
        S_FALSE = 1,
        E_NOINTERFACE = unchecked((int)0x80004002),
        E_NOTIMPL = unchecked((int)0x80004001),
        E_FAIL = WinError.E_FAIL,
        E_OUTOFMEMORY = WinError.E_OUTOFMEMORY,

        OLE_E_ADVISENOTSUPPORTED = unchecked((int)80040003),
        MK_E_NOOBJECT = unchecked((int)0x800401E5),
    }

    public enum ERROR : int
    {
        SUCCESS,
        FILE_EXISTS = 80,
        BAD_PATHNAME = 161,
        ALREADY_EXISTS = 183,
        FILENAME_EXCED_RANGE = 206,
        CANCELLED = 1223,
    } 

    //[Flags]
    //public enum SFGAO : uint
    //{
    //    CANCOPY = 0x00000001,
    //    CANMOVE = 0x00000002,
    //    CANLINK = 0x00000004,
    //    STORAGE = 0x00000008,
    //    CANRENAME = 0x00000010,
    //    CANDELETE = 0x00000020,
    //    HASPROPSHEET = 0x00000040,
    //    DROPTARGET = 0x00000100,
    //    CAPABILITYMASK = 0x00000177,
    //    ENCRYPTED = 0x00002000,
    //    ISSLOW = 0x00004000,
    //    GHOSTED = 0x00008000,
    //    LINK = 0x00010000,
    //    SHARE = 0x00020000,
    //    READONLY = 0x00040000,
    //    HIDDEN = 0x00080000,
    //    DISPLAYATTRMASK = 0x000FC000,
    //    STREAM = 0x00400000,
    //    STORAGEANCESTOR = 0x00800000,
    //    VALIDATE = 0x01000000,
    //    REMOVABLE = 0x02000000,
    //    COMPRESSED = 0x04000000,
    //    BROWSABLE = 0x08000000,
    //    FILESYSANCESTOR = 0x10000000,
    //    FOLDER = 0x20000000,
    //    FILESYSTEM = 0x40000000,
    //    HASSUBFOLDER = 0x80000000,
    //    CONTENTSMASK = 0x80000000,
    //    STORAGECAPMASK = 0x70C50008,
    //}

    [Flags]
    public enum SHCIDS : uint
    {
        ALLFIELDS = 0x80000000,
        CANONICALONLY = 0x10000000,
        BITMASK = 0xFFFF0000,
        COLUMNMASK = 0x0000FFFF,
    }
 
    public enum SHGNO
    {
        NORMAL = 0x0000,
        INFOLDER = 0x0001,
        FOREDITING = 0x1000,
        FORADDRESSBAR = 0x4000,
        FORPARSING = 0x8000,
    }
 
    public enum SICHINT : uint
    {
        DISPLAY = 0x00000000,
        CANONICAL = 0x10000000,
        ALLFIELDS = 0x80000000
    }

    public enum SIGDN : uint
    {
        NORMALDISPLAY = 0,
        PARENTRELATIVEPARSING = 0x80018001,
        PARENTRELATIVEFORADDRESSBAR = 0x8001c001,
        DESKTOPABSOLUTEPARSING = 0x80028000,
        PARENTRELATIVEEDITING = 0x80031001,
        DESKTOPABSOLUTEEDITING = 0x8004c000,
        FILESYSPATH = 0x80058000,
        URL = 0x80068000
    }

    public enum SVSI : uint
    {
        SVSI_DESELECT = 0x00000000,
        SVSI_SELECT = 0x00000001,
    }

    [Flags]
    public enum _CMF : uint
    {
        NORMAL = 0x00000000,
        DEFAULTONLY = 0x00000001,
        VERBSONLY = 0x00000002,
        EXPLORE = 0x00000004,
        NOVERBS = 0x00000008,
        CANRENAME = 0x00000010,
        NODEFAULT = 0x00000020,
        INCLUDESTATIC = 0x00000040,
        EXTENDEDVERBS = 0x00000100,
        RESERVED = 0xffff0000,
    }

    public struct SHChangeNotifyEntry
    {
        public IntPtr pidl;
        public bool fRecursive;
    } 

    public struct SHNOTIFYSTRUCT
    {
        public IntPtr dwItem1;
        public IntPtr dwItem2;
    }

    [StructLayout(LayoutKind.Explicit, Size = 264)]
    public struct STRRET
    {
        [FieldOffset(0)]
        public UInt32 uType;
        [FieldOffset(4)]
        public IntPtr pOleStr;
        [FieldOffset(4)]
        public IntPtr pStr;
        [FieldOffset(4)]
        public UInt32 uOffset;
        [FieldOffset(4)]
        public IntPtr cStr;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MENUINFO
    {
        public UInt32 cbSize;
        public MIM fMask;
        public UInt32 dwStyle;
        public UInt32 cyMax;
        public IntPtr hbrBack;
        public UInt32 dwContextHelpID;
        public UIntPtr dwMenuData;
    }

    //[StructLayout(LayoutKind.Sequential)]
    //public struct MENUITEMINFO
    //{
    //    public UInt32 cbSize;
    //    public MIIM fMask;
    //    public UInt32 fType;
    //    public UInt32 fState;
    //    public UInt32 wID;
    //    public IntPtr hSubMenu;
    //    public IntPtr hbmpChecked;
    //    public IntPtr hbmpUnchecked;
    //    public UIntPtr dwItemData;
    //    public string dwTypeData;
    //    public UInt32 cch;
    //    public IntPtr hbmpItem;
    //}

    public enum MF
    {
        MF_BYCOMMAND = 0x00000000,
        MF_BYPOSITION = 0x00000400,
    }

    public enum MIM : uint
    {
        MIM_MAXHEIGHT = 0x00000001,
        MIM_BACKGROUND = 0x00000002,
        MIM_HELPID = 0x00000004,
        MIM_MENUDATA = 0x00000008,
        MIM_STYLE = 0x00000010,
        MIM_APPLYTOSUBMENUS = 0x80000000,
    }

    public enum MK
    {
        MK_LBUTTON = 0x0001,
        MK_RBUTTON = 0x0002,
        MK_SHIFT = 0x0004,
        MK_CONTROL = 0x0008,
        MK_MBUTTON = 0x0010,
        MK_ALT = 0x1000,
    }

    [Flags]
    public enum TPM
    {
        TPM_LEFTBUTTON = 0x0000,
        TPM_RIGHTBUTTON = 0x0002,
        TPM_LEFTALIGN = 0x0000,
        TPM_CENTERALIGN = 0x000,
        TPM_RIGHTALIGN = 0x000,
        TPM_TOPALIGN = 0x0000,
        TPM_VCENTERALIGN = 0x0010,
        TPM_BOTTOMALIGN = 0x0020,
        TPM_HORIZONTAL = 0x0000,
        TPM_VERTICAL = 0x0040,
        TPM_NONOTIFY = 0x0080,
        TPM_RETURNCMD = 0x0100,
        TPM_RECURSE = 0x0001,
        TPM_HORPOSANIMATION = 0x0400,
        TPM_HORNEGANIMATION = 0x0800,
        TPM_VERPOSANIMATION = 0x1000,
        TPM_VERNEGANIMATION = 0x2000,
        TPM_NOANIMATION = 0x4000,
        TPM_LAYOUTRTL = 0x8000,
    }

    [Flags]
    public enum TVIF
    {
        TVIF_TEXT = 0x0001,
        TVIF_IMAGE = 0x0002,
        TVIF_PARAM = 0x0004,
        TVIF_STATE = 0x0008,
        TVIF_HANDLE = 0x0010,
        TVIF_SELECTEDIMAGE = 0x0020,
        TVIF_CHILDREN = 0x0040,
        TVIF_INTEGRAL = 0x0080,
    }

    [Flags]
    public enum TVIS
    {
        TVIS_SELECTED = 0x0002,
        TVIS_CUT = 0x0004,
        TVIS_DROPHILITED = 0x0008,
        TVIS_BOLD = 0x0010,
        TVIS_EXPANDED = 0x0020,
        TVIS_EXPANDEDONCE = 0x0040,
        TVIS_EXPANDPARTIAL = 0x0080,
        TVIS_OVERLAYMASK = 0x0F00,
        TVIS_STATEIMAGEMASK = 0xF000,
        TVIS_USERMASK = 0xF000,
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    struct TVITEMW
    {
        public TVIF mask;
        public IntPtr hItem;
        public TVIS state;
        public TVIS stateMask;
        public string pszText;
        public int cchTextMax;
        public int iImage;
        public int iSelectedImage;
        public int cChildren;
        public int lParam;
    }

    internal enum GetWindow_Cmd : uint
    {
        GW_HWNDFIRST = 0,
        GW_HWNDLAST = 1,
        GW_HWNDNEXT = 2,
        GW_HWNDPREV = 3,
        GW_OWNER = 4,
        GW_CHILD = 5,
        GW_ENABLEDPOPUP = 6
    } 

    public enum MSG
    {
        WM_COMMAND = 0x0111,
        WM_VSCROLL = 0x0115,
        LVM_FIRST = 0x1000,
        LVM_SETIMAGELIST = 0x1003,
        LVM_GETITEMCOUNT = 0x1004,
        LVM_GETITEMA = 0x1005,
        LVM_EDITLABEL = 0x1017,
        LVM_GETCOLUMNWIDTH = LVM_FIRST + 29,
        LVM_SETCOLUMNWIDTH = LVM_FIRST + 30,
        TVM_SETIMAGELIST = 4361,
        TVM_SETITEMW = 4415
    } 

    #endregion

    public class ILShell32
    {
        [DllImport("shell32.dll", EntryPoint = "#660")]
        public static extern bool FileIconInit(bool bFullInit);

        [DllImport("shell32.dll", EntryPoint = "#18")]
        public static extern IntPtr ILClone(IntPtr pidl);

        [DllImport("shell32.dll", EntryPoint = "#25")]
        public static extern IntPtr ILCombine(IntPtr pidl1, IntPtr pidl2);

        [DllImport("shell32.dll")]
        public static extern IntPtr ILCreateFromPath(string pszPath);

        [DllImport("shell32.dll", EntryPoint = "#16")]
        public static extern IntPtr ILFindLastID(IntPtr pidl);

        [DllImport("shell32.dll", EntryPoint = "#155")]
        public static extern void ILFree(IntPtr pidl);

        [DllImport("shell32.dll", EntryPoint = "#21")]
        public static extern bool ILIsEqual(IntPtr pidl1, IntPtr pidl2);

        [DllImport("shell32.dll", EntryPoint = "#23")]
        public static extern bool ILIsParent(IntPtr pidl1, IntPtr pidl2,
            bool fImmediate);

        [DllImport("shell32.dll", EntryPoint = "#17")]
        public static extern bool ILRemoveLastID(IntPtr pidl);

        [DllImport("shell32.dll", EntryPoint = "#71")]
        public static extern bool Shell_GetImageLists(out IntPtr lphimlLarge,
            out IntPtr lphimlSmall);

        //[DllImport("shell32.dll", EntryPoint = "#2")]
        //public static extern uint SHChangeNotifyRegister(IntPtr hWnd,
        //    SHCNRF fSources, SHCNE fEvents, uint wMsg, int cEntries,
        //    ref SHChangeNotifyEntry pFsne);

        [DllImport("shell32.dll", EntryPoint = "#4")]
        public static extern bool SHChangeNotifyUnregister(uint hNotify);

        [DllImport("shell32.dll", EntryPoint = "#165", CharSet = CharSet.Unicode)]
        public static extern ERROR SHCreateDirectory(IntPtr hwnd, string pszPath);

        [DllImport("shell32.dll", PreserveSig = false)]
        public static extern IShellItem SHCreateItemFromIDList(
            [In] IntPtr pidl,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid);

        [DllImport("shell32.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
        public static extern IShellItem SHCreateItemFromParsingName(
            [In] string pszPath,
            [In] IntPtr pbc,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid);

        [DllImport("shell32.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
        public static extern IShellItem SHCreateItemWithParent(
            [In] IntPtr pidlParent,
            [In] IShellFolder psfParent,
            [In] IntPtr pidl,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid);

        //[DllImport("shell32.dll", PreserveSig = false)]
        //public static extern IShellFolder SHGetDesktopFolder();

        [DllImport("shell32.dll")]
        public static extern IntPtr SHGetFileInfo(IntPtr pszPath,
            int dwFileAttributes, out SHFILEINFO psfi, int cbFileInfo,
            SHGFI uFlags);

        [DllImport("user32.dll")]
        public static extern bool SetMenuInfo(IntPtr hmenu,
            ref MENUINFO lpcmi);

        [DllImport("user32.dll")]
        public static extern bool SetWindowPos(IntPtr hWnd,
            IntPtr hWndInsertAfter, int X, int Y, int cx, int cy,
            uint uFlags);

        [DllImport("user32.dll")]
        public static extern int TrackPopupMenuEx(IntPtr hmenu,
            TPM fuFlags, int x, int y, IntPtr hwnd, IntPtr lptpm);

        public delegate bool Win32Callback(IntPtr hwnd, IntPtr lParam);

        [DllImport("shfolder.dll")]
        public static extern HResult SHGetFolderPath(
            [In] IntPtr hwndOwner,
            [In] CSIDL nFolder,
            [In] IntPtr hToken,
            [In] uint dwFlags,
            [Out] StringBuilder pszPath);

        [DllImport("shell32.dll", PreserveSig = false)]
        public static extern IntPtr SHGetIDListFromObject(
            [In, MarshalAs(UnmanagedType.IUnknown)] object punk);

        [DllImport("shell32.dll")]
        public static extern bool SHGetPathFromIDList(
            [In] IntPtr pidl,
            [Out] StringBuilder pszPath);

        [DllImport("shell32.dll")]
        public static extern HResult SHGetSpecialFolderLocation(IntPtr hwndOwner,
            CSIDL nFolder, out IntPtr ppidl);
    } 
}
