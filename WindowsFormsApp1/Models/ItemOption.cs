using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1.Models
{
    public class ItemOption : ModelBase
    {
        public StatType Stat { get; set; }
        public int Value { get; set; }

        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(ItemOption)) {
                var inst = (ItemOption)obj;
                return inst.Stat == this.Stat && inst.Value == this.Value;
            }
            return false;
        }

        public virtual ICollection<PriceInfo2ItemOption> PriceInfo2ItemOptions { get; set; }
    }
}
