using System;
using System.Threading;

namespace PacoChip_8
{
    static class Sound
    {
        public static int Timer;
        public static bool IsSounding = false;
        public static int SoundTime = 0;
        public static bool Enabled = true;

        public static Thread sound;

        public static void Play()
        {
            if (Sound.Timer != 0) Console.Beep(1000, (Sound.SoundTime * 1000) / 60);
            IsSounding = false;
        }
    }
}
