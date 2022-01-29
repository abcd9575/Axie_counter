using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KeyMoLib
{
    internal static class WindowsApiWrapper
    {
        /// <summary>
        /// Moves the cursor to the specified screen coordinates. If the new coordinates are not within the screen rectangle set by the most recent ClipCursor function call, the system automatically adjusts the coordinates so that the cursor stays within the rectangle.
        /// </summary>
        /// <param name="x">The new x-coordinate of the cursor, in screen coordinates</param>
        /// <param name="y">The new y-coordinate of the cursor, in screen coordinates</param>
        /// <returns>Returns nonzero if successful or zero otherwise. To get extended error information, call GetLastError.</returns>
        [DllImport("user32.dll", EntryPoint = "SetCursorPos", SetLastError = true)]
        internal static extern int SetCursorPos(int x, int y);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern Keys VkKeyScan(char ch);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern uint MapVirtualKey(int uCode, uMapType uMapType);
        

        /// <summary>
        /// Synthesizes keystrokes, mouse motions, and button clicks
        /// </summary>
        /// <param name="nInputs">The number of structures in the pInputs array</param>
        /// <param name="pInputs">An array of INPUT structures. Each structure represents an event to be inserted into the keyboard or mouse input stream.</param>
        /// <param name="cbSize">The size, in bytes, of an INPUT structure. If cbSize is not the size of an INPUT structure, the function fails</param>
        /// <returns>The function returns the number of events that it successfully inserted into the keyboard or mouse input stream. If the function returns zero, the input was already blocked by another thread. To get extended error information, call GetLastError.</returns>
        [DllImport("user32.dll", SetLastError = true)]
        internal static extern uint SendInput(
            int nInputs,
            [MarshalAs(UnmanagedType.LPArray), In] INPUT[] pInputs,
            int cbSize);

    }
}
