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

        public Form1()
        {
            InitializeComponent();
            this.Resize += new EventHandler(this.FormMain_Resize);
            this.FormClosing += new FormClosingEventHandler(this.FormMain_FormClosing);
            textBox1 = new TextBox();

            this.Height = 400;
            this.Width = 600;

            this.textBox1.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom) | AnchorStyles.Left) | AnchorStyles.Right)));
            this.textBox1.Location = new Point(0, 0);
            this.textBox1.Multiline = true;
            this.textBox1.ReadOnly = true;
            this.textBox1.Size = new Size(600, 400);
            this.textBox1.TabIndex = 0;
            this.Controls.Add(this.textBox1);

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
        }
        #endregion

        #region HubConnection_Closed
        async Task HubConnection_Closed(Exception arg)
        {
            hubConnection.Closed -= HubConnection_Closed;
            textBox1.Text += "Connection Closed" + Environment.NewLine;
            hubConnection.DisposeAsync().Wait();
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
            Stopwatch watch = new Stopwatch();

            #region ConnectionId
            hubConnection.On<string, int>("ConnectionId", (ConnectionId, BufferMilliseconds) =>
            {
                textBox1.Text = "Id: " + ConnectionId + Environment.NewLine;
                bufferMilliseconds = BufferMilliseconds;
            });
            #endregion

            #region DataAvailable
            long acceptedPackages = 0;
            hubConnection.On<byte[], int>("DataAvailable", (Buffer, BytesRecorded) =>
            {
                int offset = 0;

                // Начало воспроизведения звука
                if (acceptedPackages == 0)
                    watch.Start();

                // Пакет пришел слишком поздно, вместо части пакета уже проигран пустой звук
                if (acceptedPackages > 0 && watch.ElapsedMilliseconds > (acceptedPackages * bufferMilliseconds))
                {
                    // Разница времени
                    long differenceMs = watch.ElapsedMilliseconds - (acceptedPackages * bufferMilliseconds);

                    // Разница больше BufferMilliseconds
                    if (differenceMs >= bufferMilliseconds) {
                        acceptedPackages++;
                        return;
                    }

                    // Смещаем offset
                    offset = (int)(differenceMs * 192); // 1ms = 192 byte

                    #region debug
                    //File.AppendAllText("debug.txt", 
                    //    DateTime.Now.Minute + ":" + DateTime.Now.Second + "" + DateTime.Now.Millisecond + Environment.NewLine +
                    //    "differenceMs: " + differenceMs + " / max: " + bufferMilliseconds + Environment.NewLine + 
                    //    "offset: " + offset + Environment.NewLine +
                    //    "watch: " + watch.ElapsedMilliseconds + " / " + acceptedPackages + " = " + (acceptedPackages * bufferMilliseconds) + 
                    //    Environment.NewLine + Environment.NewLine
                    //);
                    #endregion
                }

                // Заполняем буфер
                acceptedPackages++;
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
