using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SharpShell.Interop
{

    [ComImport, Guid("88E39E80-3578-11CF-AE69-08002B2E1262")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IShellView2 : IShellView, IOleWindow
    {
        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        int CreateViewWindow2(SV2CVW2_PARAMS lpParams);

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void GetView(
            Guid pvid,
            [In, MarshalAs(UnmanagedType.U4)]
            int // SVViewType 
                uView);
        // int GetView(SHELLVIEWID*, ULONG);


        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        int SelectAndPositionItem(IntPtr pidlItem, _SVSIF flags, IntPtr point);

        new void HandleRename(
        /* [in] */ /*LPCITEMIDLIST*/ IntPtr pidlNew);

        void SelectAndPositionItem(
            /* [in] */ /*LPCITEMIDLIST*/ IntPtr pidlItem,
            /* [in] */ UInt32 uFlags,
            /* [in] */ /*POINT* */ IntPtr ppt);

    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SV2CVW2_PARAMS
    {
        public /* DWORD              */ int cbSize;
        public /* IShellView*        */ IShellView psvPrev;
        public /* LPCFOLDERSETTINGS  */ IntPtr pfs;
        public /* IShellBrowser*     */ IShellBrowser psbOwner;
        public /* RECT*              */ IntPtr prcView;
        public /* const SHELLVIEWID* */ Guid pvid;
        public /* HWND               */ IntPtr hwndView;
    }

    public struct SV2CVW2_PARAMS_
    {
        public uint cbSize;

        public IShellView psvPrev;
        public uint pfs; // const
        public IntPtr psbOwner; // IShellBrowser
        public RECT prcView; // RECT
        public Guid pvid; // const

        public IntPtr hwndView;
    }


    internal class IIDGuid
    {
        private IIDGuid() { } // Avoid FxCop violation AvoidUninstantiatedInternalClasses

        // IID GUID strings for relevant COM interfaces
        internal const string IModalWindow = "b4db1657-70d7-485e-8e3e-6fcb5a5c1802";
        internal const string IFileDialog = "42f85136-db7e-439c-85f1-e4075d135fc8";
        internal const string IFileOpenDialog = "d57c7288-d4ad-4768-be02-9d969532d960";
        internal const string IFileSaveDialog = "84bccd23-5fde-4cdb-aea4-af64b83d78ab";
        internal const string IFileDialogEvents = "973510DB-7D7F-452B-8975-74A85828D354";
        internal const string IShellItem = "43826D1E-E718-42EE-BC55-A1E261C37BFE";
        internal const string IShellItemArray = "B63EA76D-1F85-456F-A19C-48159EFA858B";
    }

    internal static class AppModel_CLSID
    {
        /// <summary>CLSID_EnumerableObjectCollection</summary>
        /// <remarks>IID_IEnumObjects.</remarks>
        public const string EnumerableObjectCollection = "2d3468c1-36a7-43b6-ac24-d3f02fd9607a";
        /// <summary>CLSID_FileOpenDialog</summary>
        /// <remarks>IID_IFileOpenDialog</remarks>
        public const string FileOpenDialog = "DC1C5A9C-E88A-4dde-A5A1-60F82A20AEF7";
        /// <summary>CLSID_FileSaveDialog</summary>
        /// <remarks>IID_IFileSaveDialog</remarks>
        public const string FileSaveDialog = "C0B4E2F3-BA21-4773-8DBA-335EC946EB8B";
        /// <summary>CLSID_ShellLink</summary>
        /// <remarks>IID_IShellLink</remarks>
        public const string ShellLink = "00021401-0000-0000-C000-000000000046";
        /// <summary>CLSID_TaskbarList</summary>
        /// <remarks>IID_ITaskbarList</remarks>
        public const string TaskbarList = "56FDF344-FD6D-11d0-958A-006097C9A090";
        /// <summary>CLSID_WebBrowser</summary>
        /// <remarks>IID_IWebBrowser2</remarks>
        public const string WebBrowser = "8856f961-340a-11d0-a96b-00c04fd705a2";
    }

    internal static class IID
    {
        /// <summary>IID_IAccessible</summary>
        public const string Accessible = "618736e0-3c3d-11cf-810c-00aa00389b71";
        /// <summary>IID_IEnumIDList</summary>
        public const string EnumIdList = "000214F2-0000-0000-C000-000000000046";
        /// <summary>IID_IEnumObjects</summary>
        public const string EnumObjects = "2c1c7e2e-2d0e-4059-831e-1e6f82335c2e";
        /// <summary>IID_IFileDialog</summary>
        public const string FileDialog = "42f85136-db7e-439c-85f1-e4075d135fc8";
        /// <summary>IID_IFileDialogEvents</summary>
        public const string FileDialogEvents = "973510DB-7D7F-452B-8975-74A85828D354";
        /// <summary>IID_IFileOpenDialog</summary>
        public const string FileOpenDialog = "d57c7288-d4ad-4768-be02-9d969532d960";
        /// <summary>IID_IFileSaveDialog</summary>
        public const string FileSaveDialog = "84bccd23-5fde-4cdb-aea4-af64b83d78ab";

        /// <summary>IID_IModalWindow</summary>
        public const string ModalWindow = "b4db1657-70d7-485e-8e3e-6fcb5a5c1802";
        /// <summary>IID_IObjectArray</summary>
        public const string ObjectArray = "92CA9DCD-5622-4bba-A805-5E9F541BD8C9";
        /// <summary>IID_IObjectCollection</summary>
        public const string ObjectCollection = "5632b1a4-e38a-400a-928a-d4cd63230295";
        /// <summary>IID_IPropertyStore</summary>
        public const string PropertyStore = "886d8eeb-8cf2-4446-8d02-cdba1dbdcf99";

        /// <summary>IID_IShellFolder</summary>
        public const string ShellFolder = "000214E6-0000-0000-C000-000000000046";
        /// <summary>IID_IShellLink</summary>
        public const string ShellLink = "000214F9-0000-0000-C000-000000000046";
        /// <summary>IID_IShellItem</summary>
        public const string ShellItem = "43826d1e-e718-42ee-bc55-a1e261c37bfe";
        /// <summary>IID_IShellItem2</summary>
        public const string ShellItem2 = "7e9fb0d3-919f-4307-ab2e-9b1860310c93";
        /// <summary>IID_IShellItemArray</summary>
        public const string ShellItemArray = "B63EA76D-1F85-456F-A19C-48159EFA858B";
        /// <summary>IID_ITaskbarList</summary>
        public const string TaskbarList = "56FDF342-FD6D-11d0-958A-006097C9A090";
        /// <summary>IID_ITaskbarList2</summary>
        public const string TaskbarList2 = "602D4995-B13A-429b-A66E-1935E44F4317";
        /// <summary>IID_IUnknown</summary>
        public const string Unknown = "00000000-0000-0000-C000-000000000046";
    }

    [ComImport,
      InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
      Guid(IID.ShellFolder)
    ]
    public interface IShellFolder2_Mini // : IOleContainer
    {
        //void ParseDisplayName(
        //    [In] IntPtr hwnd,
        //    [In] IBindCtx pbc,
        //    [In, MarshalAs(UnmanagedType.LPWStr)] string pszDisplayName,
        //    [In, Out] ref int pchEaten,
        //    [Out] out IntPtr ppidl,
        //    [In, Out] ref uint pdwAttributes);

        IEnumIDList EnumObjects(
            [In] IntPtr hwnd,
            [In] SHCONTF grfFlags);

        // returns an instance of a sub-folder which is specified by the IDList (pidl).
        // IShellFolder or derived interfaces
        //[return: MarshalAs(UnmanagedType.Interface)]
        //object BindToObject(
        //    [In] IntPtr pidl,
        //    [In] IBindCtx pbc,
        //    [In] ref Guid riid);
    }

    [Guid("0000000e-0000-0000-C000-000000000046")]
    [InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    [ComImport]
    public interface IBindCtx
    {
        void RegisterObjectBound([MarshalAs(UnmanagedType.Interface)] Object punk);
        void RevokeObjectBound([MarshalAs(UnmanagedType.Interface)] Object punk);
        void ReleaseBoundObjects();

        //void SetBindOptions([In()] ref BIND_OPTS pbindopts);
        //void GetBindOptions(ref BIND_OPTS pbindopts);
        //void GetRunningObjectTable(out IRunningObjectTable pprot);
        void RegisterObjectParam([MarshalAs(UnmanagedType.LPWStr)] String pszKey, [MarshalAs(UnmanagedType.Interface)] Object punk);
        void GetObjectParam([MarshalAs(UnmanagedType.LPWStr)] String pszKey, [MarshalAs(UnmanagedType.Interface)] out Object ppunk);
        void EnumObjectParam(out IEnumString ppenum);
        [PreserveSig]
        int RevokeObjectParam([MarshalAs(UnmanagedType.LPWStr)] String pszKey);
    }

    //[Guid("00000010-0000-0000-C000-000000000046")]
    //[InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    //[ComImport]
    //public interface IRunningObjectTable
    //{
    //    int Register(int grfFlags, [MarshalAs(UnmanagedType.Interface)] Object punkObject, IMoniker pmkObjectName);
    //    void Revoke(int dwRegister);
    //    [PreserveSig]
    //    int IsRunning(IMoniker pmkObjectName);
    //    [PreserveSig]
    //    int GetObject(IMoniker pmkObjectName, [MarshalAs(UnmanagedType.Interface)] out Object ppunkObject);
    //    //void NoteChangeTime(int dwRegister, ref FILETIME pfiletime);

    //    //[PreserveSig]
    //    //int GetTimeOfLastChange(IMoniker pmkObjectName, out FILETIME pfiletime);
    //    void EnumRunning(out IEnumMoniker ppenumMoniker);
    //}

    [Guid("00000101-0000-0000-C000-000000000046")]
    [InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    [ComImport]
    public interface IEnumString
    {
        [PreserveSig]
        int Next(int celt, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr, SizeParamIndex = 0), Out] String[] rgelt, IntPtr pceltFetched);
        [PreserveSig]
        int Skip(int celt);
        void Reset();
        void Clone(out IEnumString ppenum);
    }

    //[Guid("00000102-0000-0000-C000-000000000046")]
    //[InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    //[ComImport]
    //public interface IEnumMoniker
    //{
    //    //[PreserveSig]
    //    //int Next(int celt, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0), Out] IMoniker[] rgelt, IntPtr pceltFetched);
    //    [PreserveSig]
    //    int Skip(int celt);
    //    void Reset();
    //    void Clone(out IEnumMoniker ppenum);
    //}

    //[Guid("0000000f-0000-0000-C000-000000000046")]
    //[InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    //[ComImport]
    //public interface IMoniker
    //{
    //    // IPersist portion
    //    void GetClassID(out Guid pClassID);

    //    // IPersistStream portion
    //    [PreserveSig]
    //    int IsDirty();

    //    //void Load(IStream pStm);
    //    //void Save(IStream pStm, [MarshalAs(UnmanagedType.Bool)] bool fClearDirty);
    //    void GetSizeMax(out Int64 pcbSize);

    //    // IMoniker portion
    //    void BindToObject(IBindCtx pbc, IMoniker pmkToLeft, [In()] ref Guid riidResult, [MarshalAs(UnmanagedType.Interface)] out Object ppvResult);
    //    void BindToStorage(IBindCtx pbc, IMoniker pmkToLeft, [In()] ref Guid riid, [MarshalAs(UnmanagedType.Interface)] out Object ppvObj);
    //    void Reduce(IBindCtx pbc, int dwReduceHowFar, ref IMoniker ppmkToLeft, out IMoniker ppmkReduced);
    //    void ComposeWith(IMoniker pmkRight, [MarshalAs(UnmanagedType.Bool)] bool fOnlyIfNotGeneric, out IMoniker ppmkComposite);
    //    void Enum([MarshalAs(UnmanagedType.Bool)] bool fForward, out IEnumMoniker ppenumMoniker);

    //    [PreserveSig]
    //    int IsEqual(IMoniker pmkOtherMoniker);
    //    void Hash(out int pdwHash);
    //    [PreserveSig]
    //    int IsRunning(IBindCtx pbc, IMoniker pmkToLeft, IMoniker pmkNewlyRunning);
    //    //void GetTimeOfLastChange(IBindCtx pbc, IMoniker pmkToLeft, out FILETIME pFileTime);
    //    void Inverse(out IMoniker ppmk);
    //    void CommonPrefixWith(IMoniker pmkOther, out IMoniker ppmkPrefix);
    //    void RelativePathTo(IMoniker pmkOther, out IMoniker ppmkRelPath);
    //    void GetDisplayName(IBindCtx pbc, IMoniker pmkToLeft, [MarshalAs(UnmanagedType.LPWStr)] out String ppszDisplayName);
    //    void ParseDisplayName(IBindCtx pbc, IMoniker pmkToLeft, [MarshalAs(UnmanagedType.LPWStr)] String pszDisplayName, out int pchEaten, out IMoniker ppmkOut);
    //    [PreserveSig]
    //    int IsSystemMoniker(out int pdwMksys);
    //}

    ////http://referencesource.microsoft.com/#System.Windows.Forms/winforms/Managed/System/WinForms/FileDialog_Vista_Interop.cs,1cbacdda87b82375
    //[ComImport,
    //  Guid(IIDGuid.IShellItem),
    //  InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    //public interface IShellItem2
    //{
    //    void BindToHandler([In, MarshalAs(UnmanagedType.Interface)] IntPtr pbc, [In] ref Guid bhid, [In] ref Guid riid, out IntPtr ppv);

    //    void GetParent([MarshalAs(UnmanagedType.Interface)] out IShellItem2 ppsi);

    //    void GetDisplayName([In] SIGDN sigdnName, [MarshalAs(UnmanagedType.LPWStr)] out string ppszName);

    //    void GetAttributes([In] uint sfgaoMask, out uint psfgaoAttribs);

    //    void Compare([In, MarshalAs(UnmanagedType.Interface)] IShellItem2 psi, [In] uint hint, out int piOrder);
    //}

    [
       ComImport(),
       Guid("00000002-0000-0000-c000-000000000046"),
       InterfaceType(ComInterfaceType.InterfaceIsIUnknown)
    ]
    public interface IMalloc
    {
        [PreserveSig]
        IntPtr Alloc(int cb);

        [PreserveSig]
        IntPtr Realloc(IntPtr pv, int cb);

        [PreserveSig]
        void Free(IntPtr pv);

        [PreserveSig]
        int GetSize(IntPtr pv);

        [PreserveSig]
        int DidAlloc(IntPtr pv);

        [PreserveSig]
        void HeapMinimize();
    }

    //public static class NativeShell
    //{
    //    public static int Size(this IMalloc malloc, IntPtr pv)
    //    {
    //        return malloc.GetSize(pv);
    //    }

    //    public static string GetFilePathFromShellItem(this IShellItem2 item)
    //    {
    //        string filename;
    //        item.GetDisplayName(SIGDN.SIGDN_DESKTOPABSOLUTEPARSING, out filename);
    //        return filename;
    //    }
    //}


    public enum _SIGDN : uint
    {
        SIGDN_NORMALDISPLAY = 0x00000000,           // SHGDN_NORMAL
        SIGDN_PARENTRELATIVEPARSING = 0x80018001,   // SHGDN_INFOLDER | SHGDN_FORPARSING
        SIGDN_DESKTOPABSOLUTEPARSING = 0x80028000,  // SHGDN_FORPARSING
        SIGDN_PARENTRELATIVEEDITING = 0x80031001,   // SHGDN_INFOLDER | SHGDN_FOREDITING
        SIGDN_DESKTOPABSOLUTEEDITING = 0x8004c000,  // SHGDN_FORPARSING | SHGDN_FORADDRESSBAR
        SIGDN_FILESYSPATH = 0x80058000,             // SHGDN_FORPARSING
        SIGDN_URL = 0x80068000,                     // SHGDN_FORPARSING
        SIGDN_PARENTRELATIVEFORADDRESSBAR = 0x8007c001,     // SHGDN_INFOLDER | SHGDN_FORPARSING | SHGDN_FORADDRESSBAR
        SIGDN_PARENTRELATIVE = 0x80080001           // SHGDN_INFOLDER
    }

    /*
    STDMETHOD(SetStatusTextSB) (LPCOLESTR lpszStatusText)
	{
		return E_NOTIMPL;
	}

    STDMETHOD(OnViewWindowActive)(struct IShellView *ppshv)
	{
		return E_NOTIMPL;
	}

    STDMETHOD(QueryInterface)(REFIID iid, void **ppvObject)
	{
		if(ppvObject == NULL)
			return E_POINTER;

		*ppvObject = NULL;

		if(iid == IID_IUnknown)
			*ppvObject = (IUnknown*)(IShellBrowser*) this;
		else if(iid == IID_IOleWindow)
			*ppvObject = (IOleWindow*) this;			
		else if(iid == IID_IShellBrowser)
			*ppvObject = (IShellBrowser*) this;
		else if(iid == IID_ICommDlgBrowser)
			*ppvObject = (ICommDlgBrowser*) this;
		else
			return E_NOINTERFACE;
		((IUnknown*)(*ppvObject))->AddRef();
		return S_OK;
	}


    // *** ICommDlgBrowser methods ***
	STDMETHOD(OnDefaultCommand) (THIS_ struct IShellView * ppshv)
	{	
		
    http://yaexplorer.googlecode.com/svn-history/r88/trunk/IShellBrowserImpl.h
    IFolderView, IFolderView2, IShellView and IShellView2
    
		FORMATETC cFmt = {(CLIPFORMAT) CF_HDROP, NULL, DVASPECT_CONTENT, -1, TYMED_HGLOBAL | TYMED_FILE

    VGIO_BACKGROUND
Refers to the background of the view. It is used with IID_IContextMenu to get a context menu for the view background. 
SVGIO_SELECTION 
Refers to the currently selected items. IID_IDataObject

    IDataObject* pDataObject = NULL;
		ppshv->GetItemObject(SVGIO_SELECTION, IID_IDataObject, (void**)&pDataObject);

		IShellFolder* pIShellFolder = NULL;
		ppshv->GetItemObject(SVGIO_SELECTION, IID_IShellFolder, (void**)&pIShellFolder);
        */

    //http://www.codeproject.com/Articles/3590/C-does-Shell-Part
    // Performs an operation on a specified file.
    //[DllImport("shell32.dll")]
    //public static extern IntPtr ShellExecute(
    //    IntPtr hwnd,               // Handle to a parent window.
    //    [MarshalAs(UnmanagedType.LPTStr)]
    //    String verb,          // lpOperation : Pointer to a null-terminated string, referred to in 
    //                          // this case as a verb, that specifies the action to 
    //                          // be performed.
    //    [MarshalAs(UnmanagedType.LPTStr)]
    //    String lpFile,        // Pointer to a null-terminated string that specifies 
    //                          // the file or object on which to execute the specified 
    //                          // verb.
    //    [MarshalAs(UnmanagedType.LPTStr)]
    //    String lpParameters,  // If the lpFile parameter specifies an executable file, 
    //                          // lpParameters is a pointer to a null-terminated string 
    //                          // that specifies the parameters to be passed to the 
    //                          // application.
    //    [MarshalAs(UnmanagedType.LPTStr)]
    //    String lpDirectory,   // Pointer to a null-terminated string that specifies
    //                          // the default directory. 
    //    Int32 nShowCmd);      // Flags that specify how an application is to be
    //                          // displayed when it is opened.
    //Here is an example for using this function:

    //Hide   Copy Code
    //int iRetVal;
    //iRetVal = (int)ShellLib.ShellApi.ShellExecute(
    //    this.Handle,
    //    "edit",
    //    @"c:\windows\Greenstone.bmp",
    //    "",
    //    Application.StartupPath,
    //    (int)ShellLib.ShellApi.ShowWindowCommands.SW_SHOWNORMAL);

}

//ub struct ITEMIDLIST {
//    pub mkid: SHITEMID,
//}
//     struct SHITEMID {
//    pub cb: USHORT,
//    pub abID: [BYTE; 0],
//}

// https://retep998.github.io/doc/shell32/fn.SHOpenFolderAndSelectItems.html
//https://retep998.github.io/doc/shell32/fn.SHGetFolderPathW.html
// unsafe extern "system" fn SHOpenFolderAndSelectItems(pidlFolder: PCIDLIST_ABSOLUTE, cidl: UINT, apidl: PCUITEMID_CHILD_ARRAY, dwFlags: DWORD) -> HRESULT
// https://social.msdn.microsoft.com/Forums/windowsdesktop/en-US/ab072aa7-b12c-4761-9187-347ef78244d9/how-to-obtain-pointer-to-ishellview2-interface?forum=windowsuidevelopment

