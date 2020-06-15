//  Native methods to help with mouse movement and key sending.
//  Copyright(C) 2016  Chris K
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.If not, see<http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace VoiceCoder.Util
{
    public class Native
    {
        public const uint MOUSE_LEFT_DOWN = 0x02;
        public const uint MOUSE_LEFT_UP = 0x04;
        public const uint MOUSE_RIGHT_DOWN = 0x08;
        public const uint MOUSE_RIGHT_UP = 0x10;
        public const uint MOUSE_BUTTON_LEFT = MOUSE_LEFT_DOWN | MOUSE_LEFT_UP;
        public const uint MOUSE_BUTTON_RIGHT = MOUSE_RIGHT_DOWN | MOUSE_RIGHT_UP;

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);

        [DllImport("user32.dll", SetLastError = true)]
        static extern UInt32 GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, UInt32 dwNewLong);

        // Source: http://stackoverflow.com/questions/1316681/getting-mouse-position-in-c-sharp
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;

            public static implicit operator Point(POINT point)
            {
                return new Point(point.X, point.Y);
            }
        }

        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out POINT lpPoint);

        public static Point GetCursorPosition()
        {
            POINT lpPoint;
            GetCursorPos(out lpPoint);
            return lpPoint;
        }

        public static void SetCursorPosition(Point point)
        {
            if (point != null)
            {
                Cursor.Position = point;
            }
        }

        public static Rectangle GetScreenDimensions()
        {
            return Screen.PrimaryScreen.WorkingArea;
        }

        public static void DoMouseClick(bool leftButton)
        {
            mouse_event(leftButton ? MOUSE_BUTTON_LEFT : MOUSE_BUTTON_RIGHT, (uint)Cursor.Position.X, (uint)Cursor.Position.Y, 0, 0);
        }

        public static void DoMouseDrag(bool isDragging)
        {
            mouse_event(isDragging ? MOUSE_LEFT_DOWN : MOUSE_LEFT_UP, (uint)Cursor.Position.X, (uint)Cursor.Position.Y, 0, 0);
        }

        public static void MoveMouseOffset(int xOffset, int yOffset)
        {
            Point currentPoint = GetCursorPosition();
            SetCursorPosition(new Point(currentPoint.X + xOffset, currentPoint.Y + yOffset));
        }

        public static void MoveMouseAbsolute(int xOffset, int yOffset)
        {
            Cursor.Position = new Point(xOffset, yOffset);
        }

        public static void EmitKeys(String data)
        {
            if (data == null || data == "")
            {
                return;
            }

            IntPtr p = IntPtr.Zero;
            p = GetForegroundWindow();
            if (p != IntPtr.Zero)
            {
                SendKeys.SendWait(data);
                SendKeys.Flush();
            }
        }
    }
}
