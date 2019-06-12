using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using NAudio.Wave;

namespace ServerSound
{
    public class SoundHub : Hub
    {
        #region SoundHub
        static IHubCallerClients activClients;
        static ConcurrentDictionary<Task, (CancellationToken, DateTime)> sends = new ConcurrentDictionary<Task, (CancellationToken, DateTime)>();
        #endregion

        #region wi_DataAvailable
        public static void wi_DataAvailable(object sender, WaveInEventArgs e)
        {
            if (activClients != null)
            {
                #region Удаляем старые пакеты
                if (sends.Count > 0)
                {
                    foreach (var s in sends)
                    {
                        if (DateTime.Now > s.Value.Item2)
                        {
                            if (!s.Key.IsCompleted && !s.Key.IsCompletedSuccessfully)
                                s.Value.Item1.ThrowIfCancellationRequested();

                            s.Key.Dispose();
                            sends.TryRemove(s.Key, out _);
                        }
                    }
                }
                #endregion

                // Отправляем пакет
                var cts = new CancellationToken();
                sends.TryAdd(activClients.All.SendAsync("DataAvailable", e.Buffer, e.BytesRecorded, DateTime.Now, cancellationToken: cts), (cts, DateTime.Now.AddMilliseconds(Startup.s.BufferMilliseconds)));
            }
        }
        #endregion

        #region OnConnectedAsync
        public override Task OnConnectedAsync()
        {
            activClients = Clients;
            Clients.Client(Context.ConnectionId).SendAsync("ConnectionId", Context.ConnectionId, Startup.s.BufferMilliseconds, DateTime.Now);
            Console.WriteLine("ConnectionId: " + Context.ConnectionId);
            return base.OnConnectedAsync();
        }
        #endregion
    }
}
