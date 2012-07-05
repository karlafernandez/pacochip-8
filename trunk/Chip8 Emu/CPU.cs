using System;
using System.Threading;
using System.Windows.Forms;

namespace PacoChip_8
{
    static class CPU
    {
        public static int[] V = new int[Globals.CANT_REGISTROS];
        public static int[] HP48 = new int[8];
        public static int[] Stack = new int[Globals.TAMANO_PILA];

        //variables que representan registros varios
        public static int PC;
        public static int I;
        public static int SP;
        public static int KK;

        //variables para manejar los opcodes
        public static int opcode1 = 0;
        public static int opcode2 = 0;  //X
        public static int opcode3 = 0;  //Y
        public static int opcode4 = 0;

        static int instruccion;  //representa una instruccion del Chip-8. Tiene largo 2 byte

        public static void ProcessOpcodes()
        {
            #region lectura de instrucciones

            // leemos cada una de las instrucciones desde la Memory.RAM. 
            // cada instruccion es de 2 bytes
            instruccion = Memory.RAM[PC] << 8 | Memory.RAM[PC + 1];

            // dejamos incrementado el Program Counter en 2, lista para leer 
            // la siguiente instruccion en el siguiente ciclo.
            PC += 2;

            #endregion

            #region extracción de opcodes

            //obtengo el valor del registro KK, de largo 1 byte, el más chico de la instrucción
            KK = (instruccion & 0x00FF);

            // cada opcode es de 4 bit
            opcode1 = (instruccion & 0xF000) >> 12; //los 4 bits mayores de la instrucción
            opcode2 = (instruccion & 0x0F00) >> 8;  //X
            opcode3 = (instruccion & 0x00F0) >> 4;  //Y
            opcode4 = (instruccion & 0x000F);  //Opcode N = los 4 bits menores de la instrucción

            #endregion

            #region Ejecución de instrucciones

            /*if (PC == 0x4a6)
            {
                I = I;
            }*/

            // Ejecutamos las instrucciones a traves de los opcodes
            switch (opcode1)
            {
                // opcodes del tipo 0xxx
                case (0x0):
                    {
                        switch (instruccion)
                        {
                            case (0x00E0):
                                {
                                    //opcode 00E0: Clear Screen.

                                    Screen.Clear();
                                    break;
                                }
                            case (0x00EE):
                                {
                                    // opcode 00EE: Return From Subroutine.

                                    SP--;
                                    PC = Stack[SP];
                                    break;
                                }

                            case (0x00fb):
                                {
                                    //Scroll 4 pixels right

                                    if (Globals.CanDraw == 1)
                                    {
                                        for (int y = 0; y < Globals.RES_Y; y++)
                                        {
                                            for (int x = Globals.RES_X - 4 - 1; x >= 0; x--)
                                            {
                                                Memory.Screen[x + 4, y] = Memory.Screen[x, y];
                                            }

                                            for (int x = 0; x < 4; x++)
                                                Memory.Screen[x, y] = 0;
                                        }

                                        Globals.CanDraw = 0;
                                    }
                                    else
                                        PC -= 2;
                                    break;
                                }
                            case (0x00fc):
                                {
                                    //Scroll 4 pixels left

                                    if (Globals.CanDraw == 1)
                                    {
                                        for (int y = 0; y < Globals.RES_Y; y++)
                                        {
                                            for (int x = 4; x < Globals.RES_X; x++)
                                            {
                                                Memory.Screen[x - 4, y] = Memory.Screen[x, y];
                                            }

                                            for (int x = Globals.RES_X - 4; x < Globals.RES_X; x++)
                                                Memory.Screen[x, y] = 0;
                                        }

                                        Globals.CanDraw = 0;
                                    }
                                    else
                                        PC -= 2;
                                    break;
                                }
                            case (0x00fd):
                                {
                                    //MessageBox.Show("The program has finished. Emulation will now be stopped.");
                                    Globals.Stop = true;
                                    break;
                                }
                            case (0x00fe):
                                {
                                    Screen.ChangeMode(Screen.ScreenMode.Chip8);
                                    break;
                                }
                            case (0x00ff):
                                {
                                    Screen.ChangeMode(Screen.ScreenMode.SChip);
                                    break;
                                }
                            default:

                                if (opcode3 == 0xc)
                                {
                                    //0x00Cx: Scroll the screen down x lines

                                    if (Globals.CanDraw == 1)
                                    {
                                        for (int i = 0; i < Globals.RES_X; i++)
                                        {
                                            for (int j = Globals.RES_Y - opcode4 - 1; j >= 0; j--)
                                            {
                                                Memory.Screen[i, j + opcode4] = Memory.Screen[i, j];
                                            }

                                            for (int j = 0; j < opcode4; j++)
                                                Memory.Screen[i, j] = 0;
                                        }

                                        Globals.CanDraw = 0;
                                    }
                                    else
                                        PC -= 2;
                                }
                                else if (opcode2 != 0)
                                {
                                    //0nnn - SYS addr
                                    //Jump to a machine code routine at nnn.
                                    //This instruction is only used on the old computers on which Chip-8 was originally implemented. It is ignored by modern interpreters.
                                }
                                else
                                    MessageBox.Show("Unknown Opcode: 0x" + Convert.ToString(instruccion, 16) + " at 0x" + Convert.ToString(PC - 0x202, 16));

                                break;
                        }

                        break;
                    }

                case (0x1):
                    {
                        //1nnn - JP addr
                        //Jump to location nnn.
                        //The interpreter sets the program counter to nnn.

                        PC = instruccion & 0xFFF;
                        break;
                    }

                case (0x2):
                    {
                        //2nnn - CALL addr
                        //Call subroutine at nnn.
                        //The interpreter increments the Stack pointer, then puts the current PC on the top of the Stack. The PC is then set to nnn.

                        Stack[SP] = PC;
                        SP++;
                        PC = instruccion & 0xFFF;
                        break;
                    }

                case (0x3):
                    {
                        //3xkk - SE Vx, byte
                        //Skip next instruction if Vx = kk.
                        //The interpreter compares register Vx to kk, and if they are equal, increments the program counter by 2.

                        if (V[opcode2] == (instruccion & 0xFF)) PC += 2;
                        break;
                    }

                case (0x4):
                    {
                        //4xkk - SNE Vx, byte
                        //Skip next instruction if Vx != kk.
                        //The interpreter compares register Vx to kk, and if they are not equal, increments the program counter by 2.

                        if (V[opcode2] != (instruccion & 0xFF)) PC += 2;
                        break;
                    }

                case (0x5):
                    {
                        //5xy0 - SE Vx, Vy
                        //Skip next instruction if Vx = Vy.
                        //The interpreter compares register Vx to register Vy, and if they are equal, increments the program counter by 2.

                        if (V[opcode2] == V[opcode3]) PC += 2;
                        break;
                    }

                case (0x6):
                    {
                        //6xkk - LD Vx, byte
                        //Set Vx = kk.
                        //The interpreter puts the value kk into register Vx.

                        V[opcode2] = (instruccion & 0xFF);
                        break;
                    }

                case (0x7):
                    {
                        //7xkk - ADD Vx, byte
                        //Set Vx = Vx + kk.
                        //Adds the value kk to the value of register Vx, then stores the result in Vx. 

                        V[opcode2] = (V[opcode2] + instruccion) & 0xFF;
                        break;
                    }

                case (0x8):
                    {
                        switch (opcode4)
                        {
                            case (0x0):
                                {
                                    //8xy0 - LD Vx, Vy
                                    //Set Vx = Vy.
                                    //Stores the value of register Vy in register Vx.

                                    V[opcode2] = V[opcode3];
                                    break;
                                }

                            case (0x1):
                                {
                                    //8xy1 - OR Vx, Vy
                                    //Set Vx = Vx OR Vy.
                                    //Performs a bitwise OR on the values of Vx and Vy, then stores the result in Vx.
                                    //A bitwise OR compares the corrseponding bits from two values, and if either bit is 1, then the same bit in the result is also 1. Otherwise, it is 0.

                                    V[opcode2] |= V[opcode3];
                                    break;
                                }

                            case (0x2):
                                {
                                    //8xy2 - AND Vx, Vy
                                    //Vx = Vx AND Vy.
                                    //Performs a bitwise AND on the values of Vx and Vy, then stores the result in Vx.
                                    //A bitwise AND compares the corrseponding bits from two values, and if both bits are 1, then the same bit in the result is also 1. Otherwise, it is 0. 

                                    V[opcode2] &= V[opcode3];
                                    break;
                                }

                            case (0x3):
                                {
                                    //8xy3 - XOR Vx, Vy
                                    //Set Vx = Vx XOR Vy.
                                    //Performs a bitwise exclusive OR on the values of Vx and Vy, then stores the result in Vx.
                                    //An exclusive OR compares the corrseponding bits from two values, and if the bits are not both the same, then the corresponding bit in the result is set to 1. Otherwise, it is 0. 

                                    V[opcode2] ^= V[opcode3];
                                    break;
                                }

                            case (0x4):
                                {
                                    //8xy4 - ADD Vx, Vy
                                    //Set Vx = Vx + Vy, set VF = carry.
                                    //The values of Vx and Vy are added together. If the result is greater than 8 bits (i.e., > 255,) VF is set to 1, otherwise 0.
                                    //Only the lowest 8 bits of the result are kept, and stored in Vx.

                                    V[opcode2] += V[opcode3];

                                    if (V[opcode2] > 0xFF)
                                        V[0xF] = 1;
                                    else
                                        V[0xF] = 0;

                                    V[opcode2] &= 0xFF;

                                    break;
                                }

                            case (0x5):
                                {
                                    //8xy5 - SUB Vx, Vy
                                    //Set Vx = Vx - Vy, set VF = NOT borrow.
                                    //If Vx > Vy, then VF is set to 1, otherwise 0. Then Vy is subtracted from Vx, and the results stored in Vx.

                                    if (V[opcode2] >= V[opcode3])
                                        V[0xF] = 1;
                                    else
                                        V[0xF] = 0;

                                    V[opcode2] = (V[opcode2] - V[opcode3]) & 0xFF;

                                    break;
                                }

                            case (0x6):
                                {
                                    //8xy6 - SHR Vx {, Vy}
                                    //Set Vx = Vx SHR 1.      -------------------------------------------------------------------------------- ERRONEO
                                    //If the least-significant bit of Vx is 1, then VF is set to 1, otherwise 0. Then Vx is divided by 2.

                                    //Store the value of register VY shifted right one bit in register VX
                                    //Set register VF to the least significant bit prior to the shift

                                    if (Globals.Alternative8xy6Opcode == true && Screen.Mode == Screen.ScreenMode.Chip8)
                                    {
                                        V[0xF] = V[opcode3] & 1;
                                        V[opcode2] = (V[opcode3] >> 1) & 0xFF;
                                    }
                                    else
                                    {
                                        V[0xF] = V[opcode2] & 1;
                                        V[opcode2] = (V[opcode2] >> 1) & 0xFF;
                                    }

                                    break;
                                }

                            case (0x7):
                                {
                                    //8xy7 - SUBN Vx, Vy
                                    //Set Vx = Vy - Vx, set VF = NOT borrow.
                                    //If Vy > Vx, then VF is set to 1, otherwise 0. Then Vx is subtracted from Vy, and the results stored in Vx.

                                    if (V[opcode3] >= V[opcode2])
                                        V[0xF] = 1;
                                    else
                                        V[0xF] = 0;

                                    V[opcode2] = (V[opcode3] - V[opcode2]) & 0xff;

                                    break;
                                }

                            case (0xe):
                                {
                                    //8xyE - SHL Vx {, Vy}
                                    //Set Vx = Vx SHL 1. --------------------------------------------------------------------------------------------ERRONEO
                                    //If the most-significant bit of Vx is 1, then VF is set to 1, otherwise to 0. Then Vx is multiplied by 2.


                                    //Store the value of register VY shifted left one bit in register VX
                                    //Set register VF to the most significant bit prior to the shift

                                    if (Globals.Alternative8xy6Opcode == true && Screen.Mode == Screen.ScreenMode.Chip8)
                                    {
                                        V[0xf] = V[opcode3] >> 7;
                                        V[opcode2] = (V[opcode3] << 1) & 0xFF;
                                    }
                                    else
                                    {
                                        V[0xf] = V[opcode2] >> 7;
                                        V[opcode2] = (V[opcode2] << 1) & 0xFF;
                                    }

                                    break;
                                }
                            default:
                                MessageBox.Show("Unknown Opcode: 0x" + Convert.ToString(instruccion, 16) + " at 0x" + Convert.ToString(PC - 0x202, 16));
                                break;
                        }
                        break;
                    }

                case (0x9):
                    {
                        //9xy0 - SNE Vx, Vy
                        //Skip next instruction if Vx != Vy.
                        //The values of Vx and Vy are compared, and if they are not equal, the program counter is increased by 2.

                        if (V[opcode2] != V[opcode3]) PC += 2;
                        break;
                    }

                case (0xa):
                    {
                        //Annn - LD I, addr
                        //Set I = nnn.
                        //The value of register I is set to nnn.

                        I = instruccion & 0xFFF;
                        break;
                    }

                case (0xb):
                    {
                        //Bnnn - JP V0, addr
                        //Jump to location nnn + V0.
                        //The program counter is set to nnn plus the value of V0.

                        PC = (instruccion & 0xFFF) + V[0];
                        break;
                    }
                case (0xc):
                    {
                        //Cxkk - RND Vx, byte
                        //Set Vx = random byte AND kk.
                        //The interpreter generates a random number from 0 to 255, which is then ANDed with the value kk.
                        //The results are stored in Vx. See instruction 8xy2 for more information on AND.

                        V[opcode2] = Globals.random.Next(0, 255) & (instruccion & 0xFF);
                        break;
                    }

                case (0xd):
                    {
                        //Dxyn - DRW Vx, Vy, nibble
                        //Display n-byte sprite starting at memory location I at (Vx, Vy), set VF = collision.
                        //The interpreter reads n bytes from memory, starting at the address stored in I.
                        //These bytes are then displayed as sprites on screen at coordinates (Vx, Vy). Sprites are XORed onto the existing screen.
                        //If this causes any pixels to be erased, VF is set to 1, otherwise it is set to 0.
                        //If the sprite is positioned so part of it is outside the coordinates of the display, it wraps around to the opposite side of the screen.
                        //See instruction 8xy3 for more information on XOR, and section 2.4, Display, for more information on the Chip-8 screen and sprites.

                        Screen.Draw();

                        break;
                    }

                case (0xe):
                    {
                        switch (instruccion & 0xFF)
                        {
                            case (0x9e):
                                {
                                    //Ex9E - SKP Vx
                                    //Skip next instruction if key with the value of Vx is pressed.
                                    //Checks the keyboard, and if the key corresponding to the value of Vx is currently in the down position, PC is increased by 2.

                                    if (Input.KeyState[V[opcode2]] == Input.InputState.KeyDown || Input.KeyState[V[opcode2]] == Input.InputState.KeyHeld) PC += 2;
                                    break;
                                }

                            case (0xa1):
                                {
                                    //ExA1 - SKNP Vx
                                    //Skip next instruction if key with the value of Vx is not pressed.
                                    //Checks the keyboard, and if the key corresponding to the value of Vx is currently in the up position, PC is increased by 2.

                                    if (Input.KeyState[V[opcode2]] == Input.InputState.KeyUp) PC += 2;
                                    break;
                                }

                            default:
                                MessageBox.Show("Unknown Opcode: 0x" + Convert.ToString(instruccion, 16) + " at 0x" + Convert.ToString(PC - 0x202, 16));
                                break;
                        }
                        break;
                    }

                case (0xf):
                    {
                        switch (instruccion & 0xFF)
                        {
                            case (0x07):
                                {
                                    //Fx07 - LD Vx, DT
                                    //Set Vx = delay timer value.
                                    //The value of DT is placed into Vx.

                                    V[opcode2] = Globals.delayTimer;
                                    break;
                                }

                            case (0x0a):
                                {
                                    //Fx0A - LD Vx, K
                                    //Wait for a key press, store the value of the key in Vx.
                                    //All execution stops until a key is pressed, then the value of that key is stored in Vx.
                                    //Chip8 Wait for key release. SCHip, wait for key press.

                                    PC -= 2;

                                    for (int i = 0; i < 16; i++)
                                    {
                                        if (Input.KeyState[i] == Input.InputState.KeyDown)
                                        {
                                            V[opcode2] = i;
                                            PC += 2;

                                            //Input.KeyState[i] = Input.InputState.KeyHeld;
                                        }
                                    }

                                    break;
                                }

                            case (0x15):
                                {
                                    //Fx15 - LD DT, Vx
                                    //Set delay timer = Vx.
                                    //DT is set equal to the value of Vx.

                                    Globals.delayTimer = V[opcode2];
                                    break;
                                }

                            case (0x18):
                                {
                                    //Fx18 - LD ST, Vx
                                    //Set sound timer = Vx.
                                    //ST is set equal to the value of Vx.

                                    Sound.Timer = V[opcode2];
                                    break;
                                }

                            case (0x1e):
                                {
                                    //Fx1E - ADD I, Vx
                                    //Set I = I + Vx.
                                    //The values of I and Vx are added, and the results are stored in I.

                                    //Check FX1E (I = I + VX) buffer overflow.
                                    //If buffer overflow, register VF must be set to 1, otherwise 0. As a result, register VF not set to 1.

                                    I += V[opcode2];

                                    if (I > 0xFFF)
                                        V[0xF] = 1;
                                    else
                                        V[0xF] = 0;

                                    break;
                                }

                            case (0x29):
                                {
                                    //Fx29 - LD F, Vx
                                    //Set I = location of sprite for digit Vx.
                                    //The value of I is set to the location for the hexadecimal sprite corresponding to the value of Vx.
                                    //See section 2.4, Display, for more information on the Chip-8 hexadecimal font..

                                    I = (V[opcode2] & 0xF) * 5;
                                    break;
                                }

                            case (0x30):
                                {
                                    //Fx30 - LD F, Vx
                                    //Set I = location of sprite for digit Vx.
                                    //The value of I is set to the location for the hexadecimal sprite corresponding to the value of Vx.
                                    //Versión para SChip.

                                    I = ((V[opcode2] & 0xF) * 10) + 80;
                                    break;
                                }

                            case (0x33):  //OK
                                {
                                    //Fx33 - LD B, Vx
                                    //Store BCD representation of Vx in memory locations I, I+1, and I+2.
                                    //The interpreter takes the decimal value of Vx, and places the hundreds digit in memory at location in I, the tens digit at location I+1, and the ones digit at location I+2.

                                    string cadena = V[opcode2].ToString("000");

                                    Memory.RAM[I] = Convert.ToByte(cadena.Substring(0, 1));
                                    Memory.RAM[I + 1] = Convert.ToByte(cadena.Substring(1, 1));
                                    Memory.RAM[I + 2] = Convert.ToByte(cadena.Substring(2, 1));
                                    break;
                                }

                            case (0x55):
                                {
                                    //Fx55 - LD [I], Vx
                                    //Store registers V0 through Vx in memory starting at location I.
                                    //The interpreter copies the values of registers V0 through Vx into memory, starting at the address in I.
                                    //When done, I=I+X+1. Solo en Chip8

                                    for (int i = 0; i <= opcode2; i++)
                                    {
                                        Memory.RAM[I + i] = (byte)V[i];
                                    }

                                    if (Globals.AlternativeFx55Opcode == true)
                                        if (Screen.Mode == Screen.ScreenMode.Chip8) I += opcode2 + 1;

                                    break;
                                }

                            case (0x65):
                                {
                                    //Fx65 - LD Vx, [I]
                                    //Read registers V0 through Vx from memory starting at location I.
                                    //The interpreter reads values from memory starting at location I into registers V0 through Vx.
                                    //When done, I=I+X+1. Solo en Chip8

                                    for (int i = 0; i <= opcode2; i++)
                                    {
                                        V[i] = Memory.RAM[I + i];
                                    }

                                    if (Globals.AlternativeFx55Opcode == true)
                                        if (Screen.Mode == Screen.ScreenMode.Chip8) I += opcode2 + 1;

                                    break;
                                }
                            case (0x75):
                                {
                                    //Fx75: Save V0...VX (X<8) in the HP48 flags 

                                    for (int i = 0; i <= opcode2; i++)
                                    {
                                        HP48[i] = V[i];
                                    }

                                    break;
                                }
                            case (0x85):
                                {
                                    //Fx85: Load V0...VX (X<8) from the HP48 flags

                                    for (int i = 0; i <= opcode2; i++)
                                    {

                                        V[i] = HP48[i];
                                    }

                                    break;
                                }
                            default:
                                MessageBox.Show("Unknown Opcode: 0x" + Convert.ToString(instruccion, 16) + " at 0x" + Convert.ToString(PC - 0x202, 16));
                                break;
                        }
                        break;
                    }
            }

            #endregion
        }

