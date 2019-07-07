using System;

namespace SharpShell.Helpers
{
    public class ShellItemEventArgs : EventArgs
    {
        public ShellItemEventArgs(ShellItem shellItem)
        {
            ShellItem = shellItem;
        }

        public ShellItem ShellItem { get; private set; }
    }
}