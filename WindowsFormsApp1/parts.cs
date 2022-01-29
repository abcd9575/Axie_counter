using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1
{
    partial class Form1

    {
        public static Dictionary<string, string> 파츠맵 = new Dictionary<string, string>()
        {

            {"니모", "nimo_tail"}, {"샌달", "sandal"}, {"리스키피쉬", "risky_fish"}, {"쿠쿠", "cock"}, {"코이", "koi"}, {"퍼치", "perch"}, {"람", "lam_angr"}, {"플라이어", "pli"}, {"피라냐", "pira"}, {"골드피쉬", "goldf"},
            {"나바가", "nava" }, {"블루문", "blue" }, {"오란다", "oran" }, {"민트", "mint" }, {"터틀", "tiny_tu" }, {"펌킨", "pump" }, {"핫벗", "hot_b" }, {"스폰지", "spon" }, {"터닙", "turnip" }, {"캐럿", "carrot_h" },
            {"시리어스", "serious_" }, {"로닌", "ronin" }, {"듀블", "dual" }, {"캣피쉬", "cat" }, {"넛크랙", "nut_cracker_nut_c" }, {"지그재그", "zig" }, {"스네일쉘", "snai" }, {"래깅", "lagg" }, {"아네모네등", "aqua_v" },
            {"아네모네뿔", "aquap" }, {"쇼얼스타", "shoal" }, {"쉘터", "hermi" }, {"아코", "arc" }, {"클램쉘", "clam" }, {"바빌", "baby" }, {"고다", "goda" }, {"핫썬", "leek" },{"캑터스", "cac" },{"넛스로", "nut_t" },{"리틀브랜치","bra"}
            ,{"리스키비스트","risky_be"},{"큐트버니","cute"},{"안테나","ante"},{"인싸이저","inci"},{"가리시웜","gari"},{"허밋","hermit"}
        };
        public string 파츠(string str)
        {
            string 값= 파츠맵.FirstOrDefault(a => a.Key == str).Value;
            return 값;
        }
    }
}
/*
namespace WindowsFormsApp1
{
    partial class Form1
    {
        public static Dictionary<char, int> DD키맵 = new Dictionary<char, int>()
        {

            {'1', 201}, {'2', 202}, {'3', 203}, {'4', 204}, {'5', 205}, {'6', 206}, {'7', 207}, {'8', 208}, {'9', 209}, {'0', 210},
            {'q', 301}, {'w', 302}, {'e', 303}, {'r', 304}, {'t', 305}, {'y', 306}, {'u', 307}, {'i', 308}, {'o', 309}, {'p', 310},
            {'a', 401}, {'s', 402}, {'d', 403}, {'f', 404}, {'g', 405}, {'h', 406}, {'j', 407}, {'k', 408}, {'l', 409},
            {'z', 501}, {'x', 502}, {'c', 503}, {'v', 504}, {'b', 505}, {'n', 506}, {'m', 507}, {'D', 509},
        };
        public enum 조합키
        {
            Ctrl = 600,
            //Alt,
            Shift = 500
        }
        public void Send(조합키 조합키, string str)
        {

            DD_key((int)조합키, 1);
            대기(딜레이(키입력지연));
            Send(str);
            DD_key((int)조합키, 2);
            대기(딜레이(키입력지연));
        }
        private void Send(string str)
        {
            foreach (char c in str.ToCharArray())
            {
                send_key(DD키맵[c]);
            }
        }
    }
}
*/