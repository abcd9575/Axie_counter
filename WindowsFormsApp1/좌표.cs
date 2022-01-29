using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1
{
    public class 좌표
    {
        public int 넓이;
        public int 높이;
        public int x;
        public int y;

        public 좌표() { }
        public 좌표(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
        public 좌표(int x, int y,int 넓이=0, int 높이=0) : this(x,y)
        {
            this.넓이 = 넓이;
            this.높이 = 높이;
        }
        public 좌표(int[] arr) : this(arr[0], arr[1], arr[2], arr[3]) { }
    }
}
