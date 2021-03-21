using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;

namespace testsv
{
    class Program
    {
        public static byte[] ImageToByteArray(System.Drawing.Image imageIn)
        {
            using (var ms = new MemoryStream())
            {
                imageIn.Save(ms, ImageFormat.Png);
                return ms.ToArray();
            }
        }

        public static Image CaptureWindow(IntPtr handle)
        {
            // get te hDC of the target window
            IntPtr hdcSrc = User32.GetWindowDC(handle);
            // get the size
            User32.RECT windowRect = new User32.RECT();
            User32.GetWindowRect(handle, ref windowRect);
            int width = windowRect.right - windowRect.left;
            int height = windowRect.bottom - windowRect.top;
            // create a device context we can copy to
            IntPtr hdcDest = GDI32.CreateCompatibleDC(hdcSrc);
            // create a bitmap we can copy it to,
            // using GetDeviceCaps to get the width/height
            IntPtr hBitmap = GDI32.CreateCompatibleBitmap(hdcSrc, width, height);
            // select the bitmap object
            IntPtr hOld = GDI32.SelectObject(hdcDest, hBitmap);
            // bitblt over
            GDI32.BitBlt(hdcDest, 0, 0, width, height, hdcSrc, 0, 0, GDI32.SRCCOPY);
            // restore selection
            GDI32.SelectObject(hdcDest, hOld);
            // clean up 
            GDI32.DeleteDC(hdcDest);
            User32.ReleaseDC(handle, hdcSrc);
            // get a .NET image object for it
            Image img = Image.FromHbitmap(hBitmap);
            // free up the Bitmap object
            GDI32.DeleteObject(hBitmap);
            return img;
        }

        static byte[] TakeSS()
        {
            Image img = CaptureWindow(User32.GetDesktopWindow());
            return ImageToByteArray(img);
            //File.WriteAllBytes("testpng.png", Image);
        }

        static void Main(string[] args)
        {
            IPAddress ipAddress = IPAddress.Parse("127.0.0.1");

            Console.WriteLine("Starting TCP listener...");

            TcpListener listener = new TcpListener(ipAddress, 500);

            listener.Start();

            byte[] data = new byte[65536];

            while (true)
            {
                Socket client = listener.AcceptSocket();
                Console.WriteLine("Connection accepted.");

                var childSocketThread = new Thread(() =>
                {
                    
                    int size = client.Receive(data);
                    //Console.WriteLine(size);
                    Console.WriteLine("Recieved data: ");

                    string WhatToDo = "";

                    for (int i = 0; i < size; i++)
                    {
                        WhatToDo += Convert.ToChar(data[i]);
                    }

                    if (WhatToDo.Contains("choice="))
                    {
                        WhatToDo = StringUtils.StringUtils.RemoveEverythingBeforeFirst(WhatToDo, "choice=");
                        WhatToDo = WhatToDo.Remove(0, 7);
                        WhatToDo = WhatToDo.ToLower();

                        if (WhatToDo[0] == 'a')
                        {
                            PaintPixel(1, 1);
                        }
                        else if (WhatToDo[0] == 'b')
                        {
                            PaintPixel(1919, 1);
                        }
                        else if (WhatToDo[0] == 'c')
                        {
                            PaintPixel(1, 1079);
                        }
                        else if (WhatToDo[0] == 'd')
                        {
                            PaintPixel(1919, 1079);
                        }
                        else
                            InvalidateRect(IntPtr.Zero, IntPtr.Zero, true);

                        client.Send(Encoding.ASCII.GetBytes("1"));
                    }
                    else
                    {
                        string img = Convert.ToBase64String(TakeSS());
                        string Resp = File.ReadAllText("resp.txt") + $"Content-Type: image/png\n" + img;
                        //Console.WriteLine("RESP:");
                        //Console.WriteLine(Resp);
                        client.Send(Encoding.UTF8.GetBytes(Resp));
                    }
                    
                    
                    //Console.WriteLine(WhatToDo);

                   
                    client.Close();
                });

                childSocketThread.Start();
            }
        }

        [DllImport("User32.dll")]
        public static extern IntPtr GetDC(IntPtr hwnd);
        [DllImport("User32.dll")]
        public static extern void ReleaseDC(IntPtr hwnd, IntPtr dc);

        [DllImport("user32.dll")]
        static extern bool InvalidateRect(IntPtr hWnd, IntPtr lpRect, bool bErase);

        public static IntPtr desk = GetDC(IntPtr.Zero);
        static Graphics g = Graphics.FromHdc(desk);

        private static void PaintPixel(int xpos, int ypos)
        {
            g.FillRectangle(Brushes.White, new Rectangle(xpos, ypos, 1, 1));
        }
    }

    public class User32
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }
        [DllImport("user32.dll")]
        public static extern IntPtr GetDesktopWindow();
        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowDC(IntPtr hWnd);
        [DllImport("user32.dll")]
        public static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDC);
        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowRect(IntPtr hWnd, ref RECT rect);
    }

    public class GDI32
    {

        public const int SRCCOPY = 0x00CC0020; // BitBlt dwRop parameter
        [DllImport("gdi32.dll")]
        public static extern bool BitBlt(IntPtr hObject, int nXDest, int nYDest,
            int nWidth, int nHeight, IntPtr hObjectSource,
            int nXSrc, int nYSrc, int dwRop);
        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateCompatibleBitmap(IntPtr hDC, int nWidth,
            int nHeight);
        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateCompatibleDC(IntPtr hDC);
        [DllImport("gdi32.dll")]
        public static extern bool DeleteDC(IntPtr hDC);
        [DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);
        [DllImport("gdi32.dll")]
        public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);
    }
}
