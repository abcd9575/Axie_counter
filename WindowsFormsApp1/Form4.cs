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

    public partial class Form4 : Form
    {
        public bool 로그인성공;
        public string LoginID
        {
            get => LoginBox.Text.Trim();
            set => LoginBox.Text = value;
        }
        public string LoginPW
        {
            get => PwBox.Text;
            set => PwBox.Text = value;
        }

        public Form4()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            로그인성공 = true;
            if (로그인성공)
                this.Close();

        }

        private void Form4_Load(object sender, EventArgs e)
        {

        }

    }
}
