using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace PacoChip_8
{
    static class Screen
    {
        public enum ScreenMode
        {
            Chip8, SChip
        };
        public static ScreenMode Mode = ScreenMode.Chip8;

        public static void Clear()
        {
            Array.Clear(Memory.Screen, 0, Memory.Screen.Length);
        }

        public static void Draw()
        {
            CPU.PC -= 2;

            if (Globals.CanDraw == 1)
            {
                //Globals.startTime = DateTime.Now;

                CPU.V[0xF] = 0;

                int PosX = CPU.V[CPU.opcode2];
                int PosY = CPU.V[CPU.opcode3];

                if (Mode == ScreenMode.Chip8)
                {
                    PosX &= (Globals.RES_X >> 1) - 1;
                    PosY &= (Globals.RES_Y >> 1) - 1;
                }
                else
                {
                    PosX &= Globals.RES_X - 1;
                    PosY &= Globals.RES_Y - 1;
                }

                if (CPU.opcode4 > 0)   //Dibujar sprite normal
                {
                    if (Mode == ScreenMode.Chip8)
                    {
                        for (int h = 0; h < CPU.opcode4; h++)
                        {
                            for (int w = 0; w < 8; w++)
                            {
                                if (((Memory.RAM[CPU.I + h] << w) & 0x80) == 0x80)
                                {
                                    if ((PosY + h) << 1 < Globals.RES_Y && (PosX + w) << 1 < Globals.RES_X) //Comprobar que el pixel no se sale de la pantalla
                                    {
                                        if (Memory.Screen[(PosX + w) << 1, (PosY + h) << 1] == 0x01)
                                        {
                                            CPU.V[0xF] = 1;
                                        }

                                        Memory.Screen[(PosX + w) << 1, (PosY + h) << 1] ^= 1;
                                        Memory.Screen[((PosX + w) << 1) + 1, (PosY + h) << 1] ^= 1;
                                        Memory.Screen[(PosX + w) << 1, ((PosY + h) << 1) + 1] ^= 1;
                                        Memory.Screen[((PosX + w) << 1) + 1, ((PosY + h) << 1) + 1] ^= 1;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        for (int h = 0; h < CPU.opcode4; h++)
                        {
                            for (int w = 0; w < 8; w++)
                            {
                                if (((Memory.RAM[CPU.I + h] << w) & 0x80) == 0x80)
                                {
                                    if (PosY + h < Globals.RES_Y && PosX + w < Globals.RES_X) //Comprobar que el pixel no se sale de la pantalla
                                    {
                                        if (Memory.Screen[(PosX + w), (PosY + h)] == 0x01)
                                        {
                                            CPU.V[0xF] = 1;
                                        }

                                        Memory.Screen[(PosX + w), (PosY + h)] ^= 1;
                                    }
                                }
                            }
                        }
                    }
                }
                else   //Sprite 16x16 del SChip
                {
                    if (Screen.Mode == Screen.ScreenMode.SChip)
                    {
                        for (int i = 0; i < 16; i++)
                        {
                            int line = (Memory.RAM[CPU.I + (i << 1)] << 8) | Memory.RAM[(CPU.I + (i << 1)) + 1];

                            for (int j = 0; j < 16; j++)
                            {
                                if (((line << j) & 0x8000) == 0x8000)
                                {
                                    if (PosY + i < Globals.RES_Y && PosX + j < Globals.RES_X) //Comprobar que el pixel no se sale de la pantalla
                                    {
                                        if (Memory.Screen[(PosX + j), (PosY + i)] == 0x01)
                                        {
                                            CPU.V[0xF] = 1;
                                        }

                                        Memory.Screen[(PosX + j), (PosY + i)] ^= 1;
                                    }
                                }
                            }
                        }
                    }
                    else //En caso de intentar dibujar un sprite de 16x16 en modo Chip8, se dibuja de 8x16
                    {
                        for (int i = 0; i < 16; i++)
                        {
                            int line = (Memory.RAM[CPU.I + i]);

                            for (int j = 0; j < 8; j++)
                            {
                                if (((line << j) & 0x80) == 0x80)
                                {
                                    if ((PosY + i) << 1 < Globals.RES_Y && (PosX + j) << 1 < Globals.RES_X) //Comprobar que el pixel no se sale de la pantalla
                                    {
                                        if (Memory.Screen[(PosX + j) << 1, (PosY + i) << 1] == 0x01)
                                        {
                                            CPU.V[0xF] = 1;
                                        }

                                        Memory.Screen[(PosX + j) << 1, (PosY + i) << 1] ^= 1;
                                        Memory.Screen[((PosX + j) << 1) + 1, (PosY + i) << 1] ^= 1;
                                        Memory.Screen[(PosX + j) << 1, ((PosY + i) << 1) + 1] ^= 1;
                                        Memory.Screen[((PosX + j) << 1) + 1, ((PosY + i) << 1) + 1] ^= 1;
                                    }
                                }
                            }
                        }
                    }
                }

                //Copiar el array a la pantalla

                CPU.PC += 2;
            }
            Globals.CanDraw = 0;
        }

        public static void ChangeMode(ScreenMode m)
        {
            Clear();

            /*if (m == Chip8)
            {
                Globals.RES_X = 64;
                Globals.RES_Y = 32;
                screenArray = new int[Globals.RES_X, Globals.RES_Y];
            }
            else if (m == SChip)
            {
                Globals.RES_X = 128;
                Globals.RES_Y = 64;
                screenArray = new int[Globals.RES_X, Globals.RES_Y];
            }*/

            Mode = m;
        }
        /*
        public static void InvertColors(bool UpdateConfig = true)
        {
            paleta.Entries[2] = paleta.Entries[0];
            paleta.Entries[0] = paleta.Entries[1];
            paleta.Entries[1] = paleta.Entries[2];
            paleta.Entries[2] = Color.FromArgb(0, 0, 0, 0);

            bmp.Palette = paleta;

            if(UpdateConfig == true) Globals.InvertedColors = !Globals.InvertedColors;
        }*/

        /*
        void Screen.ShowOld()
        {
            Graphics g = pictureBox1.CreateGraphics();

            Bitmap bmp = new Bitmap(Globals.RES_X + 0, Globals.RES_Y + 0, PixelFormat.Format32bppArgb);
            Graphics gBmp = Graphics.FromImage(bmp);
            gBmp.CompositingMode = CompositingMode.SourceCopy;
            g.InterpolationMode = InterpolationMode.NearestNeighbor;

            for (int i = 0; i < Globals.RES_X; i++)
            {
                for (int j = 0; j < Globals.RES_Y; j++)
                {
                    if (Memory.Screen[i, j] == 1)
                        bmp.SetPixel(i + 0, j + 0, Color.FromArgb(255, 219, 209, 173));
                    else
                        bmp.SetPixel(i + 0, j + 0, Color.FromArgb(255, 114, 107, 85));
                }
            }

            //bmp.SetPixel(pp++, 20, Color.White);

            // draw the bitmap on our window
            g.DrawImage(bmp, 0, 0, pictureBox1.Width + 2, pictureBox1.Height + 2);

            if (Screenshot == true)
            {
                bmp.Save(Path.Combine(Application.StartupPath, "screenshot.png"), ImageFormat.Png);
                Screenshot = false;
            }

            bmp.Dispose();
            gBmp.Dispose();
        }*/

    }
}
