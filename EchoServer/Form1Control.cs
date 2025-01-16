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
            fDesign.richTextBox1.AppendText("�N���C�A���g�ڑ��{�^���������N���C�A���g�ڑ���L���ɂ��ĉ������B" + "\r\n");
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
            fDesign.richTextBox1.AppendText("�T�[�o�[�̓N���C�A���g�̐ڑ���҂��Ă��܂��B" + "\r\n");

            await ClientMatchi();
            if (EndFlag)
            {
                IsClientCn = false;
                fDesign.richTextBox1.AppendText("�N���C�A���g�ڑ����L�����Z�����܂����B" + "\r\n");
                return;
            }
            else
            {
                IsClientCn = true;
                fDesign.richTextBox1.AppendText("�N���C�A���g���ڑ�����܂����B" + "\r\n");
            }

            NetStm = UserClient.GetStream();
            fDesign.buttonCnCancel.Enabled = false;
            fDesign.buttonAsyncCancel.Enabled = true;
            fDesign.richTextBox1.AppendText("�N���C�A���g����̓d����ҋ@���Ă��܂��B" + "\r\n");

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
                fDesign.richTextBox1.AppendText("����M�������I�����܂����B" + "\r\n");
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
            //�񓯊��������L�����Z�����܂��B
            cts.Cancel();
        }

        //�񓯊��Őڑ��҂�
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
                    //SvrListener.Stop()���\�b�h�����s���ꂽ���O����������B
                    this.Invoke((Action)(() => { fDesign.richTextBox1.AppendText(ex.Message + "\r\n"); }));
                }
            });
            await tk1;
        }

        //�񓯊��d������M
        private async void RecSend(System.Threading.CancellationToken ct)
        {
            //�N���C�A���g���瑗��ꂽ�f�[�^����M����
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
                        //cts.Cancel()���\�b�h�����s���ꂽ���O����������B
                        this.Invoke((Action)(() =>
                        {
                            fDesign.buttonConec.Enabled = true;
                            fDesign.buttonAsyncCancel.Enabled = false;
                            fDesign.richTextBox1.AppendText("����M�������I�����܂����B2" + "\r\n");
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
                //�f�[�^�̈ꕔ����M����B
                //NetworkStream.Read()���\�b�h�����s���ēǂݎ���f�[�^�������ꍇ(����0��Ԃ��Ȃ��l��)�����ŃX���b�h�̓u���b�N�����B
                //��������s����O�Ƀf�[�^�̎�M��1�ȏ�ɂȂ�܂őҋ@���Ă�����s���鎖�B
                //���̃u���b�N��Networkstream.ReadTimeout�̐ݒ�l�ɂ�茈�܂邪����l��-1(����)�Ȃ̂Ńf�[�^��������܂Ńu���b�N�����B
                //DataAvailable�v���p�e�B��True(��M�o�b�t�@�L��)���`�F�b�N���Ă���Ȃ�ReadTimeout�̃^�C���A�E�g�͎����㔭�����Ȃ��B
                //ReadTimeout�v���p�e�B��Networkstream.Read()���\�b�h�����s������A�ǂݎ��\�f�[�^������܂ł̎��Ԃ̗l���B
                int resSize = NetStm.Read(resBytes, 0, resBytes.Length);
                //Read��0��Ԃ������̓N���C�A���g���ؒf�����Ɣ��f
                if (resSize == 0)
                {
                    disconnected = true;
                    return;
                }
                //��M�����f�[�^��~�ς���
                ms.Write(resBytes, 0, resSize);


                //��M�����f�[�^�𕶎���ɕϊ�
                string resMsg = enc.GetString(ms.ToArray());
                ms.Close();
                MsgRece = resMsg.Replace("\r", "");
                this.Invoke((Action)(() => { fDesign.richTextBox1.AppendText(MsgRece + "\r\n"); }));

                if (!disconnected)
                {
                    //�N���C�A���g�Ƀf�[�^�𑗐M����
                    //�N���C�A���g�ɑ��M���镶������쐬
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

                        //�������Byte�^�z��ɕϊ�
                        byte[] sendBytes = enc.GetBytes(sendMsg);
                        //�f�[�^�𑗐M����
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
            this.Text = "�\�P�b�g�ʐM�E�G�R�[�T�[�o�[�E�T���v��";
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