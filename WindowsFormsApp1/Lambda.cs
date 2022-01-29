using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1
{
    partial class Form1
    {
        private int 딜레이(int i) => i += new Random().Next(0, 3);
        private void send_enter(int 횟수=1) => send_key(313, 횟수);
        private void send_BS(int 횟수 = 1) => send_key(214, 횟수);
        private void send_위(int 횟수 = 1) => send_key(709, 횟수);
        private void send_아래(int 횟수 = 1) => send_key(711, 횟수);
        private void send_오른쪽(int 횟수 = 1) => send_key(712, 횟수);
        private void send_왼쪽(int 횟수 = 1) => send_key(710, 횟수);

        private void mouseclick(int[] point) => mouseclick(point[0], point[1]);
        private void mouseclick(좌표 p) => mouseclick(new int[] { p.x, p.y });

        private void mousemove(int[] point) => mousemove(point[0], point[1]);
        private void mousemove(좌표 p) => mousemove(new int[] { p.x, p.y });
        private void 스크린샷(좌표 TopLeft, 좌표 BottomRight) => 스크린샷(TopLeft.x, BottomRight.x, TopLeft.y, BottomRight.y);
    }
}
