using SimpleTCP;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.IO;

/*
        * ==========================================
        *               Notes for me!
        * ==========================================
        * 
        * Currently the client reads the "pause" but it is being read by worker 1.
        * Only need two threads. One for sending and one for receiving. In the receiving, parse the string,
        * and if it says "Command:command" run function, but if it says "Chat:message" run chat.
        */

namespace networkingPlayer
{
    public partial class Form1 : Form
    {
        private TcpClient client;
        public StreamReader STR;
        public StreamWriter STW;
        public string receiveText;
        public string TextToSend;
        public string sendCommand;
        public string receiveCommand;

        public Form1()
        {
            InitializeComponent();

            IPAddress[] localIP = Dns.GetHostAddresses(Dns.GetHostName());
            foreach(IPAddress address in localIP)
            {
                if(address.AddressFamily == AddressFamily.InterNetwork)
                {
                    ServerIPtextBox.Text = address.ToString();
                }
            }
        }
        //start server code
        private void btnStart_Click(object sender, EventArgs e)
        {
            TcpListener listener = new TcpListener(IPAddress.Any, int.Parse(ServerPorttextBox.Text));
            listener.Start();
            client = listener.AcceptTcpClient();
            STR = new StreamReader(client.GetStream());
            STW = new StreamWriter(client.GetStream());
            STW.AutoFlush = true; //Ask prof about this one.
            backgroundWorker1.RunWorkerAsync();
            backgroundWorker2.WorkerSupportsCancellation = true;

            
        }

        //Connect to host code below.
        private void ConnectButton_Click(object sender, EventArgs e)
        {
            client = new TcpClient();
            IPEndPoint IpEnd = new IPEndPoint(IPAddress.Parse(ClientIPtextBox.Text), int.Parse(clientPorttextBox.Text));
            try
            {
                client.Connect(IpEnd);
                if (client.Connected)
                {
                    ChatScreentextbox.AppendText("Connected to Server" + "\n");
                    STR = new StreamReader(client.GetStream());
                    STW = new StreamWriter(client.GetStream());
                    STW.AutoFlush = true;
                    backgroundWorker1.RunWorkerAsync();
                    backgroundWorker2.WorkerSupportsCancellation = true;

                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            while(client.Connected)
            {
                try
                {
                    receiveText = STR.ReadLine();
                    string[] words = receiveText.Split('Û');
                    string checkString = words[0];

                    if (checkString == "chat")
                    {
                        this.ChatScreentextbox.Invoke(new MethodInvoker(delegate ()
                        {
                            ChatScreentextbox.AppendText("Partner: " + words[1] + "\n");
                        }));
                        receiveText = "";
                    }

                    else if (checkString == "command")
                    {
                        string playerCommand = words[1];
                        if (playerCommand == "pause"){
                            
                            axWindowsMediaPlayer1.Ctlcontrols.currentPosition = Convert.ToDouble(words[2]);
                            axWindowsMediaPlayer1.Ctlcontrols.pause();
                            
                        }
                        if (playerCommand == "play"){
                            axWindowsMediaPlayer1.Ctlcontrols.play();
                        }
                        
                    }
                }   
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message.ToString());
                }





                /*  "chat:hello"
                  "pause"
                  "play" */

            }
        }

        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            if(client.Connected)
            {
                
                STW.WriteLine (TextToSend);
                string[] words = TextToSend.Split('Û');
                string checkString = words[0];

                if(checkString == "chat")
                {
                    this.ChatScreentextbox.Invoke(new MethodInvoker(delegate ()
                    {
                        ChatScreentextbox.AppendText("Me:" + words[1] + "\n");
                    }));
                }
                
               
             }
            else
            {
                MessageBox.Show("Sending failed");
            }
            backgroundWorker2.CancelAsync();
        }

       /* private void backgroundWorker3_DoWork(object sender, DoWorkEventArgs e)
        {
            while (client.Connected)
            {
                
                if (STR.ReadLine() == "pause")
                {

                    this.axWindowsMediaPlayer1.Invoke(new MethodInvoker(delegate ()
                    {
                        axWindowsMediaPlayer1.Ctlcontrols.pause();
                    }));
                    receiveCommand = " ";
                }
             
            }
            
            backgroundWorker3.CancelAsync();
        }*/


        private void SendButton_Click(object sender, EventArgs e)
        {
            if(MessagetextBox.Text != "")
            {
                TextToSend = "chatÛ" + MessagetextBox.Text;
                backgroundWorker2.RunWorkerAsync();
            }
            MessagetextBox.Text = "";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            if(openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.startBox1.Text = openFileDialog1.FileName;
            }
        }
        
        //Below code executes button functionality.

        private void button2_Click(object sender, EventArgs e)
        {
            axWindowsMediaPlayer1.URL = startBox1.Text;
            axWindowsMediaPlayer1.Ctlcontrols.play();
        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            axWindowsMediaPlayer1.Ctlcontrols.stop();
        }

        private void pauseButton_Click(object sender, EventArgs e)
        {
            //CODE FOR CLICKING PAUSE.
            try
            {
                axWindowsMediaPlayer1.Ctlcontrols.pause();
                double videoPosition = axWindowsMediaPlayer1.Ctlcontrols.currentPosition; //Gets current position of file.
                string position = videoPosition.ToString(); //Converts the double to a string for sending.
                string commandText = "commandÛpauseÛ" + position; //Appends position to command string so that it can be parsed.
                STW.WriteLine(commandText);
            }
            catch(Exception ex)
            {
                ChatScreentextbox.AppendText("No server-client connection.");
            }
            
            
            
         }

        private void playButton_Click(object sender, EventArgs e)
        {
            try
            {
                axWindowsMediaPlayer1.Ctlcontrols.play();
                ChatScreentextbox.AppendText("Resuming video." + "\n");
                string commandText = "commandÛplay";
                STW.WriteLine(commandText);
            }
            catch (Exception ex)
            {
                ChatScreentextbox.AppendText("No server-client connection.");
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

       



       
    }
}
