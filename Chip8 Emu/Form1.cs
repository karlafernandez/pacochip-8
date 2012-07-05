using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;

namespace PacoChip_8
{
    public partial class Form1 : Form
    {
        #region Draw Screen

        Graphics g;
        Bitmap bmp;
        Stream LogoStream;
        Bitmap Logo;

        Stopwatch ScreenStopWatch = new Stopwatch();

        void InitializeScreen()
        {
            bmp = new Bitmap(Globals.RES_X, Globals.RES_Y, PixelFormat.Format32bppArgb);

            g = Graphics.FromImage(bmp);
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.None;
            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixel;

            LogoStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("PacoChip_8.Logo.png");
            Logo = new Bitmap(LogoStream);
            pictureBox1.Image = Logo;
            pictureBox1.Refresh();
        }

        void PrintGraphics()
        {
            SolidBrush brush;

            if (Globals.InvertedColors == true)
            {
                brush = new SolidBrush(Color.FromArgb(255, 114, 107, 85));
                g.Clear(Color.FromArgb(255, 219, 209, 173));
            }
            else
            {
                brush = new SolidBrush(Color.FromArgb(255, 219, 209, 173));
                g.Clear(Color.FromArgb(255, 114, 107, 85));
            }

            for (int y = 0; y < Globals.RES_Y; y++)
            {
                for (int x = 0; x < Globals.RES_X; x++)
                {
                    if (Memory.Screen[x, y] == 1)
                    {
                        g.FillRectangle(brush, new Rectangle(x, y, 1, 1));
                    }
                }
            }
        }

        void UpdateScreen()
        {
            //Globals.startTime = DateTime.Now;
            ScreenStopWatch.Start();

            while (Globals.Stop == false && Globals.EmulationState == Globals.Emulation_State.Running)
            {
                //Globals.currentTime = DateTime.Now;

                if (ScreenStopWatch.Elapsed.TotalMilliseconds >= 1000 / 60.0)
                {
                    //Globals.startTime = DateTime.Now;

                    ScreenStopWatch.Reset();
                    ScreenStopWatch.Start();

                    PrintGraphics();
                    pictureBox1.Invoke(new Action(() =>
                    {
                        if (pictureBox1.Image != null)
                        {
                            pictureBox1.Image.Dispose();
                        }

                        pictureBox1.Image = ResizeImage(bmp, 4);
                        pictureBox1.Refresh();
                    }));

                    DebugPanel();
#if DEBUG
                    DebugOutput();
#endif
                }
                Thread.Sleep(0);
            }
            ScreenStopWatch.Reset();
        }

        Bitmap ResizeImage(Bitmap Img, int mult)
        {
            Bitmap bmp = new Bitmap(Img.Width * mult, Img.Height * mult);
            Graphics g = Graphics.FromImage(bmp);

            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            g.DrawImage(Img, 0, 0, bmp.Width + 2, bmp.Height + 2);
            g.Dispose();

            return bmp;
        }

        #endregion

        #region Funcionamiento

        void Initialize()
        {
            Globals.LoadConfig();

            Version vrs = new Version(Application.ProductVersion);
            this.Text = "PacoChip 8 v" + vrs.Major + "." + vrs.Minor;

            listBox1.Items.Clear();
            for (int i = 0; i < Globals.TAMANO_PILA; i++) listBox1.Items.Add("0x" + CPU.Stack[i].ToString("X4"));

            InitializeScreen();

            invertirColoresToolStripMenuItem.Checked = Globals.InvertedColors;

            UpdateForm();
        }

        void OpenROM()
        {
            StopEmulator();
            ODlg.FileName = "";
            ODlg.Filter = "All files (*.*)|*.*";
            if (ODlg.ShowDialog() != DialogResult.OK) return;
            Globals.ROMPath = ODlg.FileName;

            toolStripButton1.Enabled = true;
            iniciarToolStripMenuItem.Enabled = true;

            RunEmulator();
        }

        void RunEmulator()
        {
            if (Globals.EmulationState == Globals.Emulation_State.Stopped)
            {
                Globals.Stop = false;
                Globals.EmulationState = Globals.Emulation_State.Running;
                
                if (Logo != null) Logo.Dispose();

                CPU.ResetHardware();
                EmulatorRun.RunWorkerAsync();
                ScreenUpdate.RunWorkerAsync();
            }
            else if (Globals.EmulationState == Globals.Emulation_State.Paused)
            {
                Globals.EmulationState = Globals.Emulation_State.Running;

                EmulatorRun.RunWorkerAsync();
                ScreenUpdate.RunWorkerAsync();
            }
            UpdateButtons();
        }

