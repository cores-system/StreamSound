using NAudio.Wave;
using System;

namespace InfoDevices
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("WaveIn");
            for (int waveDevice = 0; waveDevice < WaveIn.DeviceCount; waveDevice++)
            {
                WaveInCapabilities deviceInfo = WaveIn.GetCapabilities(waveDevice);
                Console.WriteLine("Device {0}: {1}, {2} channels", waveDevice, deviceInfo.ProductName, deviceInfo.Channels);
            }

            Console.WriteLine("\nWaveOut");
            for (int waveDevice = 0; waveDevice < WaveOut.DeviceCount; waveDevice++)
            {
                WaveOutCapabilities deviceInfo = WaveOut.GetCapabilities(waveDevice);
                Console.WriteLine("Device {0}: {1}, {2} channels", waveDevice, deviceInfo.ProductName, deviceInfo.Channels);
            }

            Console.ReadLine();
        }
    }
}
