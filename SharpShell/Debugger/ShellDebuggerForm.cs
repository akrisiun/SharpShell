using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ShellDebugger
{
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.None)]
    public partial class ShellDebuggerForm : Form
    {
        ShellDebuggerModel Model;

        #region ctor, destroyed

        public ShellDebuggerForm()
        {
            InitializeComponent();
            Model = new ShellDebuggerModel(this);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            Model.Bind();
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            Model.OnHandleDestroyed();
            base.OnHandleDestroyed(e);
        }

        //protected override void WndProc(ref Message m)
        //{
        //    var contextMenu = this.shellTreeView.ShellContextMenu;
        //    if ((contextMenu == null)
        //        || (!contextMenu.HandleMenuMessage(ref m)))
        //    {
        //        base.WndProc(ref m);
        //    }
        //}

        #endregion

    }
}