        void PauseEmulator()
        {
            if (Globals.EmulationState == Globals.Emulation_State.Running)
            {
                Globals.EmulationState = Globals.Emulation_State.Paused;
            }
            else if (Globals.EmulationState == Globals.Emulation_State.Paused)
            {
                Globals.EmulationState = Globals.Emulation_State.Running;
                EmulatorRun.RunWorkerAsync();
                ScreenUpdate.RunWorkerAsync();
            }
            UpdateButtons();
        }

        void StopEmulator()
        {
            if (Globals.EmulationState != Globals.Emulation_State.Stopped)
            {
                Globals.Stop = true;
                Screen.Clear();

                Globals.EmulationState = Globals.Emulation_State.Stopped;
                UpdateButtons();
            }
        }

        void UpdateButtons()
        {
            if (Globals.EmulationState == Globals.Emulation_State.Stopped || Globals.Stop == true)
            {
                toolStripButton1.Enabled = true;
                toolStripButton4.Enabled = false;
                toolStripButton2.Enabled = false;

                iniciarToolStripMenuItem.Enabled = true;
                pauseToolStripMenuItem.Enabled = false;
                detenerToolStripMenuItem.Enabled = false;
            }
            else if (Globals.EmulationState == Globals.Emulation_State.Paused && Globals.Stop == false)
            {
                toolStripButton1.Enabled = true;
                toolStripButton4.Enabled = true;
                toolStripButton2.Enabled = true;

                iniciarToolStripMenuItem.Enabled = true;
                pauseToolStripMenuItem.Enabled = true;
                detenerToolStripMenuItem.Enabled = true;
            }
            else
            {
                toolStripButton1.Enabled = false;
                toolStripButton4.Enabled = true;
                toolStripButton2.Enabled = true;

                iniciarToolStripMenuItem.Enabled = false;
                pauseToolStripMenuItem.Enabled = true;
                detenerToolStripMenuItem.Enabled = true;
            }
        }

        void UpdateForm()
        {
            if (Globals.Debug == true)
            {
                panel1.Visible = true;
                panel2.Width = this.ClientSize.Width - panel1.Width;
            }
            else
            {
                panel1.Visible = false;
                panel2.Width = this.ClientSize.Width;
            }

            if (panel2.Width >> 1 <= panel2.Height)
            {
                pictureBox1.Width = panel2.Width;
                pictureBox1.Height = pictureBox1.Width >> 1;
            }
            else
            {
                pictureBox1.Height = panel2.Height;
                pictureBox1.Width = pictureBox1.Height << 1;
            }

            pictureBox1.Top = (panel2.Height >> 1) - (pictureBox1.Height >> 1);
            pictureBox1.Left = (panel2.Width >> 1) - (pictureBox1.Width >> 1);
        }

