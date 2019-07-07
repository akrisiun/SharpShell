using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;
using SharpShell.Interop;
using SharpShell.Pidl;
using Shell.SharpContextMenu;
using System.Linq;
using SharpShell.SharpContextMenu;

namespace SharpShell.Helpers
{

    /// <summary>
    /// Represents a ShellItem object.
    /// </summary>
    public class ShellItem : IDisposable
    {
        /// <summary>
        /// Initializes the <see cref="ShellItem"/> class.
        /// </summary>
        static ShellItem()
        {
            //  Create the lazy desktop shell folder.
            desktopShellFolder = new Lazy<ShellItem>(CreateDesktopShellFolder);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShellItem"/> class.
        /// </summary>
        public ShellItem()
        {
            //  Create the lazy path.
            path = new Lazy<string>(CreatePath);
            overlayIcon = new Lazy<Icon>(CreateOverlayIcon);
        }

        public ShellItem(IntPtr pidl, string path)
            : this()
        {
            this.Initialise(pidl, path);
        }

        /// <summary>
        /// Creates the icon.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        private Icon CreateOverlayIcon()
        {
            var fileInfo = new SHFILEINFO();
            Shell32.SHGetFileInfo(PIDL, 0, out fileInfo, (uint)Marshal.SizeOf(fileInfo),
                SHGFI.SHGFI_PIDL | SHGFI.SHGFI_ICON | SHGFI.SHGFI_ADDOVERLAYS);
            return fileInfo.hIcon != IntPtr.Zero ? Icon.FromHandle(fileInfo.hIcon) : null;
        }

        /// <summary>
        /// Creates the desktop shell folder.
        /// </summary>
        /// <returns>The desktop shell folder.</returns>
        private static ShellItem CreateDesktopShellFolder()
        {
            //  Get the desktop shell folder interface. 
            //IShellFolder desktopShellFolderInterface = null;
            //var result = Shell32.SHGetDesktopFolder(out desktopShellFolderInterface);
            IShellFolder2 desktopShellFolderInterface = null;
            var result = Shell32Ext.SHGetDesktopFolder(out desktopShellFolderInterface);

            //  Validate the result.
            if (result != 0)
            {
                //  Throw the failure as an exception.
                Marshal.ThrowExceptionForHR(result);
            }

            //  Get the dekstop PDIL.
            var desktopPIDL = IntPtr.Zero;
            result = Shell32.SHGetSpecialFolderLocation(IntPtr.Zero, CSIDL.CSIDL_DESKTOP, ref desktopPIDL);

            //  Validate the result.
            if (result != 0)
            {
                //  Throw the failure as an exception.
                Marshal.ThrowExceptionForHR(result);
            }

            //  Get the file info.
            var fileInfo = new SHFILEINFO();
            Shell32.SHGetFileInfo(desktopPIDL, 0, out fileInfo, (uint)Marshal.SizeOf(fileInfo),
                SHGFI.SHGFI_DISPLAYNAME | SHGFI.SHGFI_PIDL | SHGFI.SHGFI_SMALLICON | SHGFI.SHGFI_SYSICONINDEX);

            //  Return the Shell Folder.
            return new ShellItem
            {
                DisplayName = fileInfo.szDisplayName,
                IconIndex = fileInfo.iIcon,
                HasSubFolders = true,
                IsFolder = true,
                ShellFolderInterface = desktopShellFolderInterface,
                PIDL = desktopPIDL,
                RelativePIDL = desktopPIDL
            };
        }

        public void Initialise(IntPtr pidl, string path)
        {
            this.PIDL = pidl;
            this.RelativePIDL = ILShell32.ILFindLastID(pidl); 

            IntPtr folderPidl = PIDL;

            SHFILEINFO? info = PidlManager.GetShFileInfo(pidl);
            var dir = System.IO.Path.GetDirectoryName(path);

            if ((info.Value.Attributes & SFGAO.SFGAO_FOLDER) == 0)
            { 
                // Item folder
                var handle = PidlManager.FromPath(dir);
                folderPidl = handle.pidl;
                SHFILEINFO? info2 = PidlManager.GetShFileInfo(folderPidl);
                if (!info2.HasValue)
                    return; // new DirectoryNotFoundException

                ParentItem = new ShellItem();
                ParentItem.PIDL = folderPidl;
                ParentItem.RelativePIDL = ILShell32.ILFindLastID(folderPidl); 

                ParentItem.DisplayName = info2.Value.szDisplayName;
                ParentItem.Attributes = (SFGAO)info2.Value.Attributes;
                ParentItem.IsFolder = (Attributes & SFGAO.SFGAO_FOLDER) != 0;
                ParentItem.IsFileSystem = (Attributes & SFGAO.SFGAO_FILESYSTEM) != 0;
            }

            //IShellItem i = ILShell32.SHCreateItemFromParsingName(path, IntPtr.Zero,
            //           typeof(IShellItem).GUID); 
            int hr = 0;
            IShellFolder pShellFolderInterface =
                IFolder(folderPidl, desktopShellFolder.Value.ShellFolderInterface, ref hr);

            if (pShellFolderInterface == null)
                throw new NullReferenceException("SheelObject ShellFolderInterface error");

            DisplayName = info.Value.szDisplayName;
            Attributes = (SFGAO)info.Value.Attributes;
            IsFolder = (Attributes & SFGAO.SFGAO_FOLDER) != 0;
            IsFileSystem = (Attributes & SFGAO.SFGAO_FILESYSTEM) != 0;

            if ((info.Value.Attributes & SFGAO.SFGAO_FOLDER) == 0)
                this.ParentItem.InitFolder(folderPidl, dir, pShellFolderInterface, false);
            else if (dir == null && path.EndsWith(@":\"))
                this.InitFolder(PIDL, path, pShellFolderInterface, false);
            else
                this.InitFolder(PIDL, dir, pShellFolderInterface, false);
        }

        /// <summary>
        /// Initialises the ShellItem, from its PIDL and parent.
        /// </summary>
        /// <param name="pidl">The pidl.</param>
        /// <param name="parentFolder">The parent folder.</param>
        internal void Initialise(IntPtr pidl, ShellItem parentFolder)
        {
            //  Set the parent item and relative pidl.
            ParentItem = parentFolder;
            RelativePIDL = pidl;

            //  Create the fully qualified PIDL.
            PIDL = Shell32.ILCombine(parentFolder.PIDL, pidl);

            //  Use the desktop folder to get attributes.
            var flags = SFGAO.SFGAO_FOLDER | SFGAO.SFGAO_HASSUBFOLDER | SFGAO.SFGAO_BROWSABLE | SFGAO.SFGAO_FILESYSTEM;
            //todo was this parentFolder.ShellFolderInterface.GetAttributesOf(1, ref pidl, ref flags);

            var apidl = Marshal.AllocCoTaskMem(IntPtr.Size * 1);
            Marshal.Copy(new IntPtr[] { pidl }, 0, apidl, 1);

            parentFolder.ShellFolderInterface.GetAttributesOf(1, apidl, ref flags);

            IsFolder = (flags & SFGAO.SFGAO_FOLDER) != 0;
            IsFileSystem = (flags & SFGAO.SFGAO_FILESYSTEM) != 0;
            HasSubFolders = (flags & SFGAO.SFGAO_HASSUBFOLDER) != 0;

            //  Get the file info.
            var fileInfo = new SHFILEINFO();
            Shell32.SHGetFileInfo(PIDL, 0, out fileInfo, (uint)Marshal.SizeOf(fileInfo),
                SHGFI.SHGFI_SMALLICON | SHGFI.SHGFI_SYSICONINDEX | SHGFI.SHGFI_PIDL
                | SHGFI.SHGFI_DISPLAYNAME | SHGFI.SHGFI_TYPENAME
                | SHGFI.SHGFI_ATTRIBUTES);

            //  Set extended attributes.
            DisplayName = fileInfo.szDisplayName;
            Attributes = (SFGAO)fileInfo.dwAttributes;
            TypeName = fileInfo.szTypeName;
            IconIndex = fileInfo.iIcon;

            //  Are we a folder?
            if (IsFolder)
            {
                InitFolder(pidl, this.Path, parentFolder.ShellFolderInterface);
            }
        }

        IShellFolder IFolder(IntPtr pidl, IShellFolder pShellFolderInterface, ref int result)
        {
            if (Shell32.ILIsEqual(pidl, Desktop.PIDL))
            {
                result = WinError.S_OK;
                return (IShellFolder)Desktop.ShellFolderInterface;
            }

            IntPtr ppv = IntPtr.Zero;

            result = pShellFolderInterface.BindToObject(pidl, IntPtr.Zero,
                ref Shell32.IID_IShellFolder,
                out ppv);

            if (ppv == IntPtr.Zero)
            {

                // SBSP_ABSOLUTE - pidl is an absolute pidl (relative from desktop)
                var pidlTmp = Shell32.ILClone(pidl);
                IntPtr folderTmpPtr = IntPtr.Zero;
                IShellFolder desktopFolder = NativePidl.DesktopFolder;
                var hr = desktopFolder.BindToObject(pidlTmp, IntPtr.Zero,
                        ref Shell32.IID_IShellFolder,
                        out folderTmpPtr);
                if (hr != WinError.S_OK)
                    return null;

                var folderTmp = Marshal.GetObjectForIUnknown(folderTmpPtr) as IShellFolder;

                if (folderTmp == null)
                {
                    Shell32.ILFree(pidlTmp);
                    return null; ; // WinError.E_FAIL
                }

                return folderTmp;
            }

            var shellFolderInterface = Marshal.GetObjectForIUnknown(ppv) as IShellFolder;
            return shellFolderInterface;
        }

        void InitFolder(IntPtr pidl, string dirPath, IShellFolder pShellFolderInterface, 
            bool isThrow = true)
        {
            //  Bind the shell folder interface.
            int result = 0;
            IShellFolder shellFolderInterface = IFolder(pidl, pShellFolderInterface, ref result);
            this.ShellFolderInterface = shellFolderInterface;

            // as ShellFolderInterface2
            this.ItemInterface = null;
            if (this.IsFileSystem)
            {
                // if (RunningVista)
                this.ItemInterface = ILShell32.SHCreateItemFromParsingName(dirPath, IntPtr.Zero,
                        typeof(IShellItem).GUID);
            }

            //  Validate the result.
            if (shellFolderInterface != null && result != 0)
            {
                //  Throw the failure as an exception.
                if (isThrow)
                    Marshal.ThrowExceptionForHR((int)result);
            }
        }

        /// <summary>
        /// Gets the system path for this shell item.
        /// </summary>
        private string CreatePath()
        {
            var stringBuilder = new StringBuilder(256);
            Shell32.SHGetPathFromIDList(PIDL, stringBuilder);
            return stringBuilder.ToString();
        }

        /// <summary>
        /// Gets the children.
        /// </summary>
        /// <param name="childTypes">The child types.</param>
        public IEnumerable<ShellItem> GetChildren(ChildTypes childTypes, bool lThrow = true)
        {
            return ShellObject.GetChildren(this, childTypes, lThrow);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            //  Release the shell folder interface.
            if (ShellFolderInterface != null)
            {
                Marshal.ReleaseComObject(ShellFolderInterface);
                ShellFolderInterface = null;
            }

            if (this.ItemInterface != null)
            {
                Marshal.ReleaseComObject(this.ItemInterface);
                ItemInterface = null;
            }

            //  Free the PIDL.
            if (PIDL != IntPtr.Zero)
                Marshal.FreeCoTaskMem(PIDL);

            //  Suppress finalization.
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        public override string ToString()
        {
            return string.IsNullOrEmpty(DisplayName) ? base.ToString() : DisplayName;
        }

        #region Properties

        /// <summary>
        /// The lazy desktop shell folder.
        /// </summary>
        private static readonly Lazy<ShellItem> desktopShellFolder;

        /// <summary>
        /// The lazy path.
        /// </summary>
        private readonly Lazy<string> path;

        /// <summary>
        /// The overlay icon.
        /// </summary>
        private readonly Lazy<Icon> overlayIcon;

        /// <summary>
        /// Gets the parent item.
        /// </summary>
        public ShellItem ParentItem { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance is folder.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is folder; otherwise, <c>false</c>.
        /// </value>
        public bool IsFolder { get; private set; }

        public bool IsFileSystem { get; private set; }

        /// <summary>
        /// Gets the display name.
        /// </summary>
        public string DisplayName { get; private set; }

        /// <summary>
        /// Gets the name of the type.
        /// </summary>
        /// <value>
        /// The name of the type.
        /// </value>
        public string TypeName { get; private set; }

        /// <summary>
        /// Gets the attributes.
        /// </summary>
        public SFGAO Attributes { get; private set; }

        /// <summary>
        /// Gets the index of the icon.
        /// </summary>
        /// <value>
        /// The index of the icon.
        /// </value>
        public int IconIndex { get; private set; }

        /// <summary>
        /// Gets the ShellFolder of the Desktop.
        /// </summary>
        public static ShellItem DesktopShellFolder { get { return desktopShellFolder.Value; } }

        /// <summary>
        /// Gets the shell folder interface.
        /// </summary>
        public IShellFolder ShellFolderInterface { get; private set; }

        public IShellItem ItemInterface { get; private set; }

        /// <summary>
        /// Gets the Full PIDL.
        /// </summary>
        public IntPtr PIDL { get; private set; }

        /// <summary>
        /// Gets the relative PIDL.
        /// </summary>
        public IntPtr RelativePIDL { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance has children.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance has children; otherwise, <c>false</c>.
        /// </value>
        public bool HasSubFolders { get; private set; }

        /// <summary>
        /// Gets the path.
        /// </summary>
        public string Path { get { return path.Value; } }

        /// <summary>
        /// Gets the overlay icon.
        /// </summary>
        /// <value>
        /// The overlay icon.
        /// </value>
        public Icon OverlayIcon { get { return overlayIcon.Value; } }

        #endregion

        // ShellContextMenu rename

        public static ShellItem Desktop { [DebuggerStepThrough] get { return desktopShellFolder.Value; } }
        public ShellItem Parent { [DebuggerStepThrough] get { return this.ParentItem; } }

        // Absolute
        public IntPtr Pidl { [DebuggerStepThrough] get { return this.PIDL; } }
        public IntPtr RelPidl { [DebuggerStepThrough] get { return this.RelativePIDL; } }

        public IShellView ComInterface { [DebuggerStepThrough] get; private set; }

    }


    /// <summary>
    /// ShellObject
    /// </summary>
    public static class ShellObject
    {

        //public static Guid IID_IContextMenu = new Guid("000214e4-0000-0000-c000-000000000046");
        //IntPtr SHGetFileInfo(IntPtr pszPath, uint dwFileAttribs, out SHFILEINFO psfi, uint cbFileInfo, SHGFI uFlags);

        /// <summary>
        /// Gets the shell item for a tree node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>The shell item for the tree node.</returns>
        public static ShellItem GetShellItem(TreeNode node, IDictionary<TreeNode, ShellItem> nodesToFolders)
        {
            ShellItem shellFolder;
            if (nodesToFolders.TryGetValue(node, out shellFolder))
                return shellFolder;
            return null;
        }

        public static int CreateViewObject(IShellFolder folder, IntPtr Handle, ref Guid shellViewGuid, out IntPtr iShellViewPtr)
        {
            int hr = WinError.S_FALSE;
            try
            {
                hr = folder.CreateViewObject(Handle,
                     ref shellViewGuid, out iShellViewPtr);
            }
            catch (System.AccessViolationException)
            {
                // A first chance exception of type 'System.AccessViolationException' occurred
                iShellViewPtr = IntPtr.Zero;
            }
            catch (Exception)
            {
                iShellViewPtr = IntPtr.Zero;
            }
            return hr; // WinError
        }

        //public static int CreateViewObject(IShellFolder2 folder, IntPtr Handle, ref Guid shellViewGuid, out IntPtr iShellViewPtr)
        //{
        //    folder.CreateViewObject 
        //}

        public static IEnumerable<ShellItem> GetChildren(this ShellItem parent, ChildTypes childTypes, bool lThrow = true)
        {
            //  We'll return a list of children.
            var children = new List<ShellItem>();

            //  Create the enum flags from the childtypes.
            SHCONTF enumFlags = 0;
            if (childTypes.HasFlag(ChildTypes.Folders))
                enumFlags |= SHCONTF.SHCONTF_FOLDERS;
            if (childTypes.HasFlag(ChildTypes.Files))
            {
                enumFlags |= SHCONTF.SHCONTF_NONFOLDERS;

                //enumFlags |= SHCONTF.SHCONTF_NAVIGATION_ENUM
                //    | SHCONTF.SHCONTF_FASTITEMS // The calling application is looking for resources that can be enumerated quickly.
                //    | SHCONTF.SHCONTF_FLATLIST; // Enumerate items as a simple list even if the folder itself is not structured in that way
            }
            if (childTypes.HasFlag(ChildTypes.Hidden))
                enumFlags |= SHCONTF.SHCONTF_INCLUDEHIDDEN;
            //| SHCONTF.SHCONTF_INCLUDESUPERHIDDEN;

            try
            {
                //  Create an enumerator for the children.
                IEnumIDList pEnum;
                var result = parent.ShellFolderInterface.EnumObjects(IntPtr.Zero, enumFlags, out pEnum);

                //  Validate the result.
                if (result != 0)
                {
                    if (!lThrow)
                    {
                        return Enumerable.Empty<ShellItem>();
                    }

                    //  Throw the failure as an exception.
                    Marshal.ThrowExceptionForHR((int)result);
                }

                // TODO: This logic should go in the pidl manager.

                //  Enumerate the children, ten at a time.
                const int batchSize = 10;
                var pidlArray = Marshal.AllocCoTaskMem(IntPtr.Size * 10);
                uint itemsFetched;
                result = WinError.S_OK;
                do
                {
                    result = pEnum.Next(batchSize, pidlArray, out itemsFetched);

                    //  Get each pidl.
                    var pidls = new IntPtr[itemsFetched];
                    Marshal.Copy(pidlArray, pidls, 0, (int)itemsFetched);
                    foreach (var childPidl in pidls)
                    {
                        //  Create a new shell folder.
                        var childShellFolder = new ShellItem();

                        //  Initialize it.
                        try
                        {
                            childShellFolder.Initialise(childPidl, parent);
                        }
                        catch (Exception exception)
                        {
                            throw new InvalidOperationException("Failed to initialise child.", exception);
                        }

                        //  Add the child.
                        children.Add(childShellFolder);

                        //  Free the PIDL, reset the result.
                        Marshal.FreeCoTaskMem(childPidl);
                    }
                } while (result == WinError.S_OK);

                Marshal.FreeCoTaskMem(pidlArray);

                //  Release the enumerator.
                if (Marshal.IsComObject(pEnum))
                    Marshal.ReleaseComObject(pEnum);
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException("Failed to enumerate children.", exception);
            }

            //  Sort the children.
            var sortedChildren = children.Where(c => c.IsFolder).ToList();
            sortedChildren.AddRange(children.Where(c => !c.IsFolder));

            //  Return the children.
            return sortedChildren;
        }

        public static IShellFolder DesktopFolder
        {
            get
            {
                IShellFolder desktop = null;
                Shell32.SHGetDesktopFolder(out desktop);
                return desktop;
            }
        }

        public static IShellFolder GetIShellFolder(this ShellItem item)
        {
            var _pidl = item.PIDL; // Absolute PIDL
            IShellFolder desktop = DesktopFolder;

            IntPtr desktopPidl = IntPtr.Zero;
            Shell32.SHGetSpecialFolderLocation(IntPtr.Zero, CSIDL.CSIDL_DESKTOP,
                ref desktopPidl); ;

            if (Shell32.ILIsEqual(_pidl, desktopPidl))
            {
                return (IShellFolder)desktop;
            }
            else
            {
                IntPtr result;
                desktop.BindToObject(_pidl, IntPtr.Zero,
                    ref Shell32.IID_IShellFolder, out result);

                var folder = (IShellFolder)Marshal.GetTypedObjectForIUnknown(result,
                    typeof(IShellFolder));
                return folder;
            }
        }

    }

    /// <summary>
    /// The Child Type flags.
    /// </summary>
    [Flags]
    public enum ChildTypes
    {
        Folders = 1,
        Files = 2,
        Hidden = 4
    }
}
