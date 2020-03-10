using Pixel.Automation.Core.Interfaces;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Pixel.Automation.Native.Windows
{
    /// <summary>
    /// Credits : https://github.com/TestStack/White/blob/master/src/TestStack.White/ScreenCapture.cs
    /// </summary>
    public class ScreenCapture : IScreenCapture
    {
        [DllImport("gdi32.dll")]
        static extern bool BitBlt(IntPtr hdcDest, int xDest, int yDest, int wDest, int hDest, IntPtr hdcSource, int xSrc, int ySrc, CopyPixelOperation rop);

        [DllImport("user32.dll")]
        static extern bool ReleaseDC(IntPtr hWnd, IntPtr hDc);

        [DllImport("gdi32.dll")]
        static extern IntPtr DeleteDC(IntPtr hDc);

        [DllImport("gdi32.dll")]
        static extern IntPtr DeleteObject(IntPtr hDc);

        [DllImport("gdi32.dll")]
        static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);

        [DllImport("gdi32.dll")]
        static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        static extern IntPtr SelectObject(IntPtr hdc, IntPtr bmp);

        [DllImport("user32.dll")]
        public static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowDC(IntPtr ptr);


        public virtual Bitmap CaptureDesktop()
        {
            //TODO : Size is hard coded. How to get it without windows.Forms?
            //var bounds = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
            var bounds = new Rectangle(0, 0, 1920, 1080);
            var sz = bounds.Size;
            var hDesk = GetDesktopWindow();
            var hSrce = GetWindowDC(hDesk);
            var hDest = CreateCompatibleDC(hSrce);
            var hBmp = CreateCompatibleBitmap(hSrce, sz.Width, sz.Height);
            var hOldBmp = SelectObject(hDest, hBmp);
            BitBlt(hDest, 0, 0, sz.Width, sz.Height, hSrce, 0, 0, CopyPixelOperation.SourceCopy | CopyPixelOperation.CaptureBlt);
            Bitmap bmp = System.Drawing.Image.FromHbitmap(hBmp);
            SelectObject(hDest, hOldBmp);
            DeleteObject(hBmp);
            DeleteDC(hDest);
            ReleaseDC(hDesk, hSrce);
            return bmp;
        }

        public virtual Bitmap CaptureArea(Rectangle rectangle)
        {
            var bmp = new Bitmap(rectangle.Width, rectangle.Height, PixelFormat.Format32bppArgb);
            var graphics = Graphics.FromImage(bmp);
            graphics.CopyFromScreen(rectangle.Left, rectangle.Top, 0, 0, new Size(rectangle.Width, rectangle.Height), CopyPixelOperation.SourceCopy);
            return bmp;
        }

    }
}
