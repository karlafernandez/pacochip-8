using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace PacoChip_8
{
    static class Input
    {
        public enum InputState
        {
            KeyUp, KeyDown, KeyHeld, KeyRelease
        };
        public static InputState[] KeyState = { InputState.KeyUp, InputState.KeyUp, InputState.KeyUp, InputState.KeyUp, InputState.KeyUp, InputState.KeyUp, InputState.KeyUp, InputState.KeyUp, InputState.KeyUp, InputState.KeyUp, InputState.KeyUp, InputState.KeyUp, InputState.KeyUp, InputState.KeyUp, InputState.KeyUp, InputState.KeyUp };

        public static bool KeyHeld = false;

        //static DateTime startTime = DateTime.Now;
        //static DateTime currentTime;

        static Stopwatch InputStopWatch = new Stopwatch();

        const int TECLA_1 = 0;
        const int TECLA_2 = 1;
        const int TECLA_3 = 2;
        const int TECLA_4 = 3;
        const int TECLA_Q = 4;
        const int TECLA_W = 5;
        const int TECLA_E = 6;
        const int TECLA_R = 7;
        const int TECLA_A = 8;
        const int TECLA_S = 9;
        const int TECLA_D = 10;
        const int TECLA_F = 11;
        const int TECLA_Z = 12;
        const int TECLA_X = 13;
        const int TECLA_C = 14;
        const int TECLA_V = 15;

        // mapeamos las 16 teclas de Chip8
        static private byte[] MapeoTeclas = 
        {	       
            0x01,0x02,0x03,0x0C,
            0x04,0x05,0x06,0x0D,
            0x07,0x08,0x09,0x0E,
            0x0A,0x00,0x0B,0x0F 
        };

        /*public static void keyHeld()
        {
            for (int i = 0; i < 16; i++)
                if (KeyState[i] == InputState.KeyDown)
                {
                    KeyState[i] = InputState.KeyHeld;
                }
        }*/

        public static void keyHeld()
        {
            if (KeyHeld == true)
            {
                if (InputStopWatch.Elapsed.TotalMilliseconds >= Globals.Hz * 0.5)
                {
                    for (int i = 0; i < 16; i++)
                    {
                        if (Input.KeyState[i] == Input.InputState.KeyDown)
                        {
                            Input.KeyState[i] = Input.InputState.KeyHeld;
                        }
                    }
                    KeyHeld = false;
                    InputStopWatch.Reset();
                }
            }
        }

        public static void KeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.D1 && KeyState[MapeoTeclas[TECLA_1]] == InputState.KeyUp)
                KeyState[MapeoTeclas[TECLA_1]] = InputState.KeyDown;

            if (e.KeyCode == Keys.D2 && KeyState[MapeoTeclas[TECLA_2]] == InputState.KeyUp)
                KeyState[MapeoTeclas[TECLA_2]] = InputState.KeyDown;

            if (e.KeyCode == Keys.D3 && KeyState[MapeoTeclas[TECLA_3]] == InputState.KeyUp)
                KeyState[MapeoTeclas[TECLA_3]] = InputState.KeyDown;

            if (e.KeyCode == Keys.D4 && KeyState[MapeoTeclas[TECLA_4]] == InputState.KeyUp)
                KeyState[MapeoTeclas[TECLA_4]] = InputState.KeyDown;

            if (e.KeyCode == Keys.Q && KeyState[MapeoTeclas[TECLA_Q]] == InputState.KeyUp)
                KeyState[MapeoTeclas[TECLA_Q]] = InputState.KeyDown;

            if (e.KeyCode == Keys.W && KeyState[MapeoTeclas[TECLA_W]] == InputState.KeyUp)
                KeyState[MapeoTeclas[TECLA_W]] = InputState.KeyDown;

            if (e.KeyCode == Keys.E && KeyState[MapeoTeclas[TECLA_E]] == InputState.KeyUp)
                KeyState[MapeoTeclas[TECLA_E]] = InputState.KeyDown;

            if (e.KeyCode == Keys.R && KeyState[MapeoTeclas[TECLA_R]] == InputState.KeyUp)
                KeyState[MapeoTeclas[TECLA_R]] = InputState.KeyDown;

            if (e.KeyCode == Keys.A && KeyState[MapeoTeclas[TECLA_A]] == InputState.KeyUp)
                KeyState[MapeoTeclas[TECLA_A]] = InputState.KeyDown;

            if (e.KeyCode == Keys.S && KeyState[MapeoTeclas[TECLA_S]] == InputState.KeyUp)
                KeyState[MapeoTeclas[TECLA_S]] = InputState.KeyDown;

            if (e.KeyCode == Keys.D && KeyState[MapeoTeclas[TECLA_D]] == InputState.KeyUp)
                KeyState[MapeoTeclas[TECLA_D]] = InputState.KeyDown;

            if (e.KeyCode == Keys.F && KeyState[MapeoTeclas[TECLA_F]] == InputState.KeyUp)
                KeyState[MapeoTeclas[TECLA_F]] = InputState.KeyDown;

            if (e.KeyCode == Keys.Z && KeyState[MapeoTeclas[TECLA_Z]] == InputState.KeyUp)
                KeyState[MapeoTeclas[TECLA_Z]] = InputState.KeyDown;

            if (e.KeyCode == Keys.X && KeyState[MapeoTeclas[TECLA_X]] == InputState.KeyUp)
                KeyState[MapeoTeclas[TECLA_X]] = InputState.KeyDown;

            if (e.KeyCode == Keys.C && KeyState[MapeoTeclas[TECLA_C]] == InputState.KeyUp)
                KeyState[MapeoTeclas[TECLA_C]] = InputState.KeyDown;

            if (e.KeyCode == Keys.V && KeyState[MapeoTeclas[TECLA_V]] == InputState.KeyUp)
                KeyState[MapeoTeclas[TECLA_V]] = InputState.KeyDown;

            if (KeyHeld == false)
            {
                InputStopWatch.Start();
                KeyHeld = true;
            }
        }

        public static void KeyUp(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.D1)
                KeyState[MapeoTeclas[TECLA_1]] = InputState.KeyUp;

            if (e.KeyCode == Keys.D2)
                KeyState[MapeoTeclas[TECLA_2]] = InputState.KeyUp;

            if (e.KeyCode == Keys.D3)
                KeyState[MapeoTeclas[TECLA_3]] = InputState.KeyUp;

            if (e.KeyCode == Keys.D4)
                KeyState[MapeoTeclas[TECLA_4]] = InputState.KeyUp;

            if (e.KeyCode == Keys.Q)
                KeyState[MapeoTeclas[TECLA_Q]] = InputState.KeyUp;

            if (e.KeyCode == Keys.W)
                KeyState[MapeoTeclas[TECLA_W]] = InputState.KeyUp;

            if (e.KeyCode == Keys.E)
                KeyState[MapeoTeclas[TECLA_E]] = InputState.KeyUp;

            if (e.KeyCode == Keys.R)
                KeyState[MapeoTeclas[TECLA_R]] = InputState.KeyUp;

            if (e.KeyCode == Keys.A)
                KeyState[MapeoTeclas[TECLA_A]] = InputState.KeyUp;

            if (e.KeyCode == Keys.S)
                KeyState[MapeoTeclas[TECLA_S]] = InputState.KeyUp;

            if (e.KeyCode == Keys.D)
                KeyState[MapeoTeclas[TECLA_D]] = InputState.KeyUp;

            if (e.KeyCode == Keys.F)
                KeyState[MapeoTeclas[TECLA_F]] = InputState.KeyUp;

            if (e.KeyCode == Keys.Z)
                KeyState[MapeoTeclas[TECLA_Z]] = InputState.KeyUp;

            if (e.KeyCode == Keys.X)
                KeyState[MapeoTeclas[TECLA_X]] = InputState.KeyUp;

            if (e.KeyCode == Keys.C)
                KeyState[MapeoTeclas[TECLA_C]] = InputState.KeyUp;

            if (e.KeyCode == Keys.V)
                KeyState[MapeoTeclas[TECLA_V]] = InputState.KeyUp;
        }
    }
}
