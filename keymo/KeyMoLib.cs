using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KeyMoLib
{
    public static class KeyMoLib
    {
        public static TimeSpan ExplictTime = TimeSpan.FromMilliseconds(200);

        public static uint DD_key(Keys keyCode, EventType type = EventType.Click)
        {

            KEYEVENTF KeyboardEvent;
            switch (type)
            {
                case EventType.Down:
                    KeyboardEvent = KEYEVENTF.KEYDOWN;
                    break;
                case EventType.Up:
                    KeyboardEvent = KEYEVENTF.KEYUP;
                    break;
                default:
                case EventType.Click:
                    DD_key(keyCode, EventType.Down);
                    return DD_key(keyCode, EventType.Up);
            }
            var Inputs = new INPUT[] {
                new INPUT{
                    type = InputType.INPUT_KEYBOARD,
                    U = new InputUnion {
                        ki = new KEYBDINPUT{
                            wVk = keyCode,
                            dwFlags = KeyboardEvent
                        }
                    }
                },
            };
            uint Result = WindowsApiWrapper.SendInput(1, Inputs, INPUT.Size);
            Task.Delay(ExplictTime).GetAwaiter().GetResult();
            return Result;
        }
        public static uint DD_str(string InputString)
        {
            var Inputs = new List<INPUT>();
            foreach (var ch in InputString.ToLower().ToCharArray())
            {
                //Inputs.Add(new INPUT
                //{
                //    type = InputType.INPUT_KEYBOARD,
                //    U = {
                //            ki = {
                //                wVk = WindowsApiWrapper.VkKeyScan(ch),
                //                dwFlags = KEYEVENTF.KEYDOWN
                //            }
                //        }
                //});
                Inputs.Add(new INPUT
                {
                    type = InputType.INPUT_KEYBOARD,
                    U = {
                                ki = {
                                    wVk = WindowsApiWrapper.VkKeyScan(ch),
                                    dwFlags = KEYEVENTF.KEYUP
                                }
                            }
                });
            }
            uint Result = WindowsApiWrapper.SendInput(Inputs.Count, Inputs.ToArray(), INPUT.Size);
            Task.Delay(ExplictTime).GetAwaiter().GetResult();
            return Result;
        }
        public static void DD_mov(int x, int y) => WindowsApiWrapper.SetCursorPos(x, y);

        public static uint DD_whl(int flag)
        {
            const int WHEEL_DELTA_DOWN = 120;
            const int WHEEL_DELTA_UP = -120;

            uint Result = WindowsApiWrapper.SendInput(1, new INPUT[] {
                new INPUT{
                    type = InputType.INPUT_MOUSE,
                    U = {
                        mi = {
                            mouseData = (flag == 1)? WHEEL_DELTA_UP : WHEEL_DELTA_DOWN,
                            dwFlags = MOUSEEVENTF.WHEEL
                        }
                    }
                }
            }, INPUT.Size);
            Task.Delay(ExplictTime).GetAwaiter().GetResult();
            return Result;
        }
        public static uint DD_btn(int flags)
        {
            var Input = new INPUT
            {
                type = InputType.INPUT_MOUSE,
                U = {
                    mi = {
                        dx = 0,
                        dy = 0
                    }
                }
            };
            switch (flags)
            {
                case 1: //KEYDOWN
                    Input.U.mi.dwFlags = MOUSEEVENTF.LEFTDOWN;
                    break;
                case 2: //KEYUP
                    Input.U.mi.dwFlags = MOUSEEVENTF.LEFTUP;
                    break;
            }
            uint Result = WindowsApiWrapper.SendInput(1, new INPUT[] { Input }, INPUT.Size);
            Task.Delay(ExplictTime).GetAwaiter().GetResult();
            return Result;
        }
    }
}
