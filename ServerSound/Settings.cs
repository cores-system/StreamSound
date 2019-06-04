using System;

namespace ServerSound
{
    public class Settings
    {
        public int DeviceNumber { get; set; } = 0;

        public int WavRate { get; set; } = 48000;

        public int BufferMilliseconds { get; set; } = 20;
    }
}
