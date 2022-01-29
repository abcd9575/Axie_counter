using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1.Models
{
    public class PriceInfo : ModelBase
    {
        // 스카니아 서버 힘 30 6 가격이 5000만 

        
        /*
         * 시세 정렬 기준
         *  - 올스텟 3-7%
         *  
         *  시세 수집 시기
         *  - 판매 등록
         *  - 시세전담 PC 
         */

        //아이템 이름/정보
        public string ItemName { get; set; }
        public decimal Price { get; set; }
        public ServerType Server { get; set; }

        public virtual ICollection<PriceInfo2ItemOption> PriceInfo2ItemOptions { get; set; }
    }
}
