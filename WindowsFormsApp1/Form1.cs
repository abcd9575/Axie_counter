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
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click_1(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox8_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged_1(object sender, EventArgs e)
        {

        }

        private void label2_Click_1(object sender, EventArgs e)
        {

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }
        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {

        }
        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {

        }
        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {

        }
        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {

        }
        private void checkBox6_CheckedChanged(object sender, EventArgs e)
        {

        }
        private void checkBox7_CheckedChanged(object sender, EventArgs e)
        {

        }
        private void checkBox8_CheckedChanged(object sender, EventArgs e)
        {

        }
        private void checkBox9_CheckedChanged(object sender, EventArgs e)
        {

        }
        private void checkBox10_CheckedChanged(object sender, EventArgs e)
        {

        }
        private void checkBox11_CheckedChanged(object sender, EventArgs e)
        {

        }
        private void checkBox12_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void Button13_Click(object sender, EventArgs e)
        {
            card = 6;
            energy = 3;
            foreach (var but in button)
                button[but] = 2;
            에너지.Text = energy.ToString();
            카드갯수.Text = card.ToString();
            카드로테이션.Text = card.ToString();
            checkBox1.Checked = false; checkBox2.Checked = false; checkBox3.Checked = false; checkBox4.Checked = false;
            checkBox5.Checked = false; checkBox6.Checked = false; checkBox7.Checked = false; checkBox8.Checked = false;
            checkBox9.Checked = false; checkBox10.Checked = false; checkBox11.Checked = false; checkBox12.Checked = false;
            button1.Text = 엑시정보[0].in_deck.ToString(); button2.Text = 엑시정보[1].in_deck.ToString(); button3.Text = 엑시정보[2].in_deck.ToString();
            button4.Text = 엑시정보[3].in_deck.ToString(); button5.Text = 엑시정보[4].in_deck.ToString(); button6.Text = 엑시정보[5].in_deck.ToString();
            button7.Text = 엑시정보[6].in_deck.ToString(); button8.Text = 엑시정보[7].in_deck.ToString(); button9.Text = 엑시정보[8].in_deck.ToString();
            button10.Text = 엑시정보[9].in_deck.ToString(); button11.Text = 엑시정보[10].in_deck.ToString(); button12.Text = 엑시정보[11].in_deck.ToString();
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }
    }
}
