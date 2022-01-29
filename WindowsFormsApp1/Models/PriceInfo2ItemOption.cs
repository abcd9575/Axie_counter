using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1.Models
{
    public class PriceInfo2ItemOption : ModelBase
    {
        public int PriceInfoId { get; set; } // 1
        public virtual PriceInfo PriceInfo { get; set; }

        public int ItemOptionId { get; set; } //  2: 힘, 3: 올스텟
        public virtual ItemOption ItemOption { get; set; } 
    }
}