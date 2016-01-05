using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Festival_Support_GUI
{
    public partial class KeyboardInput : Form
    {
        public KeyboardInput()
        {
            InitializeComponent();
        }

        public string Keyboard
        {
            get
            {
                return textBox1.Text;
            }
            set
            {
                textBox1.Text = value;
            }
        }
    }
}