        void DebugPanel()
        {
            if (Globals.Debug == true)
            {
                textBox1.Invoke(new Action(() => { textBox1.Text = "0x" + CPU.V[0x0].ToString("X2"); }));
                textBox2.Invoke(new Action(() => { textBox2.Text = "0x" + CPU.V[0x1].ToString("X2"); }));
                textBox3.Invoke(new Action(() => { textBox3.Text = "0x" + CPU.V[0x2].ToString("X2"); }));
                textBox4.Invoke(new Action(() => { textBox4.Text = "0x" + CPU.V[0x3].ToString("X2"); }));
                textBox5.Invoke(new Action(() => { textBox5.Text = "0x" + CPU.V[0x4].ToString("X2"); }));
                textBox6.Invoke(new Action(() => { textBox6.Text = "0x" + CPU.V[0x5].ToString("X2"); }));
                textBox7.Invoke(new Action(() => { textBox7.Text = "0x" + CPU.V[0x6].ToString("X2"); }));
                textBox8.Invoke(new Action(() => { textBox8.Text = "0x" + CPU.V[0x7].ToString("X2"); }));
                textBox9.Invoke(new Action(() => { textBox9.Text = "0x" + CPU.V[0x8].ToString("X2"); }));
                textBox10.Invoke(new Action(() => { textBox10.Text = "0x" + CPU.V[0x9].ToString("X2"); }));
                textBox11.Invoke(new Action(() => { textBox11.Text = "0x" + CPU.V[0xa].ToString("X2"); }));
                textBox12.Invoke(new Action(() => { textBox12.Text = "0x" + CPU.V[0xb].ToString("X2"); }));
                textBox13.Invoke(new Action(() => { textBox13.Text = "0x" + CPU.V[0xc].ToString("X2"); }));
                textBox14.Invoke(new Action(() => { textBox14.Text = "0x" + CPU.V[0xd].ToString("X2"); }));
                textBox15.Invoke(new Action(() => { textBox15.Text = "0x" + CPU.V[0xe].ToString("X2"); }));
                textBox16.Invoke(new Action(() => { textBox16.Text = "0x" + CPU.V[0xf].ToString("X2"); }));

                listBox1.Invoke(new Action(() =>
                {
                    for (int i = 0; i < Globals.TAMANO_PILA; i++) listBox1.Items[i] = "0x" + CPU.Stack[i].ToString("X4");
                }));

                textBox17.Invoke(new Action(() => { textBox17.Text = "0x" + CPU.SP.ToString("X2"); }));
                textBox18.Invoke(new Action(() => { textBox18.Text = "0x" + Globals.delayTimer.ToString("X2"); }));
                textBox19.Invoke(new Action(() => { textBox19.Text = "0x" + Sound.Timer.ToString("X2"); }));
                textBox20.Invoke(new Action(() => { textBox20.Text = "0x" + CPU.PC.ToString("X4"); }));
            }
        }

#if DEBUG
        void DebugOutput()
        {
            Debug.WriteLine(Input.KeyState[0]);
        }
#endif

        void InvertColors()
        {
            Globals.InvertedColors = !Globals.InvertedColors;
        }

        #endregion

        #region Form Events

        public Form1()
        {
            InitializeComponent();

            this.KeyPreview = true;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Initialize();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Globals.SaveConfig();

            Globals.Stop = true;
            g.Dispose();
            bmp.Dispose();
            Logo.Dispose();
            LogoStream.Dispose();
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            PacoChip_8.AcercaDe About = new PacoChip_8.AcercaDe();
            About.ShowDialog();
        }

        private void debugToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Globals.Debug = debugToolStripMenuItem.Checked;
            UpdateForm();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Memory.DumpRAM(Path.Combine(Application.StartupPath, "ram.bin"));
        }

        private void button2_Click(object sender, EventArgs e)
        {
            bmp.Save(Path.Combine(Application.StartupPath, "screenshot.png"), ImageFormat.Png);
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            UpdateForm();
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            Input.KeyDown(e);
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            Input.KeyUp(e);
        }

        private void salirToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            if (trackBar1.Value == 8)
                Globals.Hz = 1000 / 240.0;
            else if (trackBar1.Value == 7)
                Globals.Hz = 1000 / 120.0;
            else if (trackBar1.Value == 6)
                Globals.Hz = 1000 / 60.0;
            else if (trackBar1.Value == 5)
                Globals.Hz = 1000 / 30.0;
            else if (trackBar1.Value == 4)
                Globals.Hz = 1000 / 15.0;
            else if (trackBar1.Value == 3)
                Globals.Hz = 1000 / 7.5;
            else if (trackBar1.Value == 2)
                Globals.Hz = 1000 / 3.75;
            else if (trackBar1.Value == 1)
                Globals.Hz = 1000 / 1.875;
            else if (trackBar1.Value == 0)
                Globals.Hz = 1000 / 0.9375;

            label17.Text = "Speed (" + 1000 / Globals.Hz + "fps)";
        }

        private void abrirROMToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            OpenROM();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            StopEmulator();
        }

        private void detenerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StopEmulator();
        }

        private void iniciarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RunEmulator();
        }

        private void pauseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PauseEmulator();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            RunEmulator();
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            PauseEmulator();
        }

        private void ScreenUpdate_DoWork(object sender, DoWorkEventArgs e)
        {
            UpdateScreen();
        }

        private void EmulatorRun_DoWork(object sender, DoWorkEventArgs e)
        {
            CPU.Run();
        }

        private void EmulatorRun_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (Globals.Stop) Globals.EmulationState = Globals.Emulation_State.Stopped;
            UpdateButtons();
        }

        private void soundToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Sound.Enabled = soundToolStripMenuItem.Checked;
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Settings settings = new Settings();
            settings.ShowDialog();
        }

        private void invertirColoresToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InvertColors();
            invertirColoresToolStripMenuItem.Checked = Globals.InvertedColors;
        }

        #endregion

    }
}
