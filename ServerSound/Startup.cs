using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using NAudio.Wave;
using Newtonsoft.Json;

namespace ServerSound
{
    public class Startup
    {
        static WaveInEvent wi;
        public static Settings s = new Settings();

        #region ConfigureServices
        public void ConfigureServices(IServiceCollection services)
        {
            // https://docs.microsoft.com/en-us/aspnet/core/signalr/configuration?view=aspnetcore-2.2&tabs=dotnet
            services.AddSignalR(hubOptions =>
            {
                //hubOptions.ClientTimeoutInterval = TimeSpan.FromSeconds(10); // Почему то заставляет обрывать связь каждые 30+- секунд
                hubOptions.KeepAliveInterval = TimeSpan.FromMilliseconds(100);
                hubOptions.HandshakeTimeout = TimeSpan.FromSeconds(5);
            });
        }
        #endregion

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            Console.WriteLine("v: 0.02-17");

            if (File.Exists("settings.json"))
                s = JsonConvert.DeserializeObject<Settings>(File.ReadAllText("settings.json"));

            #region WaveIn
            wi = new WaveInEvent();
            wi.DeviceNumber = s.DeviceNumber;
            wi.WaveFormat = new WaveFormat(s.WavRate, 2);
            wi.DataAvailable += new EventHandler<WaveInEventArgs>(SoundHub.wi_DataAvailable);
            wi.BufferMilliseconds = s.BufferMilliseconds;
            wi.StartRecording();
            #endregion

            // 
            app.UseSignalR(routes =>
            {
                routes.MapHub<SoundHub>("/sound", (options) =>
                {
                    options.ApplicationMaxBufferSize = 1000000;
                    options.TransportMaxBufferSize = 1000000;
                    options.WebSockets.CloseTimeout = TimeSpan.FromSeconds(2);
                    options.LongPolling.PollTimeout = TimeSpan.FromSeconds(2);
                });
            });


            app.Run(async (context) =>
            {
                File.WriteAllText("settings.json", JsonConvert.SerializeObject(s, Formatting.Indented));
                await context.Response.WriteAsync("Save settings.json");
            });
        }
    }
}
