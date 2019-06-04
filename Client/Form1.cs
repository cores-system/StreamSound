using Microsoft.AspNetCore.SignalR.Client;
using NAudio.Wave;
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client
{
    public partial class Form1 : Form
    {
        #region Form
        static WaveOutEvent wo;
        static BufferedWaveProvider bwp;
        NotifyIcon ni = new NotifyIcon();
        private TextBox textBox1;

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


        #region SoundPlay
        async void SoundPlay()
        {
            // Удаленный WaveIn
            var connection = new HubConnectionBuilder().WithUrl("http://192.168.0.110:5089/sound").Build();

            #region Closed
            connection.Closed += async (error) =>
            {
                await Task.Delay(5);
                await connection.StartAsync();
                textBox1.Text += "reConnection: " + connection.State.ToString() + "\n"; ;
            };
            #endregion

            #region ConnectionId
            connection.On<string>("ConnectionId", ConnectionId =>
            {
                textBox1.Text = "Id: " + ConnectionId + "\n";
            });
            #endregion

            #region DataAvailable
            connection.On<byte[], int>("DataAvailable", (Buffer, BytesRecorded) =>
            {
                bwp.AddSamples(Buffer, 0, BytesRecorded);
            });
            #endregion

            // Подключаемся
            await connection.StartAsync();
            textBox1.Text += connection.State.ToString() + "\n";

            #region WaveOut
            wo = new WaveOutEvent();
            bwp = new BufferedWaveProvider(new WaveFormat(48000, 2));
            bwp.DiscardOnBufferOverflow = true;
            wo.Init(bwp);
            wo.Play();
            #endregion
        }
        #endregion
    }
}
