using NAudio.Wave;
using System;

namespace InfoDevices
{
    class Program
    {
        static void Main(string[] args)
        {
            for (int waveInDevice = 0; waveInDevice < WaveIn.DeviceCount; waveInDevice++)
            {
                WaveInCapabilities deviceInfo = WaveIn.GetCapabilities(waveInDevice);
                Console.WriteLine("Device {0}: {1}, {2} channels", waveInDevice, deviceInfo.ProductName, deviceInfo.Channels);
            }

            Console.ReadLine();
        }
    }
}
