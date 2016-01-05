using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Festival_Support_GUI
{
    public partial class AboutForm : Form
    {
        public AboutForm()
        {
            InitializeComponent();
        }

        private void label4_Click(object sender, EventArgs e)
        {
            var l = (Label)sender;
            Process.Start("https://" + l.Text);
        }

        private void label5_Click(object sender, EventArgs e)
        {
            Process.Start("https://twitter.com/vainamov");
        }
    }
}
