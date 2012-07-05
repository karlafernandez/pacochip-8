using System;
using System.Windows.Forms;

namespace PacoChip_8
{
    public partial class AcercaDe : Form
    {
        public AcercaDe()
        {
            InitializeComponent();
        }

        private void AcercaDe_Load(object sender, EventArgs e)
        {
            Version vrs = new Version(Application.ProductVersion);
            label1.Text = "PacoChip 8 v" + vrs.Major + "." + vrs.Minor;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("mailto:maikelchan88@gmail.com?subject=PacoChip 8");
        }
    }
}
