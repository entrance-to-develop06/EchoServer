using EchoServer.FormDesign;
using System.Net.Sockets;

namespace EchoServer
{
    public partial class Form1Control : Form
    {
        private System.Net.Sockets.TcpListener SvrListener;
        private System.Net.Sockets.TcpClient UserClient;
        private System.Net.Sockets.NetworkStream NetStm;
        private System.Threading.CancellationTokenSource cts;
        private bool EndFlag = false;
        private bool IsClientCn = false;
        private Form1Design fDesign = new Form1Design();

        public Form1Control()
        {
            InitializeComponent();
            fDesign.Setting();
            FormDesignSetting();
        }

        private void Form1Control_Load(object sender, EventArgs e)
        {
            this.Location = new Point(250, 100);

            fDesign.buttonCnCancel.Enabled = false;
            fDesign.richTextBox1.AppendText("クライアント接続ボタンを押しクライアント接続を有効にして下さい。" + "\r\n");
        }

        private void Form1Control_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (IsClientCn)
            {
                NetStm.Close();
                UserClient.Close();
                SvrListener.Stop();
            }
        }

        private void ButtonExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private async void ButtonConec_Click(object sender, EventArgs e)
        {
            SvrListener = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Any, 3000);
            cts = new System.Threading.CancellationTokenSource();

            fDesign.buttonConec.Enabled = false;
            fDesign.buttonCnCancel.Enabled = true;
            fDesign.richTextBox1.AppendText("サーバーはクライアントの接続を待っています。" + "\r\n");

            await ClientMatchi();
            if (EndFlag)
            {
                IsClientCn = false;
                fDesign.richTextBox1.AppendText("クライアント接続をキャンセルしました。" + "\r\n");
                return;
            }
            else
            {
                IsClientCn = true;
                fDesign.richTextBox1.AppendText("クライアントが接続されました。" + "\r\n");
            }

            NetStm = UserClient.GetStream();
            fDesign.buttonCnCancel.Enabled = false;
            fDesign.buttonAsyncCancel.Enabled = true;
            fDesign.richTextBox1.AppendText("クライアントからの電文を待機しています。" + "\r\n");

