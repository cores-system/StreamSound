using Microsoft.AspNetCore.SignalR.Client;
using NAudio.Wave;
using System;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        static WaveOutEvent wo;
        static BufferedWaveProvider bwp;
        static WaveFileWriter waweFile;


        static void Main(string[] args)
        {
            var connection = new HubConnectionBuilder().WithUrl("http://192.168.0.110:5089/sound").Build();

            connection.Closed += async (error) =>
            {
                await Task.Delay(10);
                await connection.StartAsync();
            };

            connection.On<string>("ConnectionId", ConnectionId =>
            {
                Console.WriteLine("Id: " + ConnectionId);
            });

            connection.On<byte[], int>("DataAvailable", (Buffer, BytesRecorded) =>
            {
                bwp.AddSamples(Buffer, 0, BytesRecorded);
            });

            connection.StartAsync().Wait();
            Console.WriteLine(connection.State.ToString());





            wo = new WaveOutEvent();
            bwp = new BufferedWaveProvider(new WaveFormat(48000, 2));
            //waweFile = new WaveFileWriter(@"D:\Trash\Temporarily\publish\this.wav", new WaveFormat(48000, 2));
            bwp.DiscardOnBufferOverflow = true;
            wo.Init(bwp);
            wo.Play();





            Console.ReadLine();
        }
    }
}
