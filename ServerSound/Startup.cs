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
            services.AddSignalR();
        }
        #endregion

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            Console.WriteLine("v: 0.01");

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
                routes.MapHub<SoundHub>("/sound");
            });


            app.Run(async (context) =>
            {
                File.WriteAllText("settings.json", JsonConvert.SerializeObject(s, Formatting.Indented));
                await context.Response.WriteAsync("Save settings.json");
            });
        }
    }
}
