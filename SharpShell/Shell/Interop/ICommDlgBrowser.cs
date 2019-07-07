using System;
using System.Runtime.InteropServices;
using System.Text;
using HResult = System.Int32;

namespace SharpShell.Interop
{
    [ComImport(), Guid("000214F1-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface ICommDlgBrowserMSDN
    {
        void OnDefaultCommand(IShellView ppshv);
        void OnStateChange(IShellView ppshv, CDBOSC uChange);
        void IncludeObject(IShellView ppshv, IntPtr pidl);
    }

    [ComImport(), Guid("000214E3-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IShellViewMSDN
    {
        void GetWindow(out IntPtr phwnd);
        void ContextSensitiveHelp(bool fEnterMode);

        [PreserveSig]
        long TranslateAcceleratorA(IntPtr pmsg);

        void EnableModeless(bool fEnable);
        void UIActivate(uint uState);
        void Refresh();
        void CreateViewWindow(IntPtr psvPrevious, ref FOLDERSETTINGS pfs, IShellBrowser psb, ref RECT prcView, ref IntPtr phWnd);
        void DestroyViewWindow();
        void GetCurrentInfo(ref FOLDERSETTINGS pfs);
        void AddPropertySheetPages(long dwReserved, ref IntPtr pfnPtr, int lparam);
        void SaveViewState();
        void SelectItem(IntPtr pidlItem, uint uFlags);

        [PreserveSig]
        long GetItemObject(uint uItem, ref Guid riid, ref IntPtr ppv);
    }

    /// <summary>
    /// Exposed by the common file dialog boxes to be used when they host a Shell browser.
    /// If supported, ICommDlgBrowser exposes methods that allow a Shell view to handle several cases that
    /// require different behavior in a dialog box than in a normal Shell view. You obtain an ICommDlgBrowser interface pointer by calling QueryInterface on the IShellBrowser object.
    /// </summary>
    [ComImport, Guid("000214f1-0000-0000-c000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface ICommDlgBrowser
    {
        /// <summary>
        /// Called when a user double-clicks in the view or presses the ENTER key.
        /// </summary>
        /// <param name="ppshv">A pointer to the view's IShellView interface.</param>
        /// <returns>If this method succeeds, it returns S_OK. Otherwise, it returns an HRESULT error code.</returns>
        [PreserveSig]
        int OnDefaultCommand([In] IShellView ppshv);

        /// <summary>
        /// Called after a state, identified by the uChange parameter, has changed in the IShellView interface.
        /// </summary>
        /// <param name="ppshv">A pointer to the view's IShellView interface.</param>
        /// <param name="uChange">Change in the selection state. This parameter can be one of the following values.</param>
        /// <returns>If this method succeeds, it returns S_OK. Otherwise, it returns an HRESULT error code.</returns>
        [PreserveSig]
        int OnStateChange(
            [In] IShellView ppshv, 
            [MarshalAs(UnmanagedType.U4)] CDBOSC uChange
            );

        /// <summary>
        /// Allows the common dialog box to filter objects that the view displays.
        /// </summary>
        /// <param name="ppshv">A pointer to the view's IShellView interface.</param>
        /// <param name="pidl">A PIDL, relative to the folder, that identifies the object.</param>
        /// <returns>The browser should return S_OK to include the object in the view, or S_FALSE to hide it.</returns>
        [PreserveSig]
        int IncludeObject([In] IShellView ppshv, IntPtr pidl);


        //IncludeObject is called by the IEnumIDList implementation when hosted in the file dialogs.
        //    The enumerator should call this function to let the common dialog filter out objects it does not want to display.Typically, the file
    }

/*
 * https://github.com/NVIDIA/winex_lgpl/blob/c659330a5e8d2f1f97b84c3f9238a6d78faed32f/winex/dlls/shell32/shlview.c
     * 
     **********************************************************
    *  IShellView_QueryInterface
   
    static HRESULT WINAPI IShellView_fnQueryInterface(IShellView* iface, REFIID riid, LPVOID* ppvObj)
    {
        ICOM_THIS(IShellViewImpl, iface);

        TRACE("(%p)->(\n\tIID:\t%s,%p)\n", This, debugstr_guid(riid), ppvObj);

        *ppvObj = NULL;

        if (IsEqualIID(riid, &IID_IUnknown))
        {
            *ppvObj = This;
        }
        else if (IsEqualIID(riid, &IID_IShellView))
        {
            *ppvObj = (IShellView*)This;
        }
        else if (IsEqualIID(riid, &IID_IOleCommandTarget))
        {
            *ppvObj = (IOleCommandTarget*)&(This->lpvtblOleCommandTarget);
        }
        else if (IsEqualIID(riid, &IID_IDropTarget))
        {
            *ppvObj = (IDropTarget*)&(This->lpvtblDropTarget);
        }
        else if (IsEqualIID(riid, &IID_IDropSource))
        {
            *ppvObj = (IDropSource*)&(This->lpvtblDropSource);
        }
        else if (IsEqualIID(riid, &IID_IViewObject))
        {
            *ppvObj = (IViewObject*)&(This->lpvtblViewObject);
        }

        if (*ppvObj)
        {
            IUnknown_AddRef((IUnknown*)*ppvObj);
            TRACE("-- Interface: (%p)->(%p)\n", ppvObj, *ppvObj);
            return S_OK;
        }
        TRACE("-- Interface: E_NOINTERFACE\n");
        return E_NOINTERFACE;
    }

    * 
    OnStateChange
    uChange
Type: ULONG
Change in the selection state.This parameter can be one of the following values.
CDBOSC_SETFOCUS
The focus has been set to the view.
CDBOSC_KILLFOCUS
The view has lost the focus.
CDBOSC_SELCHANGE
The selection has changed.
CDBOSC_RENAME
An item has been renamed.
CDBOSC_STATECHANGE
*/

    //http://fossies.org/linux/monodevelop/src/addins/WindowsPlatform/WindowsAPICodePack/Shell/Interop/ExplorerBrowser/ExplorerBrowserCOMInterfaces.cs
    //https://github.com/shellscape/Shellscape.Common/blob/master/Microsoft/Windows%20API/Shell/Interop/ExplorerBrowser/ExplorerBrowserCOMInterfaces.cs

   [ComImport,
    Guid("c8ad25a1-3294-41ee-8165-71174bd01c57"), // ExplorerBrowserIIDGuid.ICommDlgBrowser3),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface ICommDlgBrowser3_Mini : ICommDlgBrowser
    {

        [PreserveSig]
        //[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        HResult GetDefaultMenuText(
            IShellView shellView,
            IntPtr buffer, //A pointer to a buffer that is used by the Shell browser to return the default shortcut menu text.
            int bufferMaxLength); //should be max size = 260?

        [PreserveSig]
        //[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        HResult OnDefaultCommand(IntPtr ppshv);

        [PreserveSig]
        //[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        HResult GetCurrentFilter(
            StringBuilder pszFileSpec,
            int cchFileSpec);
    }

    // internal const string ICommDlgBrowser3 = "c8ad25a1-3294-41ee-8165-71174bd01c57";
    [ComImport,
       Guid("c8ad25a1-3294-41ee-8165-71174bd01c57"), // ExplorerBrowserIIDGuid.ICommDlgBrowser3),
       InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface ICommDlgBrowser3 : ICommDlgBrowser
    {
        // dlg1
        [PreserveSig]
        HResult OnDefaultCommand(IntPtr ppshv);

        [PreserveSig]
        //[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        HResult IncludeObject(
            IntPtr ppshv,
            IntPtr pidl);

        // dlg2
        [PreserveSig]
        HResult GetDefaultMenuText(
            IShellView shellView,
            IntPtr buffer, //A pointer to a buffer that is used by the Shell browser to return the default shortcut menu text.
            int bufferMaxLength); //should be max size = 260?

        [PreserveSig]
        HResult GetViewFlags(
            [Out] out uint pdwFlags); // CommDlgBrowser2ViewFlags 


        [PreserveSig]
        HResult Notify(
            IntPtr pshv, CommDlgBrowserNotifyType notifyType);

        // dlg3
        [PreserveSig]
        HResult GetCurrentFilter(
            StringBuilder pszFileSpec,
            int cchFileSpec);

        [PreserveSig]
        HResult OnColumnClicked(
            IShellView ppshv,
            int iColumn);

        [PreserveSig]
        HResult OnPreViewCreated(IShellView ppshv);
    }

    //internal static class KnownFoldersKFIDGuid
    //{
    //    internal const string ComputerFolder = "0AC0837C-BBF8-452A-850D-79D08E667CA7";
    //    internal const string Favorites = "1777F761-68AD-4D8A-87BD-30B759FA33DD";
    //    internal const string Documents = "FDD39AD0-238F-46AF-ADB4-6C85480369C7";
    //    internal const string Profile = "5E6C858F-0E22-4760-9AFE-EA3317B67173";
    //}

    //internal 
    public enum CommDlgBrowserNotifyType
    {
        Done = 1,
        Start = 2
    }

    public enum CDBOSC : uint
    {
        CDBOSC_SETFOCUS = 0,
        CDBOSC_KILLFOCUS = 1,
        CDBOSC_SELCHANGE = 2,
        CDBOSC_RENAME = 3,
        CDBOSC_STATECHANGE = 4
    }
}