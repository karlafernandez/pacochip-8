using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PacoChip_8
{
    public partial class Settings : Form
    {
        public Settings()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Globals.AlternativeFx55Opcode = checkBox1.Checked;
            Globals.Alternative8xy6Opcode = checkBox2.Checked;

            this.Dispose();
        }

        private void Settings_Load(object sender, EventArgs e)
        {
            checkBox1.Checked = Globals.AlternativeFx55Opcode;
            checkBox2.Checked = Globals.Alternative8xy6Opcode;
        }
    }
}
