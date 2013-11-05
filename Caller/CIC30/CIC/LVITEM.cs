using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace CIC
{
    [StructLayoutAttribute(LayoutKind.Sequential)]
    internal struct LV_ITEM
    {
        public UInt32 mask;
        public Int32 iItem;
        public Int32 iSubItem;
        public UInt32 state;
        public UInt32 stateMask;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string pszText;
        public Int32 cchTextMax;
        public Int32 iImage;
        public IntPtr lParam;
    }

    public delegate int ListViewSortDelegate(IntPtr p1, IntPtr p2, IntPtr p3);

    public class ListViewSubitemIcon : System.Windows.Forms.ListView
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        private ListViewSortDelegate _ListViewSortDelegate;

        /// Changing the style of listview to accept image on subitems
        /// </summary>
        public ListViewSubitemIcon()
        {
            this.InitializeComponent();
            this.HandleCreated += new EventHandler(ListViewSubitemIcon_HandleCreated);

            this.DoubleBuffered = true;

            this._ListViewSortDelegate = new ListViewSortDelegate(this.SortItems);
        }

        void ListViewSubitemIcon_HandleCreated(object sender, EventArgs e)
        {
            // Change the style of listview to accept image on subitems
            System.Windows.Forms.Message m = new Message();
            System.Version ver = System.Environment.OSVersion.Version;

            m.HWnd = this.Handle;
            m.Msg = Native.LVM_GETEXTENDEDLISTVIEWSTYLE;

            m.LParam = (IntPtr)(Native.LVS_EX_GRIDLINES |
                                Native.LVS_EX_FULLROWSELECT |
                                Native.LVS_EX_SUBITEMIMAGES |
                                Native.LVS_EX_CHECKBOXES |
                                Native.LVS_EX_TRACKSELECT);

            m.WParam = IntPtr.Zero;

            this.WndProc(ref m);

        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case Native.WM_VSCROLL:
                case Native.WM_HSCROLL:
                    this.Focus();
                    break;
                default:
                    break;
            }
            base.WndProc(ref m);
        }



        #region Component Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
        }
        #endregion
        public void SortAscending()
        {
            Native.SendMessage(this.Handle, Native.LVM_SORTITEMSEX, System.IntPtr.Zero, Marshal.GetFunctionPointerForDelegate(this._ListViewSortDelegate));

        }
        private int SortItems(IntPtr p1, IntPtr p2, IntPtr p3)
        {
            return (string.Compare(this.Items[p1.ToInt32()].Text, this.Items[p2.ToInt32()].Text, StringComparison.CurrentCultureIgnoreCase));
        }

    }
}
