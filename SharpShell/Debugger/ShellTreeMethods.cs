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
using System.Collections.Generic;
using System.Drawing;
using SharpShell.SharpContextMenu;

namespace ShellDebugger
{
    public static class ShellTreeMethods
    {
        /// <summary>
        /// Adds the desktop node.
        /// </summary>
        public static void AddDesktopNode(this ShellTreeView tree)
        {
            //  Get the desktop folder.
            var desktopFolder = ShellItem.DesktopShellFolder;

            //  Create the desktop node.
            var desktopNode = new TreeNode
            {
                Text = desktopFolder.DisplayName,
                ImageIndex = desktopFolder.IconIndex,
                SelectedImageIndex = desktopFolder.IconIndex,
            };

            //  Map it and add it.
            tree.NodesToFolders[desktopNode] = desktopFolder;
            tree.ShowRootLines = false;
            tree.Nodes.Add(desktopNode);

            //  Fire the event.
            tree.FireOnShellItemAdded(desktopNode);

            //  Expand it.
            //OnBeforeExpand(new TreeViewCancelEventArgs(desktopNode, false, TreeViewAction.Expand));
            DoExpand(tree, desktopNode);
            desktopNode.Expand();
        }

        public static void DoExpand(this ShellTreeView tree, TreeNode node)
        {
            //  Clear children - which may in fact be the placeholder.
            node.Nodes.Clear();
            var nodesToFolders = tree.NodesToFolders;

            //  Get the shell folder.
            ShellItem shellFolder = nodesToFolders[node];

            //  Create the enum flags.
            ChildTypes childFlags = ChildTypes.Folders;
            if (tree.ShowFiles)
                childFlags |= ChildTypes.Files;
            if (tree.ShowHiddenFilesAndFolders)
                childFlags |= ChildTypes.Hidden;

            //  Disable update while adding children.
            tree.BeginUpdate();

            int count = 0;
            try
            {
                //  Go through each child.
                foreach (var childNode in
                    EnumerateUnsafe(shellFolder, childFlags, node, nodesToFolders))
                {
                    count++;
                    //  Fire the shell item added event.
                    tree.FireOnShellItemAdded(childNode);
                }

                if (count == 0)
                {
                    childFlags = ChildTypes.Files | ChildTypes.Hidden;

                    foreach (var childNode in
                        EnumerateUnsafe(shellFolder, childFlags, node, nodesToFolders))
                    {
                        count++;
                        //  Fire the shell item added event.
                        tree.FireOnShellItemAdded(childNode);
                    }
                    if (count > 0)
                    {
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                    ex = ex.InnerException;

                MessageBox.Show(ex.Message);
            }

            //  Enable update now that we've added the children.
            tree.EndUpdate();
        }


        static IEnumerable<TreeNode> EnumerateUnsafe(ShellItem shellFolder, ChildTypes childFlags,
            TreeNode node, IDictionary<TreeNode, ShellItem> nodesToFolders)
        {
            var numer = shellFolder.GetChildren(childFlags, false).GetEnumerator();
            while (numer.MoveNext())
            {
                ShellItem child = numer.Current;

                //  Create a child node.
                var childNode = new TreeNode
                {
                    Text = child.DisplayName,
                    ImageIndex = child.IconIndex,
                    SelectedImageIndex = child.IconIndex,
                };

                //  Map the node to the shell folder.
                nodesToFolders[childNode] = child;

                //  If this item has children, add a child node as a placeholder.
                if (child.HasSubFolders)
                    childNode.Nodes.Add(string.Empty);

                //  Add the child node.
                node.Nodes.Add(childNode);

                yield return childNode;
            }
        }

        public static void OpenItemContextMenu(TreeNode node, ShellItem itemHit, int x, int y)
        {
            var tree = node.TreeView;

            if (!itemHit.IsFileSystem)
            {
                OpenSystemMenu(itemHit, tree, x, y);
                return;
            }


            // Not a desktop item
            var menu = new ShellContextMenu(itemHit);

            var pos = new Point(node.Bounds.Left, node.Bounds.Top);
            ShPidlSystem.ShowMenu(menu, itemHit.ShellFolderInterface, itemHit, tree, pos);

            //menu.ShowContextMenu(tree, pos);

            return;
        }

        public static void OpenSystemMenu(ShellItem itemHit, Control ctrl, int x, int y)
        {
            //  The shell folder we use to get the UI object is either the folder itself if the
            //  item is a folder, or the parent folder otherwise.
            var shellFolder = itemHit.IsFolder ? itemHit.ShellFolderInterface : itemHit.ParentItem.ShellFolderInterface;

            //  The item pidl is either the folder if the item is a folder, or the combined pidl otherwise.
            var fullIdList = itemHit.IsFolder
                ? PidlManager.PidlToIdlist(itemHit.PIDL)
                : PidlManager.Combine(
                    PidlManager.PidlToIdlist(itemHit.ParentItem.PIDL),
                    PidlManager.PidlToIdlist(itemHit.RelativePIDL));


            //  Get the UI object of the context menu.
            IntPtr[] apidl = new IntPtr[] { PidlManager.IdListToPidl(fullIdList) };
            // PidlManager.PidlsToAPidl(new IntPtr[] { PidlManager.IdListToPidl(fullIdList) });

            IntPtr ppv = IntPtr.Zero;
            shellFolder.GetUIObjectOf(ctrl.Handle, 1,
                    apidl,
                    Shell32.IID_IContextMenu, 0,
                    out ppv);

            //  If we have an item, cast it.
            if (ppv != IntPtr.Zero)
            {
                // desktop menu
                var contextMenu = Marshal.GetObjectForIUnknown(ppv) as IContextMenu; // IContextMenuSharp

                var popupMenu = new ContextMenu();
                contextMenu.QueryContextMenu(popupMenu.Handle, 0, 0, 65525, CMF.CMF_EXPLORE);
                popupMenu.Show(ctrl, new Point(x, y));
                return;
            }
        }

    }

    public delegate void ShellItemTreeEventHandler(object sender, ShellItemEventArgs e);

    public static class TreeViewExtensions
    {
        /// <summary>
        /// TreeView ImageList type.
        /// </summary>
        public enum ImageListType
        {
            /// <summary>
            /// Normal images.
            /// </summary>
            Normal = 0,

            /// <summary>
            /// State images.
            /// </summary>
            State = 2
        }

        /// <summary>
        /// Normal tree view image.
        /// </summary>
        private const uint TVSIL_NORMAL = 0;

        /// <summary>
        /// The state images.
        /// </summary>
        private const uint TVSIL_STATE = 2;

        /// <summary>
        /// First tree view message.
        /// </summary>
        private const uint TV_FIRST = 0x1100;

        /// <summary>
        /// Get image list message.
        /// </summary>
        private const uint TVM_GETIMAGELIST = TV_FIRST + 8;

        /// <summary>
        /// Set image list message.
        /// </summary>
        private const uint TVM_SETIMAGELIST = TV_FIRST + 9;

        /// <summary>
        /// Sets the image list.
        /// </summary>
        /// <param name="this">The tree view instance.</param>
        /// <param name="imageListType">Type of the image list.</param>
        /// <param name="imageListHandle">The image list handle.</param>
        public static void SetImageList(this TreeView @this, ImageListType imageListType, IntPtr imageListHandle)
        {
            //  Set the image list.
            User32.SendMessage(@this.Handle, TVM_SETIMAGELIST, (uint)imageListType, imageListHandle);
        }

        /// <summary>
        /// Gets the image list.
        /// </summary>
        /// <param name="this">The tree view instance.</param>
        /// <param name="imageListType">Type of the image list.</param>
        /// <returns>The image list handle.</returns>
        public static IntPtr GetImageList(this TreeView @this, ImageListType imageListType)
        {
            //  Set the image list.
            return new IntPtr(User32.SendMessage(@this.Handle, TVM_GETIMAGELIST, (uint)imageListType, IntPtr.Zero));
        }
    }

    /// <summary>
    /// The Shell Image List.
    /// </summary>
    public static class ShellImageList
    {
        /// <summary>
        /// Initializes the <see cref="ShellImageList"/> class.
        /// </summary>
        static ShellImageList()
        {
        }

        /// <summary>
        /// Gets the image list interface.
        /// </summary>
        /// <param name="imageListSize">Size of the image list.</param>
        /// <returns>The IImageList for the shell image list of the given size.</returns>
        public static IntPtr GetImageList(ShellImageListSize imageListSize)
        {
            //  Do we have the image list?
            IImageList imageList;
            if (imageLists.TryGetValue(imageListSize, out imageList))
                return GetImageListHandle(imageList);

            //  We don't have the image list, create it.
            Shell32.SHGetImageList((int)imageListSize, ref Shell32.IID_IImageList, ref imageList);

            //  Add it to the dictionary.
            imageLists.Add(imageListSize, imageList);

            //  Return it.
            return GetImageListHandle(imageList);
        }

        /// <summary>
        /// Gets the image list handle.
        /// </summary>
        /// <param name="imageList">The image list.</param>
        /// <returns>The image list handle for the image list.</returns>
        private static IntPtr GetImageListHandle(IImageList imageList)
        {
            return Marshal.GetIUnknownForObject(imageList);
        }

        /// <summary>
        /// The shell image lists.
        /// </summary>
        private readonly static Dictionary<ShellImageListSize, IImageList> imageLists = new Dictionary<ShellImageListSize, IImageList>();

        /// <summary>
        /// Shell Image List sizes. These correspond exactly by value to the sizes such 
        /// as SHIL_LARGE, SHIL_JUMBO, etc.
        /// </summary>
        public enum ShellImageListSize
        {
            /// <summary>
            /// The image size is normally 32x32 pixels. However, if the Use large icons option is selected from the Effects section of the Appearance tab in Display Properties, the image is 48x48 pixels.
            /// </summary>
            Large = 0x0,

            /// <summary>
            /// These images are the Shell standard small icon size of 16x16, but the size can be customized by the user.
            /// </summary>
            Small = 0x1,

            /// <summary>
            /// These images are the Shell standard extra-large icon size. This is typically 48x48, but the size can be customized by the user.
            /// </summary>
            ExtraLarge = 0x2,

            /// <summary>
            /// These images are the size specified by GetSystemMetrics called with SM_CXSMICON and GetSystemMetrics called with SM_CYSMICON.
            /// </summary>
            SysSmall = 0x3,

            /// <summary>
            /// Windows Vista and later. The image is normally 256x256 pixels.
            /// </summary>
            Jumbo = 0x4
        }

    }


}
