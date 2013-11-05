using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CIC
{
    public partial class frmChangePassword : Form
    {
        public frmChangePassword()
        {
            InitializeComponent();
            this.Load_ApplicationSkin();
        }

        private void Load_ApplicationSkin()
        {
            this.BackgroundImage = global::CIC.Program.AppImageList.Images["MainBackground"];
            this.BackgroundImageLayout = ImageLayout.Stretch;
            this.btnChangePassword.BackgroundImage = global::CIC.Program.AppImageList.Images["MainBackground"];
            this.btnChangePassword.BackgroundImageLayout = ImageLayout.Stretch;
            this.btnCancel.BackgroundImage = global::CIC.Program.AppImageList.Images["MainBackground"];
            this.btnCancel.BackgroundImageLayout = ImageLayout.Stretch;
        }

        private void frmChangePassword_Load(object sender, EventArgs e)
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

        private bool Verify_Password()
        {
            bool bRet = true;
            if (this.txtOldPassword.Text.Trim() != global::CIC.Program.mLoginParam.Password.Trim())
            {
                this.SetInfoMessage(0);
                bRet = false;
            }
            if (this.txtNewPassword.Text.Trim() != this.txtRetypeNewPassword.Text.Trim())
            {
                this.SetInfoMessage(1);
                bRet = false;
            }
            if (this.txtNewPassword.Text.Trim() == global::CIC.Program.mLoginParam.Password.Trim())
            {
                this.SetInfoMessage(2);
                bRet = false;
            }
            if (this.txtNewPassword.Text.Trim() == this.txtRetypeNewPassword.Text.Trim())
            {
                if ((this.txtNewPassword.Text.Trim() == "") && (this.txtRetypeNewPassword.Text.Trim() == ""))
                {
                    this.SetInfoMessage(-1);
                    bRet = false;
                }
                else
                {
                    this.SetInfoMessage(3);
                    bRet = true;
                }
            }
            return bRet;
        }

        private void SetInfoMessage(int MessageID)
        {
            this.lblErrorMsg.ForeColor = Color.Red;
            switch (MessageID)
            {
                case 0:
                    this.lblErrorMsg.Text = "Please specify old password.";
                    break;
                case 1:
                    this.lblErrorMsg.Text = "New password doesn't match.";
                    break;
                case 2:
                    this.lblErrorMsg.Text = "Don't enter the old password.";
                    break;
                case 3:
                    this.lblErrorMsg.Text = "OK Pass!";
                    this.lblErrorMsg.ForeColor = Color.Green;
                    break;
                default:
                    this.lblErrorMsg.Text = "";
                    break;
            }
            if (this.lblErrorMsg.Text.Trim() != "")
            {
                this.lblErrorMsg.Visible = true;
            }
            else
            {
                this.lblErrorMsg.Visible = false;
            }
        }

        private void ChangePassword()
        {
            if (global::CIC.Program.m_Session != null)
            {
                ININ.IceLib.Connection.Extensions.Security SecurityObject = new ININ.IceLib.Connection.Extensions.Security(global::CIC.Program.m_Session);
                ININ.IceLib.Connection.Extensions.PasswordPolicy ppp = SecurityObject.GetPasswordPolicy();
                if (this.Verify_Password() == true)
                {
                    SecurityObject.SetPasswordAsync(this.txtOldPassword.Text, this.txtNewPassword.Text, ChangePasswordCompleted, null);
                }
            }
        }

        private void ChangePasswordCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {

                string[] errorMsg = e.Error.Message.Split('\n');
                if (errorMsg.Length > 0)
                {
                    string errorStr = "";
                    for (int i = 0; i < errorMsg.Length; i++)
                    {
                        errorMsg[i] = errorMsg[i].Replace('\n', ' ');
                        errorStr += errorMsg[i].ToString()+".";
                    }
                    this.lblErrorMsg.Text = errorStr;
                }
                else
                {
                    this.lblErrorMsg.Text = e.Error.Message;
                }
                this.lblErrorMsg.ForeColor = Color.Red;
            }
            else
            {
                this.Close();
            }
        }

        private void btnChangePassword_Click(object sender, EventArgs e)
        {
            this.ChangePassword();
        }

        private void txtNewPassword_TextChanged(object sender, EventArgs e)
        {
            bool bret = this.Verify_Password();
        }

        private void txtRetypeNewPassword_TextChanged(object sender, EventArgs e)
        {
            bool bret = this.Verify_Password();
        }

        private void txtOldPassword_TextChanged(object sender, EventArgs e)
        {
            bool bret = this.Verify_Password();
        }

        private void frmChangePassword_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (global::CIC.Properties.Settings.Default.Animate == true)
            {
                bool b = Native.AnimateWindow(this.Handle, 1000, Native.AW_BLEND | Native.AW_HIDE);
                base.OnClosing(e);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

    }
}
