using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsFormsApp1.Models;

namespace WindowsFormsApp1
{
    public class 시세정보
    {
        private static Dictionary<ServerType, 시세정보> Cached = new Dictionary<ServerType, 시세정보>();

        public static 시세정보 가져오기(ServerType 서버이름)
        {
            if (Cached.ContainsKey(서버이름) == false)
                Cached.Add(서버이름, new 시세정보());

            return Cached[서버이름];
        }
        public string 올5 = "";
        public string 올6 = "";
        public string 올7 = "";

        public Dictionary<string, string> 스탯가격 = new Dictionary<string, string>();

        public string 스탯가격가져오기(string 검색옵션)
        {
            if (스탯가격.ContainsKey(검색옵션) == false)
                스탯가격.Add(검색옵션, "");
            스탯가격.TryGetValue(검색옵션, out string 가격);
            return 가격;
        }
        //public void 스탯가격저장하기(string 검색옵션 ,string 가격)
        //{
        //    스탯가격.Single(x => x.Key == 검색옵션).Value == 가격;
        //}


    }
}
