using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1
{
    partial class Form1
    {
        public static Dictionary<char, int> DD키맵 = new Dictionary<char, int>()
        {
                                                                                                                        
            {'1', 201}, {'2', 202}, {'3', 203}, {'4', 204}, {'5', 205}, {'6', 206}, {'7', 207}, {'8', 208}, {'9', 209}, {'0', 210},
            {'q', 301}, {'w', 302}, {'e', 303}, {'r', 304}, {'t', 305}, {'y', 306}, {'u', 307}, {'i', 308}, {'o', 309}, {'p', 310},
            {'a', 401}, {'s', 402}, {'d', 403}, {'f', 404}, {'g', 405}, {'h', 406}, {'j', 407}, {'k', 408}, {'l', 409},
            {'z', 501}, {'x', 502}, {'c', 503}, {'v', 504}, {'b', 505}, {'n', 506}, {'m', 507}, {'D', 509}, {'_', 603}
        };
        public enum 조합키 {
            Ctrl = 600,
            //Alt,
            Shift = 500
        }
        public void Send(조합키 조합키, string str) {
            
            DD_key((int)조합키, 1);
            대기(딜레이(키입력지연));
            Send(str);
            DD_key((int)조합키, 2);
            대기(딜레이(키입력지연));
        }
        private void Send(string str) {
            foreach (char c in str.ToCharArray()) {
                send_key(DD키맵[c]);
            }
        }
    }
}
