namespace DTU_PLC_Test
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.btnConnect = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.txtIp = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtPort = new System.Windows.Forms.TextBox();
            this.txtMsg = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtMbData = new System.Windows.Forms.TextBox();
            this.btnMbSend = new System.Windows.Forms.Button();
            this.textBox5 = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnStop = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.btnSendLoop = new System.Windows.Forms.Button();
            this.btnSend = new System.Windows.Forms.Button();
            this.cmbData = new System.Windows.Forms.ComboBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.cmbFunc = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.txtLoop = new System.Windows.Forms.TextBox();
            this.txtAddr = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtSlave = new System.Windows.Forms.TextBox();
            this.btnEndLoop = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnConnect
            // 
            this.btnConnect.Location = new System.Drawing.Point(355, 23);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(75, 23);
            this.btnConnect.TabIndex = 0;
            this.btnConnect.Text = "连接";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(18, 29);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(47, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "主站IP:";
            // 
            // txtIp
            // 
            this.txtIp.Location = new System.Drawing.Point(65, 25);
            this.txtIp.Name = "txtIp";
            this.txtIp.Size = new System.Drawing.Size(100, 21);
            this.txtIp.TabIndex = 2;
            this.txtIp.Text = "192.168.12.73";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(192, 29);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 12);
            this.label2.TabIndex = 1;
            this.label2.Text = "端口：";
            // 
            // txtPort
            // 
            this.txtPort.Location = new System.Drawing.Point(239, 25);
            this.txtPort.Name = "txtPort";
            this.txtPort.Size = new System.Drawing.Size(100, 21);
            this.txtPort.TabIndex = 2;
            this.txtPort.Text = "2317";
            // 
            // txtMsg
            // 
            this.txtMsg.Location = new System.Drawing.Point(12, 378);
            this.txtMsg.Multiline = true;
            this.txtMsg.Name = "txtMsg";
            this.txtMsg.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtMsg.Size = new System.Drawing.Size(529, 344);
            this.txtMsg.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(18, 30);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(35, 12);
            this.label3.TabIndex = 1;
            this.label3.Text = "数据:";
            // 
            // txtMbData
            // 
            this.txtMbData.Location = new System.Drawing.Point(65, 26);
            this.txtMbData.Name = "txtMbData";
            this.txtMbData.Size = new System.Drawing.Size(274, 21);
            this.txtMbData.TabIndex = 2;
            // 
            // btnMbSend
            // 
            this.btnMbSend.Location = new System.Drawing.Point(379, 25);
            this.btnMbSend.Name = "btnMbSend";
            this.btnMbSend.Size = new System.Drawing.Size(125, 23);
            this.btnMbSend.TabIndex = 0;
            this.btnMbSend.Text = "modbus格式数据发送";
            this.btnMbSend.UseVisualStyleBackColor = true;
            this.btnMbSend.Click += new System.EventHandler(this.btnMbSend_Click);
            // 
            // textBox5
            // 
            this.textBox5.Location = new System.Drawing.Point(20, 64);
            this.textBox5.Multiline = true;
            this.textBox5.Name = "textBox5";
            this.textBox5.ReadOnly = true;
            this.textBox5.Size = new System.Drawing.Size(484, 80);
            this.textBox5.TabIndex = 3;
            this.textBox5.Text = "060500100000CDB8   地址16写闭线圈\r\n06050010FF008C48   地址16写开线圈\r\n0605001100009C78   地址17" +
    "写闭线圈\r\n06050011FF00DD88   地址17写开线圈\r\n0605001200006C78   地址18写闭线圈\r\n06050012FF002D88" +
    "   地址18写开线圈";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.textBox5);
            this.groupBox1.Controls.Add(this.btnMbSend);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.txtMbData);
            this.groupBox1.Location = new System.Drawing.Point(12, 87);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(529, 164);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "直接发送Modbus数据测试";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btnStop);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.btnConnect);
            this.groupBox2.Controls.Add(this.txtIp);
            this.groupBox2.Controls.Add(this.txtPort);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Location = new System.Drawing.Point(12, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(529, 69);
            this.groupBox2.TabIndex = 5;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "主站设置";
            // 
            // btnStop
            // 
            this.btnStop.Location = new System.Drawing.Point(438, 23);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(75, 23);
            this.btnStop.TabIndex = 6;
            this.btnStop.Text = "关闭";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.btnEndLoop);
            this.groupBox3.Controls.Add(this.btnSendLoop);
            this.groupBox3.Controls.Add(this.btnSend);
            this.groupBox3.Controls.Add(this.cmbData);
            this.groupBox3.Controls.Add(this.label8);
            this.groupBox3.Controls.Add(this.label7);
            this.groupBox3.Controls.Add(this.cmbFunc);
            this.groupBox3.Controls.Add(this.label6);
            this.groupBox3.Controls.Add(this.label5);
            this.groupBox3.Controls.Add(this.txtLoop);
            this.groupBox3.Controls.Add(this.txtAddr);
            this.groupBox3.Controls.Add(this.label4);
            this.groupBox3.Controls.Add(this.txtSlave);
            this.groupBox3.Location = new System.Drawing.Point(12, 257);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(529, 115);
            this.groupBox3.TabIndex = 6;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "通讯设置";
            // 
            // btnSendLoop
            // 
            this.btnSendLoop.Location = new System.Drawing.Point(282, 75);
            this.btnSendLoop.Name = "btnSendLoop";
            this.btnSendLoop.Size = new System.Drawing.Size(75, 23);
            this.btnSendLoop.TabIndex = 5;
            this.btnSendLoop.Text = "循环发送";
            this.btnSendLoop.UseVisualStyleBackColor = true;
            this.btnSendLoop.Click += new System.EventHandler(this.btnSendLoop_Click);
            // 
            // btnSend
            // 
            this.btnSend.Location = new System.Drawing.Point(20, 74);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(163, 23);
            this.btnSend.TabIndex = 5;
            this.btnSend.Text = "发送";
            this.btnSend.UseVisualStyleBackColor = true;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            // 
            // cmbData
            // 
            this.cmbData.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbData.FormattingEnabled = true;
            this.cmbData.Items.AddRange(new object[] {
            "开",
            "闭"});
            this.cmbData.Location = new System.Drawing.Point(436, 31);
            this.cmbData.Name = "cmbData";
            this.cmbData.Size = new System.Drawing.Size(77, 20);
            this.cmbData.TabIndex = 4;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(263, 80);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(17, 12);
            this.label8.TabIndex = 3;
            this.label8.Text = "秒";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(377, 35);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(53, 12);
            this.label7.TabIndex = 3;
            this.label7.Text = "写开闭：";
            // 
            // cmbFunc
            // 
            this.cmbFunc.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbFunc.FormattingEnabled = true;
            this.cmbFunc.Items.AddRange(new object[] {
            "读线圈",
            "写线圈"});
            this.cmbFunc.Location = new System.Drawing.Point(289, 31);
            this.cmbFunc.Name = "cmbFunc";
            this.cmbFunc.Size = new System.Drawing.Size(77, 20);
            this.cmbFunc.TabIndex = 4;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(230, 35);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(53, 12);
            this.label6.TabIndex = 3;
            this.label6.Text = "功能码：";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(112, 35);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(71, 12);
            this.label5.TabIndex = 1;
            this.label5.Text = "寄存器地址:";
            // 
            // txtLoop
            // 
            this.txtLoop.Location = new System.Drawing.Point(237, 76);
            this.txtLoop.Name = "txtLoop";
            this.txtLoop.Size = new System.Drawing.Size(24, 21);
            this.txtLoop.TabIndex = 2;
            this.txtLoop.Text = "2";
            this.txtLoop.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // txtAddr
            // 
            this.txtAddr.Location = new System.Drawing.Point(184, 31);
            this.txtAddr.Name = "txtAddr";
            this.txtAddr.Size = new System.Drawing.Size(38, 21);
            this.txtAddr.TabIndex = 2;
            this.txtAddr.Text = "16";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(18, 35);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(47, 12);
            this.label4.TabIndex = 1;
            this.label4.Text = "从站ID:";
            // 
            // txtSlave
            // 
            this.txtSlave.Location = new System.Drawing.Point(65, 31);
            this.txtSlave.Name = "txtSlave";
            this.txtSlave.Size = new System.Drawing.Size(38, 21);
            this.txtSlave.TabIndex = 2;
            this.txtSlave.Text = "6";
            // 
            // btnEndLoop
            // 
            this.btnEndLoop.Location = new System.Drawing.Point(363, 74);
            this.btnEndLoop.Name = "btnEndLoop";
            this.btnEndLoop.Size = new System.Drawing.Size(75, 23);
            this.btnEndLoop.TabIndex = 6;
            this.btnEndLoop.Text = "停止循环";
            this.btnEndLoop.UseVisualStyleBackColor = true;
            this.btnEndLoop.Click += new System.EventHandler(this.btnEndLoop_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(553, 734);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.txtMsg);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "DTU-PLC远传通讯测试（作主站）";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtIp;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtPort;
        private System.Windows.Forms.TextBox txtMsg;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtMbData;
        private System.Windows.Forms.Button btnMbSend;
        private System.Windows.Forms.TextBox textBox5;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtSlave;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtAddr;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox cmbFunc;
        private System.Windows.Forms.ComboBox cmbData;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.Button btnSendLoop;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txtLoop;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Button btnEndLoop;
    }
}