            try
            {
                var tk1 = Task.Run(() =>
                {
                    RecSend(cts.Token);
                });
                await tk1;

            }
            catch (System.OperationCanceledException ex)
            {
                fDesign.buttonConec.Enabled = true;
                fDesign.buttonAsyncCancel.Enabled = false;
                fDesign.richTextBox1.AppendText("送受信を強制終了しました。" + "\r\n");
            }
        }

        private void ButtonCnCancel_Click(object sender, EventArgs e)
        {
            SvrListener.Stop();
            fDesign.buttonConec.Enabled = true;
            fDesign.buttonCnCancel.Enabled = false;
            fDesign.buttonAsyncCancel.Enabled = false;
            EndFlag = true;
        }

        private void ButtonAsyncCancel_Click(object sender, EventArgs e)
        {
            //非同期処理をキャンセルします。
            cts.Cancel();
        }

        //非同期で接続待ち
        private async System.Threading.Tasks.Task ClientMatchi()
        {
            SvrListener.Start();
            var tk1 = Task.Run(() =>
            {
                try
                {
                    UserClient = SvrListener.AcceptTcpClient();
                }
                catch (SocketException ex)
                {
                    //SvrListener.Stop()メソッドが実行されたら例外が発生する。
                    this.Invoke((Action)(() => { fDesign.richTextBox1.AppendText(ex.Message + "\r\n"); }));
                }
            });
            await tk1;
        }

        //非同期電文送受信
        private async void RecSend(System.Threading.CancellationToken ct)
        {
            //クライアントから送られたデータを受信する
            System.Text.Encoding enc = System.Text.Encoding.UTF8;
            bool disconnected = false;
            System.IO.MemoryStream ms;
            byte[] resBytes = new byte[255]; // = { };
            string MsgRece = "";
            string MsgSend = "";

            while (EndFlag == false)
            {
                ms = new System.IO.MemoryStream();
                while (NetStm.DataAvailable == false)
                {
                    try
                    {
                        ct.ThrowIfCancellationRequested();
                    }
                    catch (OperationCanceledException ex)
                    {
                        //cts.Cancel()メソッドが実行されたら例外が発生する。
                        this.Invoke((Action)(() =>
                        {
                            fDesign.buttonConec.Enabled = true;
                            fDesign.buttonAsyncCancel.Enabled = false;
                            fDesign.richTextBox1.AppendText("送受信を強制終了しました。2" + "\r\n");
                        }));
                        if (IsClientCn)
                        {
                            NetStm.Close();
                            UserClient.Close();
                            SvrListener.Stop();
                        }
                        return;
                    }
                    await Task.Delay(100);
                }
                //データの一部を受信する。
                //NetworkStream.Read()メソッドを実行して読み取れるデータが無い場合(実は0を返さない様だ)ここでスレッドはブロックされる。
                //これを実行する前にデータの受信が1以上になるまで待機してから実行する事。
                //このブロックはNetworkstream.ReadTimeoutの設定値により決まるが既定値は-1(無限)なのでデータが送られるまでブロックされる。
                //DataAvailableプロパティでTrue(受信バッファ有り)をチェックしてからならReadTimeoutのタイムアウトは事実上発生しない。
                //ReadTimeoutプロパティはNetworkstream.Read()メソッドを実行した後、読み取り可能データが入るまでの時間の様だ。
                int resSize = NetStm.Read(resBytes, 0, resBytes.Length);
                //Readが0を返した時はクライアントが切断したと判断
                if (resSize == 0)
                {
                    disconnected = true;
                    return;
                }
                //受信したデータを蓄積する
                ms.Write(resBytes, 0, resSize);


                //受信したデータを文字列に変換
                string resMsg = enc.GetString(ms.ToArray());
                ms.Close();
                MsgRece = resMsg.Replace("\r", "");
                this.Invoke((Action)(() => { fDesign.richTextBox1.AppendText(MsgRece + "\r\n"); }));

                if (!disconnected)
                {
                    //クライアントにデータを送信する
                    //クライアントに送信する文字列を作成
                    string sendMsg = "";
                    if (resMsg.StartsWith("ST R") || resMsg.StartsWith("RS R"))
                    {
                        sendMsg = "OK" + "\r" + "\n";
                    }
                    else
                    {
                        this.Invoke((Action)(() =>
                        {
                            if (MsgRece == "RD R4601")
                            {
                                if (fDesign.checkBox1.Checked)
                                {
                                    sendMsg = "1" + "\r" + "\n";
                                }
                                else
                                {
                                    sendMsg = "0" + "\r" + "\n";
                                }
                            }
                            else
                            {
                                sendMsg = "NG" + "\r" + "\n";
                            }
                        }));

                        //文字列をByte型配列に変換
                        byte[] sendBytes = enc.GetBytes(sendMsg);
                        //データを送信する
                        NetStm.Write(sendBytes, 0, sendBytes.Length);
                        MsgSend = sendMsg.Replace("\r", "");

                        this.Invoke((Action)(() => { fDesign.richTextBox1.AppendText(sendMsg + "\r\n"); }));
                    }
                }
            }

        }

        private void FormDesignSetting()
        {
            
            this.Name = "form1Control";
            this.Text = "ソケット通信・エコーサーバー・サンプル";
            this.Location = new Point(500, 200);
            this.ClientSize = new Size(684, 761);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1Control_FormClosed);
            this.Load += new System.EventHandler(this.Form1Control_Load);

            Controls.Add(fDesign.richTextBox1);

            Controls.Add(fDesign.buttonConec);
            fDesign.buttonConec.Click += new System.EventHandler(this.ButtonConec_Click);

            Controls.Add(fDesign.buttonCnCancel);
            fDesign.buttonCnCancel.Click += new System.EventHandler(this.ButtonCnCancel_Click);

            Controls.Add(fDesign.buttonAsyncCancel);
            fDesign.buttonAsyncCancel.Click += new System.EventHandler(this.ButtonAsyncCancel_Click);

            Controls.Add(fDesign.buttonExit);
            fDesign.buttonExit.Click += new System.EventHandler(this.ButtonExit_Click);

            Controls.Add(fDesign.labelFoot);

        }
    }
}