using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1.Models
{
    public class IncomeReport: ModelBase
    {
        public string AccountName { get; set; }
        public ServerType Server { get; set; }
        public long Income { get; set; }
        public long Total { get; set; }
    }
}
