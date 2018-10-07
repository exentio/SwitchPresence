using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace SwitchPresence
{
    public partial class UpdateForm : Form
    {
        public UpdateForm(string ver, string info)
        {
            InitializeComponent();
            label_update.Text += " ( " + ver +" ).";
            textBox1.Text = info;
        }

        private void button_ok_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://github.com/Random0666/SwitchRichPresence/releases");
        }
    }
}
