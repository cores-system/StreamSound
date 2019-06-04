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


        static void Main(string[] args)
        {
            // Удаленный WaveIn
            var connection = new HubConnectionBuilder().WithUrl("http://192.168.0.110:5089/sound").Build();

            #region Closed
            connection.Closed += async (error) =>
            {
                await Task.Delay(10);
                await connection.StartAsync();
            };
            #endregion

            #region ConnectionId
            connection.On<string>("ConnectionId", ConnectionId =>
            {
                Console.WriteLine("Id: " + ConnectionId);
            });
            #endregion

            #region DataAvailable
            connection.On<byte[], int>("DataAvailable", (Buffer, BytesRecorded) =>
            {
                bwp.AddSamples(Buffer, 0, BytesRecorded);
            });
            #endregion

            // Подключаемся
            connection.StartAsync().Wait();
            Console.WriteLine(connection.State.ToString());

            #region WaveOut
            wo = new WaveOutEvent();
            bwp = new BufferedWaveProvider(new WaveFormat(48000, 2));
            bwp.DiscardOnBufferOverflow = true;
            wo.Init(bwp);
            wo.Play();
            #endregion


            // Wait
            Console.ReadLine();
        }
    }
}
