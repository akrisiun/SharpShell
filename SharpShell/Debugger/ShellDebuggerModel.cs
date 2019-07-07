using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;
using SharpShell.Interop;
using SharpShell.Pidl;
using IServiceProvider = SharpShell.Interop.IServiceProvider;
using ComTypes = System.Runtime.InteropServices.ComTypes;
using System.Diagnostics;
using System.Windows.Forms;
using SharpShell.Helpers;

namespace ShellDebugger
{
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.None)]
    public class ShellDebuggerModel : IShellBrowser,
        IServiceProvider, ICommDlgBrowser, IOleCommandTarget,  // IFolderView,
        IDisposable
    {
        public ShellDebuggerForm Form;

        #region ctor, dispose, Destroyed

        public ShellDebuggerModel(ShellDebuggerForm form)
        {
            this.Form = form;

            //  Get the desktop folder PIDL and interface.
            Shell32.SHGetFolderLocation(IntPtr.Zero, CSIDL.CSIDL_DESKTOP, IntPtr.Zero, 0, out desktopFolderPidl);
            Shell32Ext.SHGetDesktopFolder(out desktopFolder);

            Form.Load += new System.EventHandler(ShellDebuggerForm_Load);
        }

        public void Bind()
        {
            var shellTreeView = Form.shellTreeView;
            shellTreeView.OnShellItemSelected += ShellTreeView_DesktopSelected;
            shellTreeView.Bind();

            var node = shellTreeView.SelectedNode;
            if (node == null)
                return;
            shellTreeView.DoSelect(node);
            shellTreeView.Focus();
        }

        private void ShellTreeView_DesktopSelected(object sender, ShellItemEventArgs e)
        {
            var shellTreeView = Form.shellTreeView;
            shellTreeView.OnShellItemSelected -= ShellTreeView_DesktopSelected;
            shellTreeView.OnShellItemSelected += shellTreeView_OnShellItemSelected;

            if (shellTreeView.Nodes[0].Nodes.Count > 1)
            {
                var node = shellTreeView.Nodes[0].Nodes[0];

                //Form.SuspendLayout();

                shellTreeView.DoSelect(node);
                node.Expand();

                //Form.ResumeLayout(false);
            }
        }

        public void Dispose()
        {
            Form = null;
        }

        public void OnHandleDestroyed()
        {
            currentAbsolutePidl = IntPtr.Zero;

            // Release the IShellView
            if (ShellView != null)
            {
                ShellView.UIActivate((uint)SVUIA_STATUS.SVUIA_DEACTIVATE);
                ShellView.DestroyViewWindow();

                //  The shell view may have come from COM but may be a SharpShell view, so check if it's COM
                //  before we release it.
                if (Marshal.IsComObject(ShellView))
                    Marshal.ReleaseComObject(ShellView);

                ShellView = null;
            }
        }

        #endregion

        #region properties

        public IShellFolder currentFolder { get; set; }
        public IntPtr currentAbsolutePidl { get; set; }

        public IntPtr lastViewPidl = IntPtr.Zero;

        public string LastSelected
        {
            get
            {
                if (lastViewPidl == IntPtr.Zero)
                    return null;

                StringBuilder pszPath = new StringBuilder();
                pszPath.Length = 256; // TODO maxLength
                if (Shell32.SHGetPathFromIDList(lastViewPidl, pszPath) == WinError.S_OK)
                    return pszPath.ToString();
                else
                    return null;
            }
        }

        private readonly IShellFolder2 desktopFolder;
        private readonly IntPtr desktopFolderPidl;

        public IntPtr DesktopFolderPidl { get { return desktopFolderPidl; } }
        public IShellFolder2 DesktopFolder { get { return desktopFolder; } }
        public IShellView ShellView { get; set; }
        public IntPtr hWndListView { get; set; }

        public static readonly FOLDERVIEWMODE folderViewMode = FOLDERVIEWMODE.FVM_DETAILS;

        public static readonly FOLDERFLAGS folderFlags =
                FOLDERFLAGS.FWF_SHOWSELALWAYS // | FOLDERFLAGS.FWF_CHECKSELECT 
                | FOLDERFLAGS.FWF_AUTOARRANGE | FOLDERFLAGS.FWF_USESEARCHFOLDER
                | FOLDERFLAGS.FWF_SHOWSELALWAYS;
        /* FOLDERFLAGS.FWF_SINGLESEL | FOLDERFLAGS.FWF_NOWEBVIEW; */

        #endregion

        void ShellDebuggerForm_Load(object sender, EventArgs e)
        {
            Form.shellTreeView.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            var splitContainerTreeAndDetails = Form.splitContainerTreeAndDetails;
            splitContainerTreeAndDetails.Dock = DockStyle.Fill;
            splitContainerTreeAndDetails.SizeChanged += SplitContainerTreeAndDetails_SizeChanged;
        }

        void shellTreeView_OnShellItemSelected(object sender, ShellItemEventArgs e)
        {
            // Browse to the selected item if it is a folder.
            // (IShellBrowser)
            this.BrowseObject(e.ShellItem.PIDL, SBSP.SBSP_SAMEBROWSER | SBSP.SBSP_ABSOLUTE);

            //if (this.lastViewPidl != IntPtr.Zero){ }
        }

        #region IOleBrowser implementation

        int IOleWindow.GetWindow(out IntPtr phwnd)
        {
            return ((IShellBrowser)this).GetWindow(out phwnd);
        }

        int IOleWindow.ContextSensitiveHelp(bool fEnterMode)
        {
            return ((IShellBrowser)this).ContextSensitiveHelp(fEnterMode);
        }

        #endregion

        #region IOleCommandTarget Members

        void IOleCommandTarget.QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, ref OLECMDTEXT CmdText)
        {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        void IOleCommandTarget.Exec(ref Guid pguidCmdGroup, uint nCmdId, uint nCmdExecOpt, ref object pvaIn, ref object pvaOut)
        {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        #endregion

        #region IShellBrowser implementation

        int IShellBrowser.GetWindow(out IntPtr phwnd)
        {
            phwnd = GetFolderViewHost();
            return WinError.S_OK;
        }

        int IShellBrowser.ContextSensitiveHelp(bool fEnterMode)
        {
            return WinError.E_NOTIMPL;
        }

        int IShellBrowser.InsertMenusSB(IntPtr hmenuShared, ref IntPtr lpMenuWidths)
        {
            return WinError.E_NOTIMPL;
        }

        int IShellBrowser.SetMenuSB(IntPtr hmenuShared, IntPtr holemenuRes, IntPtr hwndActiveObject)
        {
            return WinError.E_NOTIMPL;
        }

        int IShellBrowser.RemoveMenusSB(IntPtr hmenuShared)
        {
            return WinError.E_NOTIMPL;
        }

        int IShellBrowser.SetStatusTextSB(string pszStatusText)
        {
            return WinError.E_NOTIMPL;
        }

        int IShellBrowser.EnableModelessSB(bool fEnable)
        {
            return WinError.E_NOTIMPL;
        }

        int IShellBrowser.TranslateAcceleratorSB(IntPtr pmsg, short wID)
        {
            return WinError.S_OK;
        }

        int IShellBrowser.GetViewStateStream(long grfMode, ref IStream ppStrm)
        {
            ppStrm = null;
            return WinError.E_NOTIMPL;
        }

        int IShellBrowser.GetControlWindow(uint id, ref IntPtr phwnd)
        {
            phwnd = IntPtr.Zero;
            return WinError.S_FALSE;
        }

        int IShellBrowser.SendControlMsg(uint id, uint uMsg, short wParam, long lParam, ref long pret)
        {
            //  pret = 0;
            return WinError.E_NOTIMPL;
        }

        int IShellBrowser.OnViewWindowActive(IShellView pshv)
        {
            return WinError.E_NOTIMPL;
        }

        int IShellBrowser.SetToolbarItems(IntPtr lpButtons, uint nButtons, uint uFlags)
        {
            return WinError.E_NOTIMPL;
        }

        #endregion

        int IShellBrowser.QueryActiveShellView(ref IShellView ppshv)
        {
            Marshal.AddRef(Marshal.GetIUnknownForObject(ShellView));

            // (the IShellView object returned through SHCreateShellFolderView) 
            /// <param name="ppshv">The address of the pointer to the currently active Shell view object.</param>
            /// <returns>Returns S_OK if successful, or a COM-defined error value otherwise.</returns>
            //int QueryActiveShellView(ref IShellView ppshv);

            ppshv = ShellView;
            return WinError.S_OK;
        }

        //IShellBrowser.
        public int BrowseObject(IntPtr pidl, SBSP wFlags)
        {
            if (Form.InvokeRequired)
            {
                AutoResetEvent theEvent = new AutoResetEvent(false);
                int result = WinError.E_FAIL;
                Form.Invoke((Action)(() =>
                {
                    result = ((IShellBrowser)this).BrowseObject(pidl, wFlags);
                    theEvent.Set();
                }));
                theEvent.WaitOne();
                return result;
            }

            this.hWndListView = IntPtr.Zero;
            var hr = PidlFolderMethods.BrowseObject(this, pidl, wFlags);

            if (hWndListView != IntPtr.Zero)
                ResizeListView(this.hWndListView);

            AfterSelectPath(pidl);

            return hr;
        }


        #region IServiceProvider implementation

        int IServiceProvider.QueryService(ref Guid guidService, ref Guid riid, out IntPtr ppvObject)
        {
            ppvObject = IntPtr.Zero;

            if (riid == typeof(IOleCommandTarget).GUID)
            {
                ppvObject = Marshal.GetComInterfaceForObject(this,
                    typeof(IOleCommandTarget));
            }
            else
                if (riid == typeof(IShellBrowser).GUID)
                {
                    ppvObject = Marshal.GetComInterfaceForObject(this, typeof(IShellBrowser));
                    return WinError.S_OK;
                }
                else
                {
                    Debugger.Log(0, "Warn", "QueryService " + riid.ToString());
                }

            //  Debugger.Log(0, "Warn", "QueryService " + shellBrowserGuid.GetType().ToString());

#if DEBUG
            Debugger.Log(0, "Warn", "QueryService GUID=" + riid.ToString() + "\n");
            //            QueryService GUID=fc992f1f-debb-4596-b355-50c7a6dd1222The thread 0x645c8 has exited with code 259 (0x103).
            //QueryService GUID=                  f002a70d-e74b-4721-9243-5704338cfc2f QueryService GUID=18140cbd-aa23-4384-a38d-6a8d3e2be505QueryService GUID=dfbc7e30-f9e5-455f-88f8-fa98c1e494caQueryService GUID=dfbc7e30-f9e5-455f-88f8-fa98c1e494caQueryService System.GuidQueryService GUID=dfbc7e30-f9e5-455f-88f8-fa98c1e494caQueryService GUID=dfbc7e30-f9e5-455f-88f8-fa98c1e494caQueryService System.GuidQueryService GUID=dfbc7e30-f9e5-455f-88f8-fa98c1e494caQueryService GUID=dfbc7e30-f9e5-455f-88f8-fa98c1e494caQueryService System.GuidQueryService GUID=dfbc7e30-f9e5-455f-88f8-fa98c1e494caQueryService GUID=dfbc7e30-f9e5-455f-88f8-fa98c1e494caQueryService System.GuidQueryService GUID=dfbc7e30-f9e5-455f-88f8-fa98c1e494caQueryService GUID=dfbc7e30-f9e5-455f-88f8-fa98c1e494caQueryService System.GuidQueryService GUID=00000000-0000-0000-c000-000000000046
            //            QueryService GUID=      dd1e21cc-e2c7-402c-bf05-10328d3f6bad A first chance exception of type 'System.AccessViolationException' occurred in ShellDebugger.exe

            var guid_ICommDlgBrowser = typeof(ICommDlgBrowser).GUID;
            if (riid == guid_ICommDlgBrowser)
            {
                //    ppvObject = this;
                //    return WinError.S_OK;
            }

            var lastGuid = new Guid("{dd1e21cc-e2c7-402c-bf05-10328d3f6bad}");
            if (riid == lastGuid)
            {
                // break point
            }
#endif
            return WinError.E_NOINTERFACE;

        }

        #endregion

        #region Host commands

        private IntPtr GetFolderViewHost()
        {
            return Form.splitContainerTreeAndDetails.Panel2.Handle;
        }

        void SplitContainerTreeAndDetails_SizeChanged(object sender, EventArgs e)
        {
            ResizeListView(this.hWndListView);
        }

        void ResizeListView(IntPtr hHost)
        {
            var Width = Form.splitContainerTreeAndDetails.Width
                      - Form.shellTreeView.Width - 3;  // minus splitter (or border)
            var Height = Form.splitContainerTreeAndDetails.Height;

            User32.MoveWindow(hHost, 0, 0 // rect.left, rect.top
                , Width, Height, true);
        }

        // ICommDlgBrowser.
        public int OnDefaultCommand(IShellView ppshv)
        {
            return WinError.E_NOTIMPL; // S_OK;
        }

        int ICommDlgBrowser.OnStateChange(IShellView ppshv, CDBOSC uChange)
        {
            if (uChange == CDBOSC.CDBOSC_SELCHANGE)
            {
                var obj = GetSelectionDataObject(ppshv);
                if (obj == null)
                    return WinError.S_OK;

                var medium = new STGMEDIUM();
                var formatEtc = new FORMATETC();
                formatEtc.cfFormat = (short)15; // CF_HDROP = 15 (short)System.Windows.Forms.DataFormats.GetFormat(resourceName).Id;
                formatEtc.ptd = IntPtr.Zero;
                formatEtc.dwAspect = DVASPECT.DVASPECT_CONTENT;
                formatEtc.lindex = -1;
                formatEtc.tymed = TYMED.TYMED_HGLOBAL;

                string s = null;

                try
                {
                    obj.GetData(ref formatEtc, out medium);

                    //ReadFileListFromHandle(IntPtr hdrop) 
                    IntPtr hdrop = medium.unionmember;

                    StringBuilder sb = new StringBuilder(260); // TODO NativeMethods.MAX_PATH);

                    int count = DragQueryFile(new HandleRef(null, hdrop), unchecked((int)0xFFFFFFFF), null, 0);
                    int i = 0;
                    int charlen = DragQueryFile(new HandleRef(null, hdrop), i, sb, sb.Capacity);
                    s = sb.ToString();
                }
                catch (COMException) { }
                catch (Exception) { }


                //var h = medium.pUnkForRelease

                //        TCHAR path[MAX_PATH];

                //        // check if this single selection (or multiple)
                //        CIDA* cida = (CIDA*)stgmed.hGlobal;
                //        if (cida->cidl == 1)
                //        { 
            }

            return WinError.S_OK;
        }

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        //[ResourceExposure(ResourceScope.None)]
        public static extern int DragQueryFile(HandleRef hDrop, int iFile, StringBuilder lpszFile, int cch);

        //System.Runtime.InteropServices.ComTypes
        ComTypes.IDataObject GetSelectionDataObject(IShellView shellView)
        {
            IntPtr result = IntPtr.Zero;

            if (shellView == null)
            {
                return null;
            }

            shellView.GetItemObject(_SVGIO.SVGIO_SELECTION,
                typeof(ComTypes.IDataObject).GUID, ref result);

            if (result != IntPtr.Zero)
            {
                var wrapped = (ComTypes.IDataObject)
                    Marshal.GetTypedObjectForIUnknown(result, typeof(ComTypes.IDataObject));
                return wrapped;
            }
            return null;
        }

        int ICommDlgBrowser.IncludeObject(IShellView ppshv, IntPtr pidl)
        {
            lastViewPidl = pidl;    // actually last filtered Pidl

            //    //SHGetPathFromIDList(IntPtr pidl, [MarshalAs(UnmanagedType.LPTStr)] StringBuilder pszPath);

            //    //Guid guid = Shell32.IID_IDataObject; 
            //    //var hr = ppshv.GetItemObject(_SVGIO.SVGIO_ALLVIEW, ref guid, ref newPidl);
            //    /// Gets an interface that refers to data presented in the view.
            //    //HRESULT GetData( [in]  FORMATETC *pformatetcIn, [out] STGMEDIUM *pmedium
            //    //IID_PPV_ARGS(&pDataObj))) {
            //    //var hr = ppshv.GetItemObject(_SVGIO.SVGIO_ALLVIEW, ref 
            //    //    IID_IDataObject   ShellView::GetItemObject
            //    //    _SVGIO uItem, ref Guid riid, ref IntPtr ppv);
            //    //SVGIO_ALLVIEW

            //    // ICommDlgBrowser3::GetCurrentFilter(), t
            return WinError.S_OK;
        }

        /*
         * 
         * 
         * IShellFolder * pShellFolder;
                                                                if (SUCCEEDED(pPersistFolder->QueryInterface(IID_IShellFolder, (LPVOID*)&pShellFolder)))
                                                                {
                                                                    // now compare the items of the refreshed view with the items
                                                                    // of the view before the new folder was created.
                                                                    // The difference should be the new folder...
                                                                    nCount2 = 0;
                                                                    if (SUCCEEDED(pFolderView->ItemCount(SVGIO_ALLVIEW, &nCount2)))
                                                                    {
                                                                        for (int i=0; i<nCount2; ++i)
                                                                        {
                                                                            LPITEMIDLIST pidl;
                                                                            pFolderView->Item(i, &pidl);
         * 
                https://github.com/tablacus/TablacusExplorer/blob/master/TE/TE.cpp
                IShellView *pSV;
                    if SUCCEEDED(pSB->QueryActiveShellView(&pSV)) {
                        if FAILED(lpfnSHGetIDListFromObject(pSV, ppidl)) {
        #ifdef _W2000
                            //Windows 2000
                            IDataObject *pDataObj;
                            if SUCCEEDED(pSV->GetItemObject(SVGIO_ALLVIEW, IID_PPV_ARGS(&pDataObj))) {
                                long nCount;
                                LPITEMIDLIST *pidllist = IDListFormDataObj(pDataObj, &nCount);
                                *ppidl = pidllist[0];
                                while (--nCount >= 1) {
                                    if (pidllist) {
                                        teCoTaskMemFree(pidllist[nCount]);
                                    }
                                }
                                delete [] pidllist;
                                pDataObj->Release();
                            }
        #endif
                        }
                        pSV->Release();
                    }
                    pSB->Release();
        */

        #endregion

        // ICommDlgBrowser
        public int OnDefaultCommand(IntPtr ppshv)
        {
            return WinError.S_OK; //  .E_NOTIMPL;
        }

        public int GetDefaultMenuText(IShellView shellView, IntPtr buffer, int bufferMaxLength)
        {
            return WinError.E_NOTIMPL;
        }

        public int GetCurrentFilter(StringBuilder pszFileSpec, int cchFileSpec)
        {
            return WinError.S_OK;
        }

        public void AfterSelectPath(IntPtr pidl)
        {
            var path = PidlManager.GetPidlPath(pidl);

            Form.toolAddress.Text = path ?? "";
        }

        // ICommDlgBrowser3.
        // int Notify(
        // int GetCurrentFilter([out] LPWSTR pszFileSpec, [in]  int cchFileSpec);
        // int OnColumnClicked(IShellView ppshv, int iColumn)
        // int OnPreViewCreated(IShellView ppshv)
    }

    public static class PidlFolderMethods
    {
        public static int BrowseObject(this ShellDebuggerModel model, IntPtr pidl, SBSP wFlags)
        {
            int hr;
            IntPtr folderTmpPtr;
            IShellFolder folderTmp = null;
            IntPtr pidlTmp;

            //  We'll need the shell folder GUID.
            var shellFolderGuid = typeof(IShellFolder).GUID;
            var desktopFolder = model.DesktopFolder;
            var currentFolder = model.currentFolder;
            var currentAbsolutePidl = model.currentAbsolutePidl;

            //  Check to see if we have a desktop pidl, relative pidl or absolite pidl.
            if (Shell32.ILIsEqual(pidl, model.DesktopFolderPidl))
            {
                //  The provided PIDL is the desktop folder.
                pidlTmp = Shell32.ILClone(model.DesktopFolderPidl);
                folderTmp = model.DesktopFolder;
            }
            else if ((wFlags & SBSP.SBSP_RELATIVE) != 0)
            {
                // SBSP_RELATIVE - pidl is relative from the current folder
                if ((hr = currentFolder.BindToObject(pidl, IntPtr.Zero,
                    shellFolderGuid,
                    out folderTmpPtr)) != WinError.S_OK)
                    return WinError.E_FAIL;

                pidlTmp = Shell32.ILCombine(currentAbsolutePidl, pidl);

                folderTmp = (IShellFolder)Marshal.GetObjectForIUnknown(folderTmpPtr);
            }
            else
            {
                // SBSP_ABSOLUTE - pidl is an absolute pidl (relative from desktop)
                pidlTmp = Shell32.ILClone(pidl);
                if ((hr = desktopFolder.BindToObject(pidlTmp, IntPtr.Zero,
                    shellFolderGuid,
                    out folderTmpPtr)) != WinError.S_OK)
                    return WinError.E_FAIL;

                folderTmp = (IShellFolder)Marshal.GetObjectForIUnknown(folderTmpPtr);
            }

            if (folderTmp == null)
            {
                Shell32.ILFree(pidlTmp);
                return WinError.E_FAIL;
            }

            // Check that we have a new pidl
            if (Shell32.ILIsEqual(pidlTmp, currentAbsolutePidl))
            {
                Shell32.ILFree(pidlTmp);
                return WinError.S_OK;
            }

            model.hWndListView = IntPtr.Zero;
            model.lastViewPidl = IntPtr.Zero;

            var ret = ChangeFolder(model, pidl, wFlags, folderTmp, pidlTmp);

            //var path = PidlManager.GetPidlDisplayName(pidlTmp);
            //if (path == "Control Panel" || model.lastViewPidl == IntPtr.Zero)
            //{
            //    IEnumIDList ppenumIDList = null;
            //    var hr3 = folderTmp.EnumObjects(model.Form.Handle, 
            //        SHCONTF.SHCONTF_INCLUDEHIDDEN | SHCONTF.SHCONTF_INCLUDESUPERHIDDEN | SHCONTF.SHCONTF_NONFOLDERS, // | SHCONTF.SHCONTF_FOLDERS , 
            //        out ppenumIDList);

            //    if (ppenumIDList == null)       // if empty list
            //    {
            //        IntPtr iShellFolderPtr = IntPtr.Zero;
            //        var guidFolder2 = typeof(IShellFolder2).GUID;
            //        folderTmpPtr = IntPtr.Zero;

            //        if ((hr = desktopFolder.BindToObject(pidlTmp, IntPtr.Zero,
            //            ref guidFolder2, // shellFolderGuid,
            //            out folderTmpPtr)) == WinError.S_OK)
            //        {
            //            folderTmp = (IShellFolder2)Marshal.GetObjectForIUnknown(folderTmpPtr);

            //            var hr2 = ShellObject.CreateViewObject(folderTmp, model.Form.Handle, ref guidFolder2, out iShellFolderPtr);
            //        }

            //        if (iShellFolderPtr != IntPtr.Zero)
            //        {
            //            var ShellFolder = (IShellFolder2)
            //                           Marshal.GetObjectForIUnknown(iShellFolderPtr);
            //        }
            //    }
            //}

            return ret == WinError.S_OK ? WinError.S_OK : WinError.E_FAIL;
        }

        static int ChangeFolder(ShellDebuggerModel model, IntPtr pidl,
            SBSP wFlags, IShellFolder folderTmp, IntPtr pidlTmp)
        {
            model.currentFolder = folderTmp;
            var Form = model.Form;

            FOLDERSETTINGS fs = new FOLDERSETTINGS();
            IShellView lastIShellView = model.ShellView;
            IShellView2 lastIShellView2 = model.ShellView as IShellView2;

            if (lastIShellView != null)
                lastIShellView.GetCurrentInfo(ref fs);
            // Copy the old folder settings
            else
            {
                fs = new FOLDERSETTINGS();
                fs.fFlags = ShellDebuggerModel.folderFlags;
                fs.ViewMode = ShellDebuggerModel.folderViewMode;
            }

            // Create the IShellView
            IntPtr iShellViewPtr;
            var shellViewGuid = typeof(IShellView).GUID;
            var hr = ShellObject.CreateViewObject(folderTmp, Form.Handle,
                 ref shellViewGuid, out iShellViewPtr);

            if (hr != WinError.S_OK)
            {
                shellViewGuid = typeof(IShellView).GUID;
                hr = ShellObject.CreateViewObject(folderTmp, Form.Handle,
                    ref shellViewGuid, out iShellViewPtr);
            }

            if (hr == WinError.S_OK)
            {
                model.ShellView = null;
                model.ShellView = (IShellView) // IShellView2
                               Marshal.GetObjectForIUnknown(iShellViewPtr);
                //if (model.ShellView == null)
                //    model.ShellView = (IShellView)
                //               Marshal.GetObjectForIUnknown(iShellViewPtr);
                // int CreateViewWindow2(SV2CVW2_PARAMS lpParams);

                var hWndListView = IntPtr.Zero;
                RECT rc =
                    new RECT(0, 0,
                   Form.ClientSize.Width,
                   Form.ClientSize.Height);

                int res;
                model.lastViewPidl = IntPtr.Zero;
                var shellView = model.ShellView;

                try
                {
                    // Create the actual list view.
                    res = shellView.CreateViewWindow(lastIShellView, ref fs,
                          model, ref rc, ref hWndListView);

                    model.hWndListView = hWndListView;
                    shellView.EnableModeless(true);
                }
                catch (COMException)
                {
                    return WinError.E_FAIL;
                }

                if (res < 0)
                    return WinError.E_FAIL;

                // Release the old IShellView
                if (lastIShellView != null)
                {
                    lastIShellView.GetCurrentInfo(ref fs);
                    lastIShellView.UIActivate(SVUIA_STATUS.SVUIA_DEACTIVATE);
                    lastIShellView.DestroyViewWindow();
                }

                // Set focus to the IShellView
                model.ShellView.UIActivate(SVUIA_STATUS.SVUIA_ACTIVATE_FOCUS);
                model.currentAbsolutePidl = pidlTmp;

                if (model.lastViewPidl != IntPtr.Zero)
                {
                    //    var lastItem = model.LastSelected;
                }
                //else
                //{
                //    // empty list
                //}
            }

            return WinError.S_OK;
        }

    }
}
