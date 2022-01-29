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
    
    public partial class Form2 : Form
    {
        private int PosValue;
        private int LastRoundCardvalue;
        private int GainedCardvalue;
        public int Passposvalue
        {
            get { return this.PosValue; }
            set { this.PosValue = value;
                label1.Text = "Is axie placed in \'" + this.PosValue + "\' died?";
            }
        }
        public int PassLastround_CardCnt
        {
            get { return this.LastRoundCardvalue; }
            set { this.LastRoundCardvalue = value; }
        }
        public int Passgained_CardCnt
        {
            get { return this.GainedCardvalue; }
            set { this.GainedCardvalue = value; }
        }


        /*        protected override void OnLoad(EventArgs e) // Form1_Load 안쓰임. 대신 이게 쓰임.
                {
                    base.OnLoad(e);
                    this.PosValue = Passposvalue; // 다른폼(Form1)에서 전달받은 값을 변수에 저장
                    this.UsedCardvalue = PassUsedcardvalue;
                }*/


        int return_num = 0;
        int count = 0;

/*        public Form2(int position, int usedcard_cnt)
        {
            InitializeComponent();
            count = usedcard_cnt;
            label1.Text = "Is axie placed in \'" + position + "\' died?";
        }*/
        public Form2()
        {
            InitializeComponent();
            
        }


        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.No;
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Yes;
            Form3 popup = new Form3(/*count*/);
            popup.Passvalue_card_cnt = LastRoundCardvalue;
            popup.Passvalue_gain_cnt = GainedCardvalue;
            popup.ShowDialog();
            if (popup.DialogResult == DialogResult.No)
                return;
            PassLastround_CardCnt = popup.Passvalue_card_cnt;
            Passgained_CardCnt = popup.Passvalue_gain_cnt;
            this.Close();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            this.PosValue = Passposvalue; // 다른폼(Form1)에서 전달받은 값을 변수에 저장
            this.LastRoundCardvalue = PassLastround_CardCnt;
            this.GainedCardvalue = Passgained_CardCnt;
        }
    }
}
