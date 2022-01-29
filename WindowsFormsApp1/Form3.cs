using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{

    public partial class Form3 : Form
    {
        private int lastround_cardcnt;
        private int gained_cardcnt;
        public int Passvalue_card_cnt
        {
            get => this.lastround_cardcnt;
            set => numericUpDown1.Value = lastround_cardcnt = value;
        }
        public int Passvalue_gain_cnt
        {
            get => this.gained_cardcnt;
            set => numericUpDown2.Value = gained_cardcnt = value;
        }
        /*        protected override void OnLoad(EventArgs e) // Form1_Load 안쓰임. 대신 이게 쓰임.
                {
                    base.OnLoad(e);
                    this.Form3_value = Passvalue; // 다른폼(Form1)에서 전달받은 값을 변수에 저장
                }*/

        public Form3(/*int usedcard_cnt*/)
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Passvalue_card_cnt = Convert.ToInt32(Math.Round(numericUpDown1.Value, 0));
            Passvalue_gain_cnt = Convert.ToInt32(Math.Round(numericUpDown2.Value, 0));
            this.Close();
        }

        private void Form3_Load(object sender, EventArgs e)
        {
            this.lastround_cardcnt = Passvalue_card_cnt;
            this.gained_cardcnt = Passvalue_gain_cnt;
        }
    }
}
