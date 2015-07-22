using SlimDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace LightingEngine_v2
{
    internal static class Helper
    {
        public static void Dispose(params IDisposable[] obj)
        {
            IDisposable[] disp = obj.ToArray();
            for (int i = 0; i < disp.Length; i++)
            {
                if (disp[i] != null)
                {
                    if (disp[i] is ComObject)
                    {
                        if ((disp[i] as ComObject).Disposed)
                        {
                            disp[i] = null;
                            continue;
                        }
                    }
                    try
                    {
                        disp[i].Dispose();
                        disp[i] = null;
                    }
                    catch { }
                }
            }
        }

        public static void Dispose<T>(params List<T>[] obj) where T : IDisposable
        {
            for (int j = 0; j < obj.Length; j++)
            {
                T[] disp = obj[j].ToArray();
                Dispose((IDisposable[])(object)disp);
            }
        }

        [DllImport("User32.dll")]
        private static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetCursorPos(ref Win32Point pt);

        [StructLayout(LayoutKind.Sequential)]
        struct Win32Point
        {
            public Int32 X;
            public Int32 Y;
        };

        public static void SetMousePosition(Point p)
        {
            SetCursorPos((int)p.X, (int)p.Y);
        }

        public static Point GetMousePosition()
        {
            Win32Point w32Mouse = new Win32Point();
            GetCursorPos(ref w32Mouse);
            return new Point(w32Mouse.X, w32Mouse.Y);
        }
    }
}