        public static void ResetHardware()
        {
            // Reseteo de Timers
            Globals.delayTimer = 0x0;
            Sound.Timer = 0x0;

            // Reseteo de Registros generales
            instruccion = 0x0;
            PC = Globals.DIR_INICIO;
            SP = 0x0;
            I = 0x0;

            for (int i = 0; i < 16; i++)
                V[i] = 0;

            // Limpiado del Registro V
            for (int regActual = 0; regActual < Globals.CANT_REGISTROS; regActual++)
            {
                Memory.RAM[regActual] = 0x0;
            }

            for (int i = 0; i < 8; i++)
                HP48[i] = 0;

            // Limpiado de Memory.RAM
            for (int dir = 0; dir < Globals.TAMANO_MEM; dir++)
            {
                Memory.RAM[dir] = 0x0;
            }

            // Limpiado de la Pila
            for (int item = 0; item < Globals.TAMANO_PILA; item++)
            {
                Stack[item] = 0x0;
            }

            for (int i = 0; i < Globals.RES_X; i++)
            {
                for (int j = 0; j < Globals.RES_Y; j++)
                {
                    Memory.Screen[i, j] = 0x0;
                }
            }

            // Carga de Fuentes a Memory.RAM (eran 80 bytes, 5 byte por cada una de las 16 letras)
            for (int i = 0; i < 80; i++)
            {
                Memory.RAM[i] = Memory.FontChip8[i];
            }

            //Cargar fuente SChip
            for (int i = 0; i < 160; i++)
            {
                Memory.RAM[i + 80] = Memory.FontSChip[i];
            }

            Screen.ChangeMode(Screen.ScreenMode.Chip8);
        }

