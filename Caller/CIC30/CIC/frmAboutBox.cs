using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace CIC
{
    partial class frmAboutBox : Form
    {
        public frmAboutBox()
        {
            InitializeComponent();
            this.Text = String.Format("About {0}", AssemblyTitle);
            this.labelProductName.Text = AssemblyProduct;
            this.labelVersion.Text = String.Format("Version {0} ", AssemblyVersion);
            this.labelCopyright.Text = AssemblyCopyright;
            this.labelCompanyName.Text = "";
            this.textBoxDescription.Text = AssemblyDescription;
        }

        #region Assembly Attribute Accessors

        public string AssemblyTitle
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
                if (attributes.Length > 0)
                {
                    AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
                    if (titleAttribute.Title != "")
                    {
                        return titleAttribute.Title;
                    }
                }
                return System.IO.Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
            }
        }

        public string AssemblyVersion
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

        public string AssemblyDescription
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyDescriptionAttribute)attributes[0]).Description;
            }
        }

        public string AssemblyProduct
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyProductAttribute)attributes[0]).Product;
            }
        }

        public string AssemblyCopyright
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
            }
        }

        public string AssemblyCompany
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyCompanyAttribute)attributes[0]).Company;
            }
        }
        #endregion

        private void Load_ApplicationSkin()
        {
            this.BackgroundImage = global::CIC.Program.AppImageList.Images["MainBackground"];
            this.BackgroundImageLayout = ImageLayout.Stretch;
        }

        private void DrawFrameAnimation(RECT rcFrom, RECT rcTo)
        {
            int nSteps = 15;
            IntPtr hdc;
            IntPtr hdcForm;
            IntPtr hdcCompatible;
            IntPtr hbmForm;
            Native.GdiFlush();
            System.Threading.Thread.Sleep(50);
            hdc = Native.GetDC(IntPtr.Zero);
            hdcForm = Native.GetDC(this.Handle);
            hdcCompatible = Native.CreateCompatibleDC(hdcForm);
            hbmForm = Native.CreateCompatibleBitmap(hdcForm, this.Width, this.Height);
            Native.SelectObject(hdcCompatible, hbmForm);
            Native.BitBlt(hdcCompatible, 0, 0, this.Width, this.Height, hdcForm, 0, 0, Native.SRCCOPY);
            for (int i = 0; i < nSteps; i++)
            {
                RECT curRect = new RECT();
                int xStart;
                int yStart;
                double fraction = (double)i / (double)nSteps;
                curRect.Left = rcFrom.Left + (int)((double)(rcTo.Left - rcFrom.Left) * fraction);
                curRect.Top = rcFrom.Top + (int)((double)(rcTo.Top - rcFrom.Top) * fraction);
                curRect.Right = rcFrom.Right + (int)((double)(rcTo.Right - rcFrom.Right) * fraction);
                curRect.Bottom = rcFrom.Bottom + (int)((double)(rcTo.Bottom - rcFrom.Bottom) * fraction);
                xStart = (this.Bounds.Right - this.Bounds.Left) - this.Width / 2 - ((curRect.Right - curRect.Left) / 2);
                yStart = (this.Bounds.Bottom - this.Bounds.Top) - this.Height / 2 - ((curRect.Bottom - curRect.Top) / 2);
                Native.BitBlt(hdc, curRect.Left, curRect.Top, (curRect.Right - curRect.Left), (curRect.Bottom - curRect.Top),hdcCompatible, 0, 0, Native.SRCCOPY);
                Native.GdiFlush();
                System.Threading.Thread.Sleep(20);
            }
            Native.ReleaseDC(this.Handle, hdcForm);
            Native.DeleteDC(hdcCompatible);
            Native.DeleteObject(hbmForm);
            Native.ReleaseDC(IntPtr.Zero, hdc);
        }

        private void frmAboutBox_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (global::CIC.Properties.Settings.Default.Animate == true)
            {

                bool b = Native.AnimateWindow(this.Handle, 1000, Native.AW_BLEND | Native.AW_HIDE);
                base.OnClosing(e);
            }
        }

        private void frmAboutBox_Load(object sender, EventArgs e)
        {
            if (global::CIC.Properties.Settings.Default.Animate == true)
            {
                RECT rcFrom = new RECT();
                RECT rcTo = new RECT();
                Rectangle screenBounds;
                screenBounds = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
                rcFrom.Left = screenBounds.Width / 2;
                rcFrom.Top = screenBounds.Height / 2;
                rcFrom.Right = rcFrom.Left;
                rcFrom.Bottom = rcFrom.Top;
                rcTo.Left = (screenBounds.Width / 2) - this.Width / 2;
                rcTo.Top = (screenBounds.Height / 2) - this.Height / 2;
                rcTo.Right = rcTo.Left + this.Width;
                rcTo.Bottom = rcTo.Top + this.Height;
                this.DrawFrameAnimation(rcFrom, rcTo);
            }
            this.Load_ApplicationSkin();
        }

    }
}
