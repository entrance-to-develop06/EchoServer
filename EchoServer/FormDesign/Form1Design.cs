using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EchoServer.FormDesign
{
    public class Form1Design
    {
        internal Dictionary<String, Label> labelDic = new Dictionary<String, Label>();
        internal RichTextBox richTextBox1;
        internal Button buttonConec;
        internal Button buttonCnCancel;
        internal Button buttonAsyncCancel;
        internal Button buttonExit;
        internal CheckBox checkBox1;
        internal Label labelFoot;

        private Label LabelsSetting(String name, String txt, int x, int y, int w, int h)
        {
            var label = new Label();
            label.Name = name;
            label.AutoSize = false;
            label.Text = txt;
            label.Location = new Point(x, y);
            label.Size = new Size(w, h);
            labelDic.Add(label.Name, label);
            //Controls.Add(label);

            return label;
        }

        private Control ControlsSetting(System.Windows.Forms.Control ctl, String name, int x, int y, int w, int h)
        {
            ctl.Name = name;
            ctl.Location = new Point(x, y);
            ctl.Size = new Size(w, h);
            //Controls.Add(ctl);

            return ctl;
        }

        internal void Setting()
        {
            this.richTextBox1 = new RichTextBox();
            richTextBox1.ReadOnly = true;
            richTextBox1.TabIndex = 0;
            richTextBox1 = (RichTextBox)(ControlsSetting(richTextBox1, "richTextBox1", 15, 30, 350, 710));

            this.buttonConec = new Button();
            buttonConec.Text = "クライアント接続";
            buttonConec.TabIndex = 2;
            buttonConec.UseVisualStyleBackColor = true;
            buttonConec = (Button)(ControlsSetting(buttonConec, "buttonConec", 390, 30, 115, 40));

            this.buttonCnCancel = new Button();
            buttonCnCancel.Text = "クライアント接続キャンセル";
            buttonCnCancel.TabIndex = 3;
            buttonCnCancel.UseVisualStyleBackColor = true;
            buttonCnCancel = (Button)(ControlsSetting(buttonCnCancel, "buttonCnCancel", 520, 30, 145, 40));

            this.buttonAsyncCancel = new Button();
            buttonAsyncCancel.Text = "通信強制終了";
            buttonAsyncCancel.TabIndex = 4;
            buttonAsyncCancel.UseVisualStyleBackColor = true;
            buttonAsyncCancel = (Button)(ControlsSetting(buttonAsyncCancel, "buttonAsyncCancel", 390, 85, 115, 40));

            this.buttonExit = new Button();
            buttonExit.Text = "閉じる";
            buttonExit.TabIndex = 5;
            buttonExit.UseVisualStyleBackColor = true;
            buttonExit = (Button)(ControlsSetting(buttonExit, "buttonExit", 550, 85, 115, 40));

            this.checkBox1 = new CheckBox();
            checkBox1.Text = "サーバーの状態１";
            checkBox1.TabIndex = 6;
            checkBox1.AutoSize = false;
            checkBox1 = (CheckBox)(ControlsSetting(checkBox1, "checkBox1", 390, 150, 108, 19));

            labelFoot = new Label();
            labelFoot.AutoSize = false;
            labelFoot.Text = "Copyright (c)  2021-2025  entrance-to-develop06";
            labelFoot = (Label)(ControlsSetting(labelFoot, "LabelFoot", 30, 740, 400, 19));
        }
    }
}