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
                ActivClients.All.SendAsync("DataAvailable", e.Buffer, e.BytesRecorded, DateTime.Now);
        }


        public override Task OnConnectedAsync()
        {
            ActivClients = Clients;
            Console.WriteLine("ConnectionId: " + Context.ConnectionId);
            Clients.Client(Context.ConnectionId).SendAsync("ConnectionId", Context.ConnectionId, Startup.s.BufferMilliseconds, DateTime.Now);
            return base.OnConnectedAsync();
        }
    }
}
