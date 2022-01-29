using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace WindowsFormsApp1
{
    public class 카드정보
    {
        public int 번호;
        public int 위치;
        public Bitmap 사진;
        public int in_deck;
        public int in_hand =0;
        public string 나올확률1;
        public string 나올확률2;
        public bool died = false;

        public 카드정보() { }
        public 카드정보(int 번호, int 카드갯수, string 나올확률1="", string 나올확률2="") 
        {
            this.번호 = 번호;
            this.in_deck = 카드갯수;
            this.나올확률1 = 나올확률1;
            this.나올확률2 = 나올확률2;
        }
    }
}
