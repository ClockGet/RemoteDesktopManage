using AxMSTSCLib;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.ComponentModel;

namespace 远程桌面管理器
{
    extern alias wrapper;
    public partial class FullScreenForm : Form
    {
        bool m_IsFullScreen = false;
        const int WM_NCLBUTTONDBLCLK = 0xA3;
        const int WM_NCLBUTTONCLK = 0xA1;
        public FullScreenForm()
        {
            InitializeComponent();
        }

        /// <summary>  
        /// 设置全屏或这取消全屏  
        /// </summary>  
        /// <param name="fullscreen">true:全屏 false:恢复</param>  
        /// <param name="rectOld">设置的时候，此参数返回原始尺寸，恢复时用此参数设置恢复</param>  
        /// <returns>设置结果</returns>  
        public Boolean SetFormFullScreen(Boolean fullscreen)//, ref Rectangle rectOld
        {
            m_IsFullScreen = fullscreen;
            Rectangle rectOld = Rectangle.Empty;
            //Int32 hwnd = 0;
            //hwnd = FindWindow("Shell_TrayWnd", null);//获取任务栏的句柄

            //if (hwnd == 0) return false;

            if (fullscreen)//全屏
            {
                //ShowWindow(hwnd, SW_HIDE);//隐藏任务栏

                SystemParametersInfo(SPI_GETWORKAREA, 0, ref rectOld, SPIF_UPDATEINIFILE);//get  屏幕范围
                Rectangle rectFull = Screen.PrimaryScreen.Bounds;//全屏范围
                SystemParametersInfo(SPI_SETWORKAREA, 0, ref rectFull, SPIF_UPDATEINIFILE);//窗体全屏幕显示

                this.FormBorderStyle = FormBorderStyle.None;
                this.WindowState = FormWindowState.Maximized;
            }
            else//还原 
            {
                this.WindowState = FormWindowState.Normal;
                this.FormBorderStyle = FormBorderStyle.Sizable;

                //ShowWindow(hwnd, SW_SHOW);//显示任务栏

                SystemParametersInfo(SPI_SETWORKAREA, 0, ref rectOld, SPIF_UPDATEINIFILE);//窗体还原
            }
            return true;
        }

        #region user32.dll

        [DllImport("user32.dll", EntryPoint = "ShowWindow")]
        public static extern Int32 ShowWindow(Int32 hwnd, Int32 nCmdShow);
        public const Int32 SW_SHOW = 5; public const Int32 SW_HIDE = 0;

        [DllImport("user32.dll", EntryPoint = "SystemParametersInfo")]
        private static extern Int32 SystemParametersInfo(Int32 uAction, Int32 uParam, ref Rectangle lpvParam, Int32 fuWinIni);
        public const Int32 SPIF_UPDATEINIFILE = 0x1;
        public const Int32 SPI_SETWORKAREA = 47;
        public const Int32 SPI_GETWORKAREA = 48;

        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        private static extern Int32 FindWindow(string lpClassName, string lpWindowName);

        #endregion
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_NCLBUTTONDBLCLK)
            {
                m_IsFullScreen = !m_IsFullScreen;
                SetFormFullScreen(m_IsFullScreen);
                foreach(var control in this.Controls)
                {
                    if (control is AxMsRdpClient7)
                    {
                        ((AxMsRdpClient7)control).FullScreen = true;
                        break;
                    }
                }
                return;
            }
            else if (m.Msg == WM_NCLBUTTONCLK)
            {
                Point mousePoint = new Point((int)m.LParam);
                mousePoint.Offset(-this.Left, -this.Top);
                if (new Rectangle(0, 0, 32, 32).Contains(mousePoint))
                {
                    var mainForm = this.Owner as MainForm;
                    if (mainForm != null)
                    {
                        mainForm.AttachFromChild(this.Text, this.Controls);
                    }
                    this.Controls.Clear();
                    this.Close();
                    return;
                }
            }
            base.WndProc(ref m);
        }
        #region 控件事件
        //bool mouseDown = true;
        //private void FullScreenForm_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        //{
        //    if (e.KeyCode == Keys.F11)
        //    {
        //        m_IsFullScreen = !m_IsFullScreen;
        //        SetFormFullScreen(m_IsFullScreen);
        //        e.Handled = true;
        //    }
        //    else if (e.KeyCode == Keys.Escape)//esc键盘退出全屏
        //    {
        //        if (m_IsFullScreen)
        //        {
        //            e.Handled = true;
        //            this.WindowState = FormWindowState.Normal;//还原  
        //            this.FormBorderStyle = FormBorderStyle.Sizable;
        //            SetFormFullScreen(false);
        //        }
        //    }
        //}
        //private void FullScreenForm_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        //{
        //    if (mouseDown)
        //    {
        //        Cursor = Cursors.Hand;
        //        // 获取当前屏幕的光标坐标
        //        Point pTemp = new Point(Cursor.Position.X, Cursor.Position.Y);
        //        // 转换成工作区坐标
        //        pTemp = this.PointToScreen(pTemp);
        //        this.Location = new Point(pTemp.X - this.Width / 2, pTemp.Y - SystemInformation.CaptionHeight / 2);
        //    }
        //}
        //private void FullScreenForm_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        //{
        //    mouseDown = false;
        //}
        #endregion
    }
}
