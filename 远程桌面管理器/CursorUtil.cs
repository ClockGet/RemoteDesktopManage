using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace 远程桌面管理器
{
    extern alias wrapper;
    public struct IconInfo
    {
        public bool fIcon;
        public int xHotspot;
        public int yHotspot;
        public IntPtr hbmMask;
        public IntPtr hbmColor;
    }

    public class CursorUtil
    {
        // Based on the article and comments here:
        // http://www.switchonthecode.com/tutorials/csharp-tutorial-how-to-use-custom-cursors
        // Note that the returned Cursor must be disposed of after use, or you'll leak memory!
        public static Cursor CreateCursor(Bitmap bm, int xHotspot, int yHotspot)
        {
            IntPtr cursorPtr;
            IntPtr ptr = bm.GetHicon();
            IconInfo tmp = new IconInfo();
            User32.GetIconInfo(ptr, ref tmp);
            tmp.xHotspot = xHotspot;
            tmp.yHotspot = yHotspot;
            tmp.fIcon = false;
            cursorPtr = User32.CreateIconIndirect(ref tmp);

            if (tmp.hbmColor != IntPtr.Zero) GDI32.DeleteObject(tmp.hbmColor);
            if (tmp.hbmMask != IntPtr.Zero) GDI32.DeleteObject(tmp.hbmMask);
            if (ptr != IntPtr.Zero) User32.DestroyIcon(ptr);

            return new Cursor(cursorPtr);
        }
        static readonly int destHeight = 500;
        static readonly int destWidth = 500;
        public static Bitmap AsBitmap(Control c)
        {
            int sw = 0, sh = 0;
            int sWidth = c.Width;
            int sHeight = c.Height;

            if (sHeight > destHeight || sWidth > destWidth)
            {
                if ((sWidth * destHeight) > (sHeight * destWidth))
                {
                    sw = destWidth;
                    sh = (destWidth * sHeight) / sWidth;
                }
                else
                {
                    sh = destHeight;
                    sw = (sWidth * destHeight) / sHeight;
                }
            }
            else
            {
                sw = sWidth;
                sh = sHeight;
            }

            using (Bitmap bm = new Bitmap(c.Width, c.Height))
            {
                using (Graphics g = Graphics.FromImage(bm))
                {
                    g.CopyFromScreen(c.PointToScreen(Point.Empty), Point.Empty, c.Size-new Size(18,18));//减去滚动条占得大小
                }
                //c.DrawToBitmap(bm, new Rectangle(0, 0, c.Width, c.Height));
                Bitmap bmp = new Bitmap(sw, sh);
                //bm.MakeTransparent();
                using (Graphics gfx = Graphics.FromImage(bmp))
                {
                    ColorMatrix matrix = new ColorMatrix();
                    matrix.Matrix33 = 0.6f;
                    using (ImageAttributes attributes = new ImageAttributes())
                    {
                        attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                        //gfx.Clear(Color.Transparent);
                        gfx.DrawImage(bm, new Rectangle(0, 0, bmp.Width, bmp.Height), 0, 0, bm.Width, bm.Height, GraphicsUnit.Pixel, attributes);
                    }  
                    Rectangle rect = new Rectangle(Point.Empty, Cursors.Default.Size);
                    Cursors.Default.Draw(gfx, rect);
                }
                return bmp;
            }
        }
        public static Bitmap CaptureWindow(IntPtr handle)
        {
            IntPtr hdcSrc = User32.GetWindowDC(handle);
            RECT windowRect = new RECT();
            User32.GetWindowRect(handle, ref windowRect);
            int width = windowRect.Right - windowRect.Left;
            int height = windowRect.Bottom - windowRect.Top;
            IntPtr hdcDest = GDI32.CreateCompatibleDC(hdcSrc);
            IntPtr hBitmap = GDI32.CreateCompatibleBitmap(hdcSrc, width, height);
            IntPtr hOld = GDI32.SelectObject(hdcDest, hBitmap);
            GDI32.BitBlt(hdcDest, 0, 0, width, height, hdcSrc, 0, 0, TernaryRasterOperations.SRCCOPY);
            GDI32.SelectObject(hdcDest, hOld);
            GDI32.DeleteDC(hdcDest);
            User32.ReleaseDC(handle, hdcSrc);
            Bitmap bitmap = Bitmap.FromHbitmap(hBitmap);
            GDI32.DeleteObject(hBitmap);
            return bitmap;
        }
    }
}
