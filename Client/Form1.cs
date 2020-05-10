using Microsoft.AspNetCore.SignalR.Client;
using NAudio.Wave;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client
{
    public partial class Form1 : Form
    {
        #region Form
        static WaveOutEvent wo;
        static BufferedWaveProvider bwp;
        HubConnection hubConnection;
        NotifyIcon ni = new NotifyIcon();
        TextBox textBox1;
        private Button button1, button2;

        public Form1()
        {
            InitializeComponent();
            this.Resize += new EventHandler(this.FormMain_Resize);
            this.FormClosing += new FormClosingEventHandler(this.FormMain_FormClosing);
            textBox1 = new TextBox();

            this.Height = 400;
            this.Width = 600;

            this.textBox1.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom) | AnchorStyles.Left) | AnchorStyles.Right)));
            this.textBox1.Location = new Point(0, 60);
            this.textBox1.Multiline = true;
            this.textBox1.ReadOnly = true;
            this.textBox1.Size = new Size(600, 400);
            this.textBox1.TabIndex = 0;
            this.Controls.Add(this.textBox1);


            this.button1 = new Button();
            this.button1.Location = new Point(12, 12);
            this.button1.Size = new Size(75, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "Рестарт";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new EventHandler(this.RestartTimePlay);
            this.Controls.Add(this.button1);

            this.button2 = new Button();
            this.button2.Location = new Point(100, 12);
            this.button2.Size = new Size(75, 23);
            this.button2.TabIndex = 0;
            this.button2.Text = "Выход";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new EventHandler(this.ExitApp);
            this.Controls.Add(this.button2);

            ni = new NotifyIcon();
            ni.Icon = new Icon("tree.ico");
            ni.Visible = true;

            ni.DoubleClick += delegate (object sender, EventArgs args)
            {
                ni.Visible = false;
                this.Show();
                this.WindowState = FormWindowState.Normal;
            };


            WaveOut();
            HubBuilder();
            SoundPlay();
        }
        #endregion

        #region FormMain_Resize
        private void FormMain_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized && ni.Visible)
            {
                this.Hide();
                ni.Visible = true;
            }
        }
        #endregion

        #region FormMain_FormClosing
        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.WindowState = FormWindowState.Minimized;
            this.Hide();
            ni.Visible = true;
        }
        #endregion

        #region RestartTimePlay
        private void RestartTimePlay(object sender, EventArgs e)
        {
            Process.Start(Application.StartupPath + "\\Client.exe");
            Environment.Exit(0);
        }
        #endregion

        #region ExitApp
        private void ExitApp(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }
        #endregion


        #region WaveOut
        void WaveOut()
        {
            wo = new WaveOutEvent();
            bwp = new BufferedWaveProvider(new WaveFormat(48000, 2));
            bwp.DiscardOnBufferOverflow = true;
            wo.Init(bwp);
            wo.Play();
        }
        #endregion


        #region HubBuilder
        void HubBuilder()
        {
            // Удаленный WaveIn
            hubConnection = new HubConnectionBuilder().WithUrl("http://192.168.0.110:5089/sound").Build();
            hubConnection.Closed += HubConnection_Closed;

            // https://docs.microsoft.com/en-us/aspnet/core/signalr/configuration?view=aspnetcore-2.2&tabs=dotnet
            hubConnection.ServerTimeout = TimeSpan.FromSeconds(2);
            hubConnection.KeepAliveInterval = TimeSpan.FromMilliseconds(300);
            hubConnection.HandshakeTimeout = TimeSpan.FromSeconds(5);
        }
        #endregion

        #region HubConnection_Closed
        async Task HubConnection_Closed(Exception arg)
        {
            hubConnection.Closed -= HubConnection_Closed;
            textBox1.Text += "Connection Closed" + Environment.NewLine;
            await hubConnection.DisposeAsync();
            await Task.Delay(80);
            HubBuilder();

            textBox1.Text += "Reconnection" + Environment.NewLine;
            if (bwp != null)
                bwp.ClearBuffer();
            SoundPlay();
        }
        #endregion

        #region SoundPlay
        async void SoundPlay()
        {
            int bufferMilliseconds = 20;
            double timeDifferenceWithServer = 0;

            #region ConnectionId
            hubConnection.On<string, int, DateTime>("ConnectionId", (ConnectionId, BufferMilliseconds, serverTime) =>
            {
                textBox1.Text = "Id: " + ConnectionId + Environment.NewLine;
                bufferMilliseconds = BufferMilliseconds;
                timeDifferenceWithServer = (DateTime.Now - serverTime).TotalMilliseconds;
            });
            #endregion

            #region DataAvailable
            hubConnection.On<byte[], int, DateTime>("DataAvailable", (Buffer, BytesRecorded, soundPlayTime) =>
            {
                int offset = 0;
                DateTime currentTime = DateTime.Now;
                soundPlayTime = soundPlayTime.AddMilliseconds(timeDifferenceWithServer);

                // Пакет пришел слишком поздно
                if (currentTime > soundPlayTime)
                {
                    // Разница времени
                    double differenceMs = (DateTime.Now - soundPlayTime).TotalMilliseconds;

                    // Смещаем offset
                    offset = (int)(differenceMs * 192); // 1ms = 192 byte

                    // Разница больше BufferMilliseconds или больше пакета
                    if (differenceMs >= bufferMilliseconds || offset >= BytesRecorded)
                    {

                        #region debug
                        //File.AppendAllText("debug.txt",
                        //    currentTime.ToString() + "." + currentTime.Millisecond + Environment.NewLine +
                        //    "differenceMs: " + differenceMs + " / max: " + bufferMilliseconds + Environment.NewLine +
                        //    Environment.NewLine + Environment.NewLine
                        //);
                        #endregion
                        return;
                    }

                    #region debug
                    //File.AppendAllText("debug.txt",
                    //    currentTime.ToString() + "." + currentTime.Millisecond + Environment.NewLine +
                    //    "differenceMs: " + differenceMs + " / max: " + bufferMilliseconds + Environment.NewLine +
                    //    "offset: " + offset + Environment.NewLine +
                    //    Environment.NewLine + Environment.NewLine
                    //);
                    #endregion
                }

                // Заполняем буфер
                bwp.AddSamples(Buffer, offset, BytesRecorded);
            });
            #endregion

            #region Подключаемся
            try
            {
                await hubConnection.StartAsync();
                textBox1.Text += hubConnection.State.ToString() + Environment.NewLine;
            }
            catch
            {
                await HubConnection_Closed(null);
            }
            #endregion
        }
        #endregion
    }
}
