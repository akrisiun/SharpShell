using SharpShell.Interop;
using System;
using System.Windows.Forms;

namespace ShellDebugger
{
    /// <summary>
    /// The ShellTreeView is a tree view that is designed to show contents of the system,
    /// just like in Windows Explorer.
    /// </summary>
    public partial class ShellTreeView : TreeView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ShellTreeView"/> class.
        /// </summary>
        public ShellTreeView()
        {
            //  TODO: Shell tree views should be double buffered.

            //  Set the image list to the shell image list.
            this.SetImageList(TreeViewExtensions.ImageListType.Normal,
                ShellImageList.GetImageList(ShellImageList.ShellImageListSize.Small)
                );
        }

        //public IContextMenu _ContextMenu;
        //public ShellContextMenu ShellContextMenu;

        //protected override void WndProc(ref Message m)
        //{
        //    if (m.Msg == (int)WindowsMessages.WM_COMMAND
        //        && ShellContextMenu != null)
        //    {
        //        if (ShellContextMenu.HandleMenuMessage(ref m))
        //            return;
        //    }

        //    base.WndProc(ref m);
        //}
    }
}