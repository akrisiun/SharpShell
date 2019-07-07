//namespace GongSolutions.Shell

using SharpShell.Helpers;
using SharpShell.Interop;
using SharpShell.Pidl;
using Shell.SharpContextMenu;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using HResult = SharpShell.Interop.WinError;

namespace SharpShell.SharpContextMenu
{
    /// <summary>
    /// Provides support for displaying the context menu of a shell item.
    /// <remarks>
    /// Use this class to display a context menu for a shell item, either
    /// as a popup menu, or as a main menu. 
    /// you must intercept a number of special messages that will be sent 
    /// to the menu's parent form. To do this, you must override 
    /// <code>
    ///     protected override void WndProc(ref Message m) {
    ///         if ((m_ContextMenu == null) || (!m_ContextMenu.HandleMenuMessage(ref m))) {
    ///             base.WndProc(ref m);
    ///         }
    ///     }
    /// </code>
    /// Standard menu commands can also be invoked from this class, for 
    /// example <see cref="InvokeDelete"/> and <see cref="InvokeRename"/>.
    /// </remarks>
    public class ShellContextMenu : IDisposable
    {
        public Exception LastError { get; set; }

        public ShellContextMenu(ShellItem item)
        {
            Initialize(new ShellItem[] { item });
        }

        public void Dispose() { }

        /// <summary>
        /// Initialises a new instance of the <see cref="ShellContextMenu"/> 
        /// class.
        /// </summary>
        /// 
        /// <param name="items">
        /// The items to which the context menu should refer.
        /// </param>
        public ShellContextMenu(ShellItem[] items)
        {
            Initialize(items);
        }

