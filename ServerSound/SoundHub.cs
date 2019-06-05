using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using NAudio.Wave;

namespace ServerSound
{
    public class SoundHub : Hub
    {
        static IHubCallerClients ActivClients;

        public static void wi_DataAvailable(object sender, WaveInEventArgs e)
        {
            if (ActivClients != null)
            {
                int length = e.Buffer.Length;
                int BytesRecorded = e.BytesRecorded; // 1ms == 192 byte

                for (int i = length - 1; i >= 0; i--)
                {
                    byte b = e.Buffer[i];
                    if (10 >= b || b >= 240) {
                        BytesRecorded--;
                        continue;
                    }

                    break;
                }

                // Есть аудио
                if (BytesRecorded > 0)
                    ActivClients.All.SendAsync("DataAvailable", e.Buffer, e.BytesRecorded);
            }
        }


        public override Task OnConnectedAsync()
        {
            ActivClients = Clients;
            Console.WriteLine("ConnectionId: " + Context.ConnectionId);
            Clients.Client(Context.ConnectionId).SendAsync("ConnectionId", Context.ConnectionId, Startup.s.BufferMilliseconds);
            return base.OnConnectedAsync();
        }
    }
}