        public static void Run()
        {
            if (ROM.Load(Globals.ROMPath) == true)
            {
                Globals.random = new Random();
                Globals.CPUStopWatch.Start();

                while (Globals.Stop == false && Globals.EmulationState == Globals.Emulation_State.Running) //game-loop
                {
                    //Application.DoEvents();

                    Timers();
                    ProcessOpcodes();
                    Input.keyHeld();

                    Thread.Sleep(0);
                }
                Globals.CPUStopWatch.Reset();
            }
        }

        static void Timers()
        {
            if (Globals.CPUStopWatch.Elapsed.TotalMilliseconds >= Globals.Hz)
            {
                Globals.CPUStopWatch.Reset();
                Globals.CPUStopWatch.Start();

                if (Globals.delayTimer > 0) Globals.delayTimer--;
                if (Sound.Timer > 0)
                {
                    if (Sound.Enabled == true)
                    {
                        if (Sound.IsSounding == false)
                        {
                            Sound.SoundTime = Sound.Timer;

                            if (Sound.sound == null || Sound.sound.IsAlive == false)
                            {
                                Sound.sound = new Thread(new ThreadStart(Sound.Play));
                                Sound.sound.Start();
                            }

                            Sound.IsSounding = true;
                        }
                    }
                    Sound.Timer--;
                }

                Globals.CanDraw = 1;
            }
            else
                Globals.CanDraw = 0;
        }
    }
}
