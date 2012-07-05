using System;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;

namespace PacoChip_8
{
    static class Globals
    {
        public static string ROMPath = String.Empty;

        public enum Emulation_State
        {
            Stopped, Running, Paused
        };
        public static Emulation_State EmulationState = Emulation_State.Stopped;

        public static bool Stop = false;
        //public static bool Reset = false;
        //public static bool IsRunning = false;
        public static bool Debug = false;

        public static bool AlternativeFx55Opcode = false;
        public static bool Alternative8xy6Opcode = false;

        public static bool InvertedColors = false;

        public static bool Screenshot = false;
        public static double Hz = 1000 / 60.0;

        //variables principales        
        public const int RES_X = 128;
        public const int RES_Y = 64;
        public const int DIR_INICIO = 0x200;
        public const int TAMANO_MEM = 0x1000;
        public const int TAMANO_PILA = 16;
        public const int CANT_REGISTROS = 16;

        //public static int ScreenWidth;
        //public static int ScreenHeight;

        public static int delayTimer;

        public static Random random;

        //public static DateTime startTime, currentTime;
        public static Stopwatch CPUStopWatch = new Stopwatch();

        public static int CanDraw = 0;

        #region Config

        public static string ConfigPath = Path.Combine(Application.StartupPath, "config");

        public static void SaveConfig()
        {
            FileStream fs = new FileStream(ConfigPath, FileMode.Create, FileAccess.Write);
            BinaryWriter bw = new BinaryWriter(fs);

            try
            {
                bw.Write(AlternativeFx55Opcode);
                bw.Write(Alternative8xy6Opcode);
                bw.Write(InvertedColors);
            }
            catch (Exception e)
            {
                MessageBox.Show("Error while saving config file. " + e.Message);
            }
            finally
            {
                bw.Close();
                fs.Close();
            }
        }

        public static void LoadConfig()
        {
            if (File.Exists(ConfigPath) == false) return;

            FileStream fs = new FileStream(ConfigPath, FileMode.Open, FileAccess.Read);
            BinaryReader br = new BinaryReader(fs);

            try
            {
                AlternativeFx55Opcode = br.ReadBoolean();
                Alternative8xy6Opcode = br.ReadBoolean();
                InvertedColors = br.ReadBoolean();
            }
            catch (Exception e)
            {
                MessageBox.Show("Error while loading config file. " + e.Message);
            }
            finally
            {
                br.Close();
                fs.Close();
            }
        }

        #endregion
    }

    static class Memory
    {
        public static int[,] Screen = new int[Globals.RES_X, Globals.RES_Y];
        public static byte[] RAM = new byte[Globals.TAMANO_MEM];

        //variables para el manejo de fuentes (80 bytes, ya que hay 5 bytes x caracter 
        //y son 16 caracteres o letras (5x16=80). Cada font es de 4x5 bits.
        public static byte[] FontChip8 = {
		   0xF0, 0x90, 0x90, 0x90, 0xF0,	// valores para 0
		   0x20, 0x60, 0x20, 0x20, 0x70,	// valores para 1
		   0x60, 0x90, 0x20, 0x40, 0xF0,	// valores para 2
		   0xF0, 0x10, 0xF0, 0x10, 0xF0,	// valores para 3
		   0x90, 0x90, 0xF0, 0x10, 0x10,	// valores para 4
		   0xF0, 0x80, 0x60, 0x10, 0xE0,	// valores para 5
		   0xF0, 0x80, 0xF0, 0x90, 0xF0,	// valores para 6
		   0xF0, 0x10, 0x10, 0x10, 0x10,	// valores para 7
		   0xF0, 0x90, 0xF0, 0x90, 0xF0,	// valores para 8
		   0xF0, 0x90, 0xF0, 0x10, 0x10,	// valores para 9
		   0x60, 0x90, 0xF0, 0x90, 0x90,	// valores para A
		   0xE0, 0x90, 0xE0, 0x90, 0xE0,	// valores para B
		   0x70, 0x80, 0x80, 0x80, 0x70,	// valores para C
		   0xE0, 0x90, 0x90, 0x90, 0xE0, 	// valores para D
		   0xF0, 0x80, 0xF0, 0x80, 0xF0,	// valores para E
		   0xF0, 0x80, 0xF0, 0x80, 0x80		// valores para F
		};

        public static byte[] FontSChip = {   //SChip
		    0x18, 0x3c, 0x66, 0x66, 0x66, 0x66, 0x66, 0x66, 0x3c, 0x18,
		    0x0c, 0x1c, 0x3c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x3e, 0x3e,
            0x3c, 0x7e, 0x66, 0x06, 0x0e, 0x1c, 0x38, 0x70, 0x7e, 0x7e,
            0x3c, 0x7e, 0x66, 0x06, 0x1c, 0x1e, 0x06, 0x66, 0x7e, 0x3c,
            0x0c, 0x1c, 0x1c, 0x3c, 0x2c, 0x6e, 0x7e, 0x0c, 0x0c, 0x1e,
            0x7E, 0x7E, 0x60, 0x60, 0x7c, 0x3E, 0x06, 0x66, 0x7C, 0x38,
            0x1c, 0x3c, 0x70, 0x60, 0x7C, 0x66, 0x66, 0x66, 0x3C, 0x18,
            0x7E, 0x7E, 0x06, 0x06, 0x0c, 0x1c, 0x38, 0x30, 0x30, 0x30,
            0x18, 0x3c, 0x66, 0x66, 0x3c, 0x7E, 0x66, 0x66, 0x7e, 0x3C,
            0x3c, 0x7E, 0x66, 0x66, 0x66, 0x3E, 0x06, 0x0E, 0x3C, 0x38,
            0x18, 0x18, 0x3c, 0x24, 0x24, 0x66, 0x7E, 0x66, 0x66, 0x66,
            0x7C, 0x7E, 0x66, 0x66, 0x7C, 0x7E, 0x66, 0x66, 0x7E, 0x7C,
            0x1c, 0x3E, 0x76, 0x60, 0x60, 0x60, 0x60, 0x76, 0x3E, 0x1C,
            0x7C, 0x7E, 0x66, 0x66, 0x66, 0x66, 0x66, 0x66, 0x7E, 0x7C,
            0x7E, 0x7E, 0x60, 0x60, 0x7C, 0x7C, 0x60, 0x60, 0x7e, 0x7e,
            0x7E, 0x7E, 0x60, 0x60, 0x7C, 0x7C, 0x60, 0x60, 0x60, 0x60
        };

        public static void DumpRAM(string OutputPath)
        {
            FileStream ram = new FileStream(OutputPath, FileMode.Create, FileAccess.Write);
            ram.Write(Memory.RAM, 0, Memory.RAM.Length);
            ram.Close();
        }
    }

    static class ROM
    {
        public static bool Load(string ROM)
        {
            string nombreRom = ROM;
            FileStream rom;

            try
            {
                rom = new FileStream(nombreRom, FileMode.Open, FileAccess.Read);

                if (rom.Length == 0)
                {
                    MessageBox.Show("Error: This is not a valid ROM.");
                    return false;
                }

                // Comenzamos a cargar la rom a la memoria a partir de la dir 0x200
                rom.Read(Memory.RAM, Globals.DIR_INICIO, (int)rom.Length);

                rom.Close();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while loading the ROM. " + ex.Message);
                return false;
            }
        }
    }
}
