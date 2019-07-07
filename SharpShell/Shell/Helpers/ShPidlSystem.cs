using SharpShell.Helpers;
using SharpShell.Interop;
using SharpShell.SharpContextMenu;
using System;
using System.IO;

namespace SharpShell.Pidl
{
    public static class ShPidlSystem
    {
        public static PidlData FromPath(string dir)
        {
            return NativePidl.PIDListFromPath(dir);
        }

        public static ShellItem ItemFromPath(string dir)
        {
            PidlData dataPidl = FromPath(dir);
            var itemHit = new ShellItem(dataPidl.Handle.pidl, dataPidl.Path);
            return itemHit;
        }

        public static System.Windows.Forms.Control GetControlWindow()
        {
            var ctrl = new ControlWindow();
            return ctrl;
        }

        public static IShellFolder ShellFolderInterface(this ShellItem itemHit)
        {
            return itemHit.IsFolder ? itemHit.ShellFolderInterface
                   : itemHit.ParentItem.ShellFolderInterface;
        }

        public static bool ShowMenu(ShellContextMenu menu, IShellFolder shellFolder, ShellItem itemHit,
               System.Windows.Forms.Control ctrl, System.Drawing.Point pt)
        {
            if (ctrl is ControlWindow)
                (ctrl as ControlWindow).ParentMenu = menu;

            var PIDL = itemHit.PIDL;

            if (itemHit.IsFolder)
            {
                IShellItem parentInterface = itemHit.ParentItem != null ? itemHit.ParentItem.ItemInterface : null;
                if (parentInterface == null)
                {
                    var parentPath = Path.GetFullPath(itemHit.Path + @"..\");
                    parentInterface = ILShell32.SHCreateItemFromParsingName(parentPath , IntPtr.Zero, typeof(IShellItem).GUID);
                }

                if (menu.ComInterface == null)
                {
                    var aPidl = PidlManager.PidlToIdlist(itemHit.PIDL);

                    if (parentInterface != null && !menu.InitFolderPath(shellFolder, parentInterface, itemHit.PIDL, itemHit.Path))
                        return false;
                }
            }

            if (menu.ComInterface == null && !menu.InitFolder(shellFolder, PIDL))
                return false;

            menu.ShowContextMenu(ctrl, pt);
            return true;
        }

        public static IntPtr ParseDesktopContext(ShellItem itemHit, IShellFolder shellFolder, System.Windows.Forms.Control ctrl)
        {
            IntPtr ppv = IntPtr.Zero;

            //  The item pidl is either the folder if the item is a folder, or the combined pidl otherwise.
            if (ILShell32.ILIsEqual(itemHit.PIDL, ShellItem.Desktop.PIDL))
            {
                var fullIdList = itemHit.IsFolder
                    ? PidlManager.PidlToIdlist(itemHit.PIDL)
                    : PidlManager.Combine(PidlManager.PidlToIdlist(itemHit.ParentItem.PIDL),
                        PidlManager.PidlToIdlist(itemHit.RelativePIDL));

                //  Get the UI object of the context menu.
                // IntPtr itemPidl = PidlManager.IdListToPidl(fullIdList);
                IntPtr itemPidl = itemHit.RelativePIDL;
                IntPtr[] apidl = new IntPtr[] { itemPidl };
                // PidlManager.PidlsToAPidl(new IntPtr[] { itemPidl })

                shellFolder.GetUIObjectOf(ctrl.Handle, (uint)1,
                    apidl,
                    ref Shell32.IID_IContextMenu, 0,
                    out ppv);

            }

            return ppv;
        }


        internal class ControlWindow : System.Windows.Forms.Control
        {
            public ControlWindow(ShellContextMenu parent = null)
            {
                ParentMenu = parent;
            }

            protected override void WndProc(ref System.Windows.Forms.Message m)
            {
                if (ParentMenu != null
                    && ParentMenu.HandleMenuMessage(ref m))
                    return;

                base.WndProc(ref m);
            }

            public ShellContextMenu ParentMenu;
        }

    }
}
