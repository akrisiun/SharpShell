using SharpShell;
using SharpShell.Interop;
using SharpShell.SharpContextMenu;
using System;
using System.Runtime.InteropServices;
using System.Text;

//namespace GongSolutions.Shell.Interop.VistaBridge
namespace Shell.SharpContextMenu
{
    class ShellItemImpl : IDisposable, IShellItem
    {
        public ShellItemImpl(IntPtr pidl, bool owner)
        {
            if (owner)
            {
                _pidl = pidl;
            }
            else
            {
                _pidl = Shell32.ILClone(pidl);
            }
        }

        ~ShellItemImpl()
        {
            Dispose(false);
        }

        public IntPtr BindToHandler(IntPtr pbc, Guid bhid, Guid riid)
        {
            if (riid == typeof(IShellFolder).GUID)
            {
                return Marshal.GetIUnknownForObject(GetIShellFolder());
            }
            else
            {
                throw new InvalidCastException();
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public HResult GetParent(out IShellItem ppsi)
        {
            IntPtr pidl = Shell32.ILClone(_pidl);
            if (ILShell32.ILRemoveLastID(pidl))
            {
                ppsi = new ShellItemImpl(pidl, true);
                return HResult.S_OK;
            }
            else
            {
                ppsi = null;
                return HResult.MK_E_NOOBJECT;
            }
        }

        public IntPtr GetDisplayName(SIGDN sigdnName)
        {
            if (sigdnName == SIGDN.FILESYSPATH)
            {
                StringBuilder result = new StringBuilder(FileSystem.MAX_PATH);
                if (!ILShell32.SHGetPathFromIDList(_pidl, result))
                    throw new ArgumentException();
                return Marshal.StringToHGlobalUni(result.ToString());
            }
            else
            {
                //IShellFolder parentFolder = GetParent().GetIShellFolder();
                IntPtr childPidl = ILShell32.ILFindLastID(_pidl);
                StringBuilder builder = new StringBuilder(FileSystem.MAX_PATH);
                builder.Length = 0; // TODO
                //STRRET strret = new STRRET();

                //TODO
                //parentFolder.GetDisplayNameOf(childPidl,
                //    (SHGNO)((int)sigdnName & 0xffff), out strret);
                //ShlWapi.StrRetToBuf(ref strret, childPidl, builder,
                //    (uint)builder.Capacity);
                return Marshal.StringToHGlobalUni(builder.ToString());
            }
        }

        public SFGAO GetAttributes(SFGAO sfgaoMask)
        {
            //IShellFolder parentFolder = GetParent().GetIShellFolder();
            SFGAO result = sfgaoMask;

            //parentFolder.GetAttributesOf(1,
            //    new IntPtr[] { ILShell32.ILFindLastID(_pidl) },
            //    ref result);
            return result & sfgaoMask;
        }

        public int Compare(IShellItem psi, SICHINT hint)
        {
            ShellItemImpl other = (ShellItemImpl)psi;
            ShellItemImpl myParent = GetParent();
            ShellItemImpl theirParent = other.GetParent();

            if (Shell32.ILIsEqual(myParent._pidl, theirParent._pidl))
            {
                return myParent.GetIShellFolder().CompareIDs(
                    // (SHCIDS)
                    (IntPtr)hint,
                    ILShell32.ILFindLastID(_pidl),
                    ILShell32.ILFindLastID(other._pidl));
            }
            else
            {
                return 1;
            }
        }

        public IntPtr Pidl
        {
            get { return _pidl; }
        }

        protected void Dispose(bool dispose)
        {
            Shell32.ILFree(_pidl);
        }

        ShellItemImpl GetParent()
        {
            IntPtr pidl = Shell32.ILClone(_pidl);

            if (ILShell32.ILRemoveLastID(pidl))
            {
                return new ShellItemImpl(pidl, true);
            }
            else
            {
                return this;
            }
        }

        IShellFolder GetIShellFolder()
        {
            IShellFolder desktop = null;
            Shell32.SHGetDesktopFolder(out desktop);

            IntPtr desktopPidl = IntPtr.Zero;

            Shell32.SHGetSpecialFolderLocation(IntPtr.Zero, CSIDL.CSIDL_DESKTOP,
                ref desktopPidl); ;

            if (Shell32.ILIsEqual(_pidl, desktopPidl))
            {
                return desktop;
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

        IntPtr _pidl;
    }
}