        /// <summary>
        /// Handles context menu messages when the <see cref="ShellContextMenu"/>
        /// is displayed on a Form's main menu bar.
        /// </summary>
        /// <see langword="true"/> if the message was a Shell Context Menu
        /// message, <see langword="false"/> if not. If the method returns false,
        /// then the message should be passed down to the base class's
        /// <see cref="Form.WndProc"/> method.
        /// </returns>
        public bool HandleMenuMessage(ref Message m)
        {
            if ((m.Msg == (int)WindowsMessages.WM_COMMAND) && ((int)m.WParam >= cmdFirst))
            {
                InvokeCommand((int)m.WParam - cmdFirst);
                return true;
            }
            else
            {
                if (comInterface3 != null)
                {
                    IntPtr result = IntPtr.Zero;
                    if (comInterface3.HandleMenuMsg2((uint)m.Msg, m.WParam, m.LParam,
                        ref result) == HResult.S_OK)
                    {
                        m.Result = result;
                        return true;
                    }
                }
                else if (comInterface2 != null)
                {
                    if (comInterface2.HandleMenuMsg((uint)m.Msg, m.WParam, m.LParam)
                            == HResult.S_OK)
                    {
                        m.Result = IntPtr.Zero;
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Populates a <see cref="Menu"/> with the context menu items for
        /// a shell item.
        /// </summary>
        public void Populate(Menu menu)
        {
            RemoveShellMenuItems(menu);
            if (comInterface != null)
                comInterface.QueryContextMenu(menu.Handle, 0,
                    cmdFirst, int.MaxValue, CMF.CMF_EXPLORE);
        }

        /// <summary>
        /// Shows a context menu for a shell item.
        /// </summary>
        public void ShowContextMenu(Control control, Point pos)
        {
            if (messageWindow == null || messageWindow.Handle == IntPtr.Zero)
                return;

            using (ContextMenu menu = new ContextMenu())
            {
                pos = control.PointToScreen(pos);
                Populate(menu);

                int command = ILShell32.TrackPopupMenuEx(menu.Handle,
                    TPM.TPM_RETURNCMD, pos.X, pos.Y, messageWindow.Handle,
                    IntPtr.Zero);
                if (command > 0)
                {
                    InvokeCommand(command - cmdFirst);
                }
            }
        }

        /// <summary>
        /// Gets the underlying COM <see cref="IContextMenu"/> interface.
        /// </summary>
        public IContextMenu ComInterface
        {
            get { return comInterface; }
            set { comInterface = value; }
        }

        void Initialize(ShellItem[] items)
        {
            IntPtr[] pidls = new IntPtr[items.Length];
            ShellItem parent = null;

            for (int n = 0; n < items.Length; ++n)
            {
                pidls[n] = ILShell32.ILFindLastID(items[n].Pidl); // RelPidl

                if (parent == null)
                {
                    if (items[n] == ShellItem.Desktop)
                    {
                        parent = ShellItem.Desktop;
                    }
                    else
                    {
                        parent = items[n].Parent;
                    }
                }
                else
                {
                    if (items[n].Parent != parent)
                    {
                        throw new Exception("All shell items must have the same parent");
                    }
                }
            }

            IShellFolder folder = null;
            IShellItem parentItem = null;
            var item = items[0];
            var path = item.Path;

            if (parent != null && parent.IsFileSystem)
            {
                folder = GetIShellFolder(parent);
            }
            else
            {
                GetFolderPidlRetry(path, ref folder, ref parentItem, item.Pidl);
            }

            if (folder == null
                || parent != null && parent.Parent == ShellItem.Desktop && !parent.IsFileSystem && !item.IsFolder)
            {
                if (item.IsFolder)
                    return;
                // GetUIObjectOf : has exited with code -1073741819 (0xc0000005) 'Access violation'.
            }

            if (item.IsFolder)
                InitFolderPath(folder, parentItem, pidls[0], path);
            else
                InitFolder(folder, pidls[0]);
        }

        public bool InitFolderPath(IShellFolder folder, IShellItem parent, IntPtr itemPidl, string path)
        {
            IntPtr pidl = itemPidl;

            if (path != null && Directory.Exists(path))     // if item is directory
            {
                IntPtr combine = IntPtr.Zero;
                string parentFolder = Path.GetFullPath(path + @"\..");
                try
                {
                    // IntPtr result;
                    //var folderParent = (IShellFolder)Marshal.GetTypedObjectForIUnknown(result,
                    //    typeof(IShellFolder));
                    //ComAliasNameAttribute <

                    var parentPidl = ShPidlSystem.FromPath(parentFolder);
                    // clone = PidlClone.Of(itemPidl);

                    combine = ILShell32.ILCombine(parentPidl.Handle.pidl, itemPidl);

                    // ILShell32.ILRemoveLastID(combine);
                }
                catch (Exception ex)
                {
                    this.LastError = ex;
                }

                if (combine == IntPtr.Zero)
                    return false;

                pidl = combine;
                var check = new PidlHandle(pidl);

            }

            return InitFolder(folder, pidl);
        }

        public bool InitFolder(IShellFolder folder, IntPtr itemPidl)
        {
            var relativePidl = ILShell32.ILFindLastID(itemPidl);
            if (comInterface != null && messageWindow != null)
            {
                return true;
            }

            IntPtr result = IntPtr.Zero;
            IntPtr[] apidls = new IntPtr[] { itemPidl };

            var refGuid = typeof(IContextMenu).GUID;

            try
            {
                //PUIDLIST_RELATIVE ILFindChild(
                //  _In_ PCIDLIST_ABSOLUTE pidlParent,
                //  _In_ PCIDLIST_ABSOLUTE pidlChild
                //);

                PidlHandle handle = new PidlHandle(relativePidl);
                var itemPath = handle.Path;

                var relPidls = new IntPtr[] { relativePidl };

                folder.GetUIObjectOf(IntPtr.Zero,
                    (uint)relPidls.Length,
                    relPidls,  // [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)]
                    ref refGuid, 0, out result);

                if (result != IntPtr.Zero)
                {
                    comInterface = (IContextMenu)
                        Marshal.GetTypedObjectForIUnknown(result,
                            typeof(IContextMenu));

                    comInterface2 = comInterface as IContextMenu2;
                    comInterface3 = comInterface as IContextMenu3;
                    messageWindow = new MessageWindow(this);
                }
                else
                {
                    var handle2 = new PidlHandle(itemPidl);
                }
            }
            catch (Exception ex)
            {
                // Value does not fall within the expected range.
                this.LastError = ex;
            }

            return result != IntPtr.Zero;
        }

        //  pidl array Initialize retry
        public static IntPtr[] GetFolderPidlRetry(string path, ref IShellFolder folder, ref IShellItem parent,
                IntPtr? itemPidl = null)
        {
            string path2 = null;
            IntPtr pidl = IntPtr.Zero;
            if (itemPidl != null)
                pidl = itemPidl.Value;
            else
            {
                var item = ShPidlSystem.FromPath(path);
                pidl = item.Handle.pidl;
            }

            IShellItem parentInfo = null;
            folder = null;
            IntPtr[] pidls = EmptyPidls;
            String pathCheck = null;

            try
            {
                // if (RunningVista) 
                IShellItem i = ILShell32.SHCreateItemFromParsingName(path, IntPtr.Zero,
                        typeof(IShellItem).GUID);
                i.GetParent(out parentInfo);

                StringBuilder path2Ref = new StringBuilder(FileSystem.MAX_PATH);
                if (Shell32.SHGetPathFromIDList(pidl, path2Ref) > 0) // == WinError.S_OK)
                    path2 = path2Ref.ToString();
                //return Marshal.StringToHGlobalUni(result.ToString()); 

                folder = GetIShellFolderCom<IShellFolder>(i);
                IntPtr relativePidl = ILShell32.ILFindLastID(pidl);
                pidls = new IntPtr[] { relativePidl };

                StringBuilder resultStr = new StringBuilder(FileSystem.MAX_PATH);
                if (!ILShell32.SHGetPathFromIDList(relativePidl, resultStr))
                    pathCheck = resultStr.ToString();

                if (relativePidl != IntPtr.Zero)
                    parent = parentInfo;
            }
            catch (Exception)
            {
                // Free the PIDL, reset the result.
                // Marshal.FreeCoTaskMem(relativePidl);
                // throw new InvalidOperationException("Failed to initialise child.", exception);
                folder = null;
                pidls = EmptyPidls;
            }

            return pidls;
        }

        public static readonly IntPtr[] EmptyPidls = new IntPtr[] { };

        IShellFolder GetIShellFolder(ShellItem parent)
        {
            IShellItem ptr = parent.ItemInterface; // or ComInterface
            if (ptr == null)
                return null;
            return GetIShellFolderCom<IShellFolder>(ptr);
        }

        //private static Guid BHID_SFObject = new Guid("3981e224-f559-11d3-8e3a-00c04f6837d5");

        public static T GetIShellFolderCom<T>(IShellItem ptr)
        {
            IntPtr result = ptr.BindToHandler(IntPtr.Zero,
                BHID.SFObject,
                typeof(T).GUID);

            var ret = (T)Marshal.GetTypedObjectForIUnknown(result,
                typeof(T));
            return ret;
        }

        void InvokeCommand(int index)
        {
            const int SW_SHOWNORMAL = 1;
            CMINVOKECOMMANDINFO_ByIndex invoke = new CMINVOKECOMMANDINFO_ByIndex();
            invoke.cbSize = Marshal.SizeOf(invoke);
            invoke.iVerb = index;
            invoke.nShow = SW_SHOWNORMAL;
            try
            {
                comInterface2.InvokeCommand(ref invoke);
            }
            catch (Exception ex)
            {
                // Breakpoint
                this.LastError = ex;
            }
        }

        void TagManagedMenuItems(Menu menu, int tag)
        {
            //MENUINFO info = new MENUINFO();

            //info.cbSize = (UInt32)Marshal.SizeOf(info);
            //info.fMask = MIM.MIM_MENUDATA;
            //info.dwMenuData = (UIntPtr)tag;

            //foreach (MenuItem item in menu.MenuItems)
            //{
            //    User32.SetMenuInfo(item.Handle, ref info);
            //}
        }

        void RemoveShellMenuItems(Menu menu)
        {
            if (menu.MenuItems.Count == 0)
                return;

            //const int tag = 0xAB;
            // List<int> remove = new List<int>();
            //int count = User32.GetMenuItemCount(menu.Handle);
            //MENUINFO menuInfo = new MENUINFO();
            //MENUITEMINFO itemInfo = new MENUITEMINFO();

            //menuInfo.cbSize = (UInt32)Marshal.SizeOf(menuInfo);
            //menuInfo.fMask = MIM.MIM_MENUDATA;
            //itemInfo.cbSize = (UInt32)Marshal.SizeOf(itemInfo);
            //itemInfo.fMask = MIIM.MIIM_ID | MIIM.MIIM_SUBMENU;

            //// First, tag the managed menu items with an arbitary 
            //// value (0xAB).
            //TagManagedMenuItems(menu, tag);

            //for (int n = 0; n < count; ++n)
            //{
            //    User32.GetMenuItemInfo(menu.Handle, n, true, ref itemInfo);

            //    if (itemInfo.hSubMenu == IntPtr.Zero)
            //    {
            //        // If the item has no submenu we can't get the tag, so 
            //        // check its ID to determine if it was added by the shell.
            //        if (itemInfo.wID >= m_CmdFirst) remove.Add(n);
            //    }
            //    else
            //    {
            //        User32.GetMenuInfo(itemInfo.hSubMenu, ref menuInfo);
            //        if ((int)menuInfo.dwMenuData != tag) remove.Add(n);
            //    }
            //}

            //// Remove the unmanaged menu items.
            //remove.Reverse();
            //foreach (int position in remove)
            //{
            //    User32.DeleteMenu(menu.Handle, position, MF.MF_BYPOSITION);
            //}
        }

        class MessageWindow : Control
        {
            public MessageWindow(ShellContextMenu parent)
            {
                m_Parent = parent;
            }

            protected override void WndProc(ref Message m)
            {
                if (!m_Parent.HandleMenuMessage(ref m))
                {
                    base.WndProc(ref m);
                }
            }

            ShellContextMenu m_Parent;
        }

        MessageWindow messageWindow;
        IContextMenu comInterface;
        IContextMenu2 comInterface2;
        IContextMenu3 comInterface3;
        const int cmdFirst = 0x8000;
    }

    public class BHID
    {
        // BHID_SFUIObject
        public static Guid SFObject
        {
            get { return m_SFObject; }
        }

        public static Guid SFUIObject
        {
            get { return m_SFUIObject; }
        }

        //Guid("3981e224-f559-11d3-8e3a-00c04f6837d5");
        // Guid BHID_SFUIObject= Guid("3981e225-f559-11d3-8e3a-00c04f6837d5");
        static Guid m_SFObject = new Guid("3981e224-f559-11d3-8e3a-00c04f6837d5");
        static Guid m_SFUIObject = new Guid("3981e225-f559-11d3-8e3a-00c04f6837d5");
    }

    public static class ShellExec
    {
        /// <summary>
        /// Invokes the Copy command on the shell item(s).
        /// </summary>
        public static void InvokeCopy(IContextMenu comInterface)
        {
            InvokeVerb(comInterface, "copy");
        }

        /// <summary>
        /// Invokes the Copy command on the shell item(s).
        /// </summary>
        public static void InvokeCut(IContextMenu comInterface)
        {
            InvokeVerb(comInterface, "cut");
        }

        /// <summary>
        /// Invokes the Delete command on the shell item(s).
        /// </summary>
        public static void InvokeDelete(IContextMenu comInterface)
        {
            try
            {
                InvokeVerb(comInterface, "delete");
            }
            catch (COMException e)
            {
                // Ignore the exception raised when the user cancels
                // a delete operation.
                if (e.ErrorCode != unchecked((int)0x800704C7) &&
                    e.ErrorCode != unchecked((int)0x80270000))
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Invokes the Paste command on the shell item(s).
        /// </summary>
        public static void InvokePaste(IContextMenu comInterface)
        {
            InvokeVerb(comInterface, "paste");
        }

        /// <summary>
        /// Invokes the Rename command on the shell item.
        /// </summary>
        public static void InvokeRename(IContextMenu comInterface)
        {
            InvokeVerb(comInterface, "rename");
        }

        /// <summary>
        /// Invokes the specified verb on the shell item(s).
        /// </summary>
        public static void InvokeVerb(this IContextMenu comInterface, string verb)
        {
            CMINVOKECOMMANDINFO invoke = new CMINVOKECOMMANDINFO();
            invoke.cbSize = (uint)Marshal.SizeOf(invoke);
            invoke.verb = Marshal.StringToBSTR(verb);   // invoke.verb = verb;

            // comInterface.InvokeCommand(ref invoke);
            IntPtr refInvoke = Marshal.AllocHGlobal(Marshal.SizeOf(invoke));
            Marshal.StructureToPtr(invoke, refInvoke, true);

            comInterface.InvokeCommand(refInvoke);

            //// Marshal.PtrToStructure(unmanagedAddr, managedObj);
            Marshal.FreeHGlobal(refInvoke);
            refInvoke = IntPtr.Zero;
        }

        public static void CallShellItemVerb(string parsingPath, string verb)
        {
            if (parsingPath == null)
                throw new ArgumentNullException("parsingPath");

            if (verb == null)
            {
                verb = "open";
            }

            // TODO

            // get an item from the path
            //IShellItem_ item;
            //int hr = SHCreateItemFromParsingName(parsingPath, IntPtr.Zero, typeof(IShellItem_).GUID, out item);
            //if (hr < 0)
            //    throw new Win32Exception(hr);

            //// get the context menu from the item
            //IContextMenu menu;
            //Guid BHID_SFUIObject = new Guid("{3981e225-f559-11d3-8e3a-00c04f6837d5}");
            //hr = item.BindToHandler(IntPtr.Zero, BHID.SFUIObject, typeof(IContextMenu).GUID, out menu);
            //if (hr < 0)
            //    throw new Win32Exception(hr);

            //// build a fake context menu so we can scan it for the verb's menu id
            //ContextMenu cm = new ContextMenu();
            //hr = menu.QueryContextMenu(cm.Handle, 0, 0, -1, CMF.CMF_NORMAL);
            //if (hr < 0)
            //    throw new Win32Exception(hr);

            //int count = GetMenuItemCount(cm.Handle);
            //int verbId = -1;
            //for (int i = 0; i < count; i++)
            //{
            //    int id = GetMenuItemID(cm.Handle, i);
            //    if (id < 0)
            //        continue;

            //    StringBuilder sb = new StringBuilder(256);
            //    hr = menu.GetCommandString(id, GCS_VERBW, IntPtr.Zero, sb, sb.Capacity);
            //    if (sb.ToString() == verb)
            //    {
            //        verbId = id;
            //        break;
            //    }
            //}

            //if (verbId < 0)
            //    throw new Win32Exception("Verb '" + verb + "' is not supported by the item");

            //// call that verb
            //CMINVOKECOMMANDINFO ci = new CMINVOKECOMMANDINFO();
            //ci.cbSize = Marshal.SizeOf(typeof(CMINVOKECOMMANDINFO));
            //ci.lpVerb = (IntPtr)verbId;
            //hr = menu.InvokeCommand(ref ci);
            //if (hr < 0)
            //    throw new Win32Exception(hr);
        }
    }

    //  NamespaceExtension version of SHCreateDefaultContextMenu
    // int IShellFolder_GetUIObjectOf(
    //    IShellFolder psf, IdList idListAbsolute,
    //    IShellNamespaceFolder proxyFolder,
    //    IntPtr hwndOwner, uint cidl, IntPtr apidl,
    //    ref Guid riid, uint rgfReserved, out IntPtr ppv)
    //{
    //    ppv = IntPtr.Zero;

    //    //  Get the ID lists from the array of PIDLs provided.
    //    var idLists = PidlManager.APidlToIdListArray(apidl, (int)cidl);

    //    if (riid == typeof(IContextMenu).GUID || riid == typeof(IContextMenu2).GUID
    //        || riid == typeof(IContextMenu3).GUID)
    //    {
    //        //  If the folder implments the context menu provider, we can use that.
    //        var contextMenuProvider = proxyFolder as IShellNamespaceFolderContextMenuProvider;
    //        if (contextMenuProvider != null)
    //        {
    //            ppv = Marshal.GetComInterfaceForObject(
    //                contextMenuProvider.CreateContextMenu(idListAbsolute, idLists),
    //                typeof(IContextMenu));
    //            return WinError.S_OK;
    //        }
    //        var dcm = new DEFCONTEXTMENU
    //        {
    //            hwnd = hwndOwner,
    //            pcmcb = null,
    //            pidlFolder = PidlManager.IdListToPidl(idListAbsolute),
    //            psf = psf,
    //            cidl = cidl,
    //            apidl = apidl,
    //            punkAssociationInfo = IntPtr.Zero,
    //            cKeys = 0,
    //            aKeys = null
    //        };

    //        //  Create the default context menu.
    //        var result = Shell32.SHCreateDefaultContextMenu(dcm, riid, out ppv);

    //        return WinError.S_OK;
    //    }

    //    return WinError.E_NOINTERFACE;
    //}
}

/* Menu https://msdn.microsoft.com/en-us/library/aa770042(v=vs.85).aspx#Enable_Disable_Menu
  
   #define IDR_BROWSE_CONTEXT_MENU  24641
   #define SHDVID_GETMIMECSETMENU   27
   #define SHDVID_ADDMENUEXTENSIONS 53

   HRESULT hr;
   HINSTANCE hinstSHDOCLC;
   HWND hwnd;
   HMENU hMenu;
   CComPtr<IOleCommandTarget> spCT;
   CComPtr<IOleWindow> spWnd;
   MENUITEMINFO mii = {0};
   CComVariant var, var1, var2;

   hr = pcmdTarget->QueryInterface(IID_IOleCommandTarget, (void**)&spCT);
   hr = pcmdTarget->QueryInterface(IID_IOleWindow, (void**)&spWnd);
   hr = spWnd->GetWindow(&hwnd);

   hinstSHDOCLC = LoadLibrary(TEXT("SHDOCLC.DLL"));
   
   if (hinstSHDOCLC == NULL)
   {
       // Error loading module -- fail as securely as possible.
       return;
   }

   hMenu = LoadMenu(hinstSHDOCLC,
                    MAKEINTRESOURCE(IDR_BROWSE_CONTEXT_MENU));

   hMenu = GetSubMenu(hMenu, dwID);

 // If the placeholder is gone or was never there, then just exit.
if (GetMenuState(hMenu, IDM_MENUEXT_PLACEHOLDER, MF_BYCOMMAND) != (UINT) -1)
{
   InsertMenu(hMenu,                    // The Context Menu
       IDM_MENUEXT_PLACEHOLDER,         // The item to insert before
       MF_BYCOMMAND|MF_STRING,          // Insert by item ID and str value
       IDM_MENUEXT_FIRST__ + nExtCur,   // The command ID
       (LPTSTR)aOpts[nExtCur].pstrText);// Some menu command text
	
   // Remove placeholder.
   DeleteMenu(hMenu, IDM_MENUEXT_PLACEHOLDER, MF_BYCOMMAND);
}

The menu IDs for extensions fall between IDM_MENUEXT_FIRST__ and IDM_MENUEXT_LAST__ for a maximum of 32 custom commands.
Enabling and Disabling Menu Items
The menus in Internet Explorer use resource identifiers from mshtmcid.h to specify which command is executed when the menu item is clicked.
Using the same resource IDs, you can easily determine whether the command has been enabled or disabled by calling IOleCommandTarget::QueryStatus, as 

 for (i = 0; i < GetMenuItemCount(hMenu); i++)
{
    OLECMD olecmd.cmdID = GetMenuItemID(hMenu, i);
    if (olecmd.cmdID > 0)
    {
        UINT mf;
        spCmdTarget->QueryStatus(&CGID_MSHTML, 1, &olecmd, NULL);
        switch (olecmd.cmdf)
        {
        case OLECMDSTATE_UP:
        case OLECMDSTATE_NINCHED:
            mf = MF_BYCOMMAND | MF_ENABLED | MF_UNCHECKED;
            break;

        case OLECMDSTATE_DOWN:
            mf = MF_BYCOMMAND | MF_ENABLED | MF_CHECKED;
            break;

        case OLECMDSTATE_DISABLED:
        default:
            mf = MF_BYCOMMAND | MF_DISABLED | MF_GRAYED;
            break;
        }
        CheckMenuItem(hMenu, olecmd.cmdID, mf);
        EnableMenuItem(hMenu, olecmd.cmdID, mf);
    }
}

CGID_EditStateCommands Mshtml.h
CGID_Explorer Shlguid.h
CGID_MSHTML Mshtmhst.h
CGID_ShellDocView   Shlguid.h
IDM_CONTEXT Mshtmcid.h
SID_SEditCommandTarget  Mshtml.h
SID_SShellBrowser   Shlguid.h
SZ_HTML_CLIENTSITE_OBJECTPARAM  Mshtmhst.h

    // MSHTML requests to display its context menu
	STDMETHOD(ShowContextMenu)(DWORD dwID, POINT* pptPosition, IUnknown* pcmdTarget, IDispatch* pDispatchObjectHit)
	{
		if (::GetKeyState(VK_LBUTTON)<0 || ::GetKeyState(VK_LBUTTON)<0)
			return ShowContextMenuEx(pptPosition);

		if (m_nIeMenuNoCstm)
			return S_FALSE;

		if ( dwID!=CONTEXT_MENU_DEFAULT && dwID != CONTEXT_MENU_TEXTSELECT
			&& dwID!=CONTEXT_MENU_ANCHOR )
			return S_FALSE;

		enum {
			IDR_BROWSE_CONTEXT_MENU  = 24641,
			IDR_FORM_CONTEXT_MENU	 = 24640,
			SHDVID_GETMIMECSETMENU	 = 27,
			SHDVID_ADDMENUEXTENSIONS = 53,
		};

		HRESULT hr;
		HINSTANCE hinstSHDOCLC;
		HWND hwnd;
		HMENU hMenu, hMenuSub;
		CComPtr<IOleCommandTarget> spCT;
		CComPtr<IOleWindow> spWnd;
		CComVariant var, var1, var2;

		hr = pcmdTarget->QueryInterface(IID_IOleCommandTarget, (void**)&spCT);
		hr = pcmdTarget->QueryInterface(IID_IOleWindow, (void**)&spWnd);
		hr = spWnd->GetWindow(&hwnd);

		hinstSHDOCLC = LoadLibrary(TEXT("SHDOCLC.DLL"));
		if (hinstSHDOCLC == NULL) { 	//+++
			hinstSHDOCLC = LoadLibrary( TEXT("ieframe.dll") );		// for vista
			if (hinstSHDOCLC == NULL)
				return S_FALSE;
		}

		hMenu = LoadMenu(hinstSHDOCLC, MAKEINTRESOURCE(IDR_BROWSE_CONTEXT_MENU));
		hMenuSub = GetSubMenu(hMenu, dwID);

		// Get the language submenu
		hr = spCT->Exec(&CGID_ShellDocView, SHDVID_GETMIMECSETMENU, 0, NULL, &var);
		MENUITEMINFO mii = {0};
		mii.cbSize = sizeof(mii);
		mii.fMask  = MIIM_SUBMENU;
		mii.hSubMenu = (HMENU) var.byref;
		SetMenuItemInfo(hMenuSub, IDM_LANGUAGE, FALSE, &mii);

		// Insert Shortcut Menu Extensions from registry
		V_VT(&var1) = VT_INT_PTR;
		V_BYREF(&var1) = hMenuSub;

		V_VT(&var2) = VT_I4;
		V_I4(&var2) = dwID;

		hr = spCT->Exec(&CGID_ShellDocView, SHDVID_ADDMENUEXTENSIONS, 0, &var1, &var2);

		CSimpleMap<DWORD, DWORD>	mapCmd;
		CSimpleArray<HMENU> 		aryDestroyMenu;
		CstmContextMenu(hMenuSub, dwID, mapCmd, aryDestroyMenu);

		// Show shortcut menu
		int iSelection = ::TrackPopupMenu(hMenuSub,
									  TPM_LEFTALIGN | TPM_RIGHTBUTTON | TPM_RETURNCMD,
									  pptPosition->x,
									  pptPosition->y,
									  0,
									  GetTopLevelWindow(),
									  (RECT*)NULL);

		// Send selected shortcut menu item command to shell
		LRESULT lRes = S_OK;
		if ( iSelection != 0 )
			lRes = ::SendMessage(hwnd, WM_COMMAND, iSelection, NULL);
		BOOL bSendFrm = FALSE;
		if (32772<=iSelection && iSelection<=34000)
			bSendFrm = TRUE;
		else if (0x1001<=iSelection && iSelection<=0x7530 || 0x7000<=iSelection && iSelection<=0x7FFF)
			bSendFrm = TRUE;
		else if (ID_INSERTPOINT_SCRIPTMENU<=iSelection && iSelection<=ID_INSERTPOINT_SCRIPTMENU_END)
			bSendFrm = TRUE;

		if (bSendFrm==FALSE && mapCmd.Lookup((DWORD)iSelection))
			bSendFrm = TRUE;

		if (bSendFrm)
			lRes = ::SendMessage(GetTopLevelWindow(), WM_COMMAND, iSelection, NULL);

		for( int ii=0; ii<aryDestroyMenu.GetSize(); ii++ )
			::DestroyMenu(aryDestroyMenu[ii]);

		for ( int ii=0; ii<mapCmd.GetSize(); ii++)
			::RemoveMenu( hMenuSub, mapCmd.GetKeyAt( ii ), MF_BYCOMMAND );

		::DestroyMenu(hMenu);
		FreeLibrary(hinstSHDOCLC);
		return S_OK;
	}
*/
