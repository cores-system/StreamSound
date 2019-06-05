using NAudio.Wave;
using System;
using System.IO;
using System.Text;

namespace Test
{
    class Program
    {
        static WaveInEvent wi;
        static WaveOutEvent wo;
        static BufferedWaveProvider bwp;
        static FileStream stream;

        static void Main(string[] args)
        {
            wo = new WaveOutEvent();
            bwp = new BufferedWaveProvider(new WaveFormat(48000, 2));
            bwp.DiscardOnBufferOverflow = true;
            wo.Init(bwp);
            wo.Play();


            wi = new WaveInEvent();
            wi.DeviceNumber = 0;
            wi.WaveFormat = new WaveFormat(48000, 2);
            wi.DataAvailable += new EventHandler<WaveInEventArgs>(wi_DataAvailable);
            wi.BufferMilliseconds = 20;
            //stream = File.Open("sound.txt", FileMode.OpenOrCreate);
            wi.StartRecording();


            Console.ReadLine();
        }


        public static void wi_DataAvailable(object sender, WaveInEventArgs e)
        {
            bwp.AddSamples(e.Buffer, 0, e.BytesRecorded);
            bwp.AddSamples(e.Buffer, 0, e.BytesRecorded);
            Console.WriteLine(bwp.BufferedBytes + " / " + bwp.BufferLength);
        }
    }
}
