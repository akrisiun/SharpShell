using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Diagnostics;
using SharpShell.Interop;
using SharpShell.Pidl;
using SharpShell.Helpers;
//using SharpShell.SharpNamespaceExtension;

namespace ShellDebugger
{
    public partial class ShellTreeView
    {

        public void Bind()
        {
            this.AfterSelect += ShellTreeView_AfterSelect;
        }

        void ShellTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            DoSelect(e.Node);
        }

        public void DoSelect(TreeNode node)
        {
            var shellItem = ShellObject.GetShellItem(node, this.nodesToFolders);
            FireOnShellItemSelected(shellItem);
        }

        /// <summary>
        /// Raises the <see cref="M:System.Windows.Forms.Control.CreateControl"/> method.
        /// </summary>
        protected override void OnCreateControl()
        {
            //  Call the base.
            base.OnCreateControl();

            //  Add the desktop node, if we're not in design mode.
            if (!DesignMode)
                ShellTreeMethods.AddDesktopNode(this);
        }

     
        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.TreeView.BeforeExpand"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.TreeViewCancelEventArgs"/> that contains the event data.</param>
        protected override void OnBeforeExpand(TreeViewCancelEventArgs e)
        {
            if (e.Node != this.Nodes[0])
                ShellTreeMethods.DoExpand(this, e.Node);

            //  Call the base.
            base.OnBeforeExpand(e);
        }

        /// <summary>
        /// Fires the on shell item added event.
        /// </summary>
        /// <param name="nodeAdded">The node added.</param>
        internal void FireOnShellItemAdded(TreeNode nodeAdded)
        {
            //  Fire the event if we have it.
            var theEvent = OnShellItemAdded;
            if (theEvent != null)
                theEvent(this, new TreeViewEventArgs(nodeAdded));
        }

        
        private void FireOnShellItemSelected(ShellItem shellItem)
        {
            var theEvent = OnShellItemSelected;
            if (theEvent != null)
                theEvent(this, new ShellItemEventArgs(shellItem));
        }

        #region Properties

        /// <summary>
        /// A map of tree nodes to the Shell Folders.
        /// </summary>
        private readonly IDictionary<TreeNode, ShellItem> nodesToFolders = new Dictionary<TreeNode, ShellItem>();
        public IDictionary<TreeNode, ShellItem> NodesToFolders { [DebuggerStepThrough] get { return nodesToFolders; } }

        /// <summary>
        /// Gets or sets a value indicating whether to show hidden files and folders.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if show hidden files and folders; otherwise, <c>false</c>.
        /// </value>
        [Category("Shell Tree View")]
        [Description("If set to true, hidden files and folders will be shown.")]
        public bool ShowHiddenFilesAndFolders { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show files.
        /// </summary>
        /// <value>
        ///   <c>true</c> if show files; otherwise, <c>false</c>.
        /// </value>
        [Category("Shell Tree View")]
        [Description("If set to true, files will be shown as well as folders.")]
        public bool ShowFiles { get; set; }

        /// <summary>
        /// Gets a value that indicates whether the <see cref="T:System.ComponentModel.Component"/> is currently in design mode.
        /// </summary>
        /// <returns>true if the <see cref="T:System.ComponentModel.Component"/> is in design mode; otherwise, false.</returns>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new bool DesignMode { get { return (System.Diagnostics.Process.GetCurrentProcess().ProcessName == "devenv"); } }

        /// <summary>
        /// Occurs when a shell item is added.
        /// </summary>
        [Category("Shell Tree View")]
        [Description("Called when a shell item is added.")]
        public event TreeViewEventHandler OnShellItemAdded;

        public event ShellItemTreeEventHandler OnShellItemSelected;

        #endregion

        //private void InitializeComponent()
        //{
        //    this.SuspendLayout();
        //    // 
        //    // ShellTreeView
        //    // 
        //    this.ResumeLayout(false);

        //}

        protected override void OnNodeMouseClick(TreeNodeMouseClickEventArgs e)
        {
            base.OnNodeMouseClick(e);

            if (e.Button == MouseButtons.Right)
            {
                //  Get the item hit.
                var itemHit = ShellObject.GetShellItem(e.Node, this.nodesToFolders);

                //  Create a default context menu.
                ShellTreeMethods.OpenItemContextMenu(e.Node, itemHit, e.X, e.Y);
            }
        }

        public ShellItem SelectedShellItem
        {
            get
            {
                return this.SelectedNode == null || !nodesToFolders.ContainsKey(SelectedNode) ? null :
                    nodesToFolders[this.SelectedNode];
            }
        }

    }

}
