namespace GameOptimizer

{
    partial class GameOptimizerForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GameOptimizerForm));
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.selectPath = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.rdoHalfPhysical = new System.Windows.Forms.RadioButton();
            this.rdoAuto = new System.Windows.Forms.RadioButton();
            this.rdoNoSMT = new System.Windows.Forms.RadioButton();
            this.cmbxAffinityCount = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.lblStandByRAM = new System.Windows.Forms.Label();
            this.tmrRam = new System.Windows.Forms.Timer(this.components);
            this.label2 = new System.Windows.Forms.Label();
            this.lblrestmr = new System.Windows.Forms.Label();
            this.btnTmrResTime = new System.Windows.Forms.Button();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.chckbxNaggleAlgo = new System.Windows.Forms.CheckBox();
            this.chckBoxGamePriority = new System.Windows.Forms.CheckBox();
            this.chckbxTimerRes = new System.Windows.Forms.CheckBox();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.chckboxPriority = new System.Windows.Forms.CheckBox();
            this.tmrPriority = new System.Windows.Forms.Timer(this.components);
            this.btnProcess = new System.Windows.Forms.Button();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.btnKill = new System.Windows.Forms.Button();
            this.enableClnStandby = new System.Windows.Forms.CheckBox();
            this.tmrResSet = new System.Windows.Forms.Timer(this.components);
            this.btnClearWorkingSet = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.btnRenewIP = new System.Windows.Forms.Button();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.lblInternet = new System.Windows.Forms.Label();
            this.timer = new System.Windows.Forms.Timer(this.components);
            this.lblDlSpeed = new System.Windows.Forms.Label();
            this.lblRamUsage = new System.Windows.Forms.Label();
            this.nudAutRamFreeup = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudAutRamFreeup)).BeginInit();
            this.SuspendLayout();
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(30, 12);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(464, 20);
            this.textBox1.TabIndex = 0;
            // 
            // selectPath
            // 
            this.selectPath.Location = new System.Drawing.Point(30, 42);
            this.selectPath.Name = "selectPath";
            this.selectPath.Size = new System.Drawing.Size(85, 26);
            this.selectPath.TabIndex = 1;
            this.selectPath.Text = "Select Game";
            this.selectPath.UseVisualStyleBackColor = true;
            this.selectPath.Click += new System.EventHandler(this.SelectPath_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // rdoHalfPhysical
            // 
            this.rdoHalfPhysical.AutoSize = true;
            this.rdoHalfPhysical.Location = new System.Drawing.Point(338, 92);
            this.rdoHalfPhysical.Name = "rdoHalfPhysical";
            this.rdoHalfPhysical.Size = new System.Drawing.Size(133, 19);
            this.rdoHalfPhysical.TabIndex = 2;
            this.rdoHalfPhysical.Text = "Half Physical Cores";
            this.rdoHalfPhysical.UseVisualStyleBackColor = true;
            // 
            // rdoAuto
            // 
            this.rdoAuto.AutoSize = true;
            this.rdoAuto.Checked = true;
            this.rdoAuto.Location = new System.Drawing.Point(338, 46);
            this.rdoAuto.Name = "rdoAuto";
            this.rdoAuto.Size = new System.Drawing.Size(82, 19);
            this.rdoAuto.TabIndex = 3;
            this.rdoAuto.TabStop = true;
            this.rdoAuto.Text = "Automatic";
            this.rdoAuto.UseVisualStyleBackColor = true;
            // 
            // rdoNoSMT
            // 
            this.rdoNoSMT.AutoSize = true;
            this.rdoNoSMT.Location = new System.Drawing.Point(338, 69);
            this.rdoNoSMT.Name = "rdoNoSMT";
            this.rdoNoSMT.Size = new System.Drawing.Size(135, 19);
            this.rdoNoSMT.TabIndex = 4;
            this.rdoNoSMT.Text = "Physical Cores Only";
            this.rdoNoSMT.UseVisualStyleBackColor = true;
            // 
            // cmbxAffinityCount
            // 
            this.cmbxAffinityCount.FormattingEnabled = true;
            this.cmbxAffinityCount.Items.AddRange(new object[] {
            "-",
            "2c/2t",
            "2c/4t",
            "4c/4t",
            "4c/8t",
            "8c/8t",
            "8c/16t"});
            this.cmbxAffinityCount.Location = new System.Drawing.Point(168, 42);
            this.cmbxAffinityCount.Name = "cmbxAffinityCount";
            this.cmbxAffinityCount.Size = new System.Drawing.Size(121, 21);
            this.cmbxAffinityCount.TabIndex = 5;
            this.cmbxAffinityCount.SelectedIndexChanged += new System.EventHandler(this.CmbxTime_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 114);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(150, 15);
            this.label1.TabIndex = 7;
            this.label1.Text = "RAM Standby List cleared:";
            // 
            // lblStandByRAM
            // 
            this.lblStandByRAM.AutoSize = true;
            this.lblStandByRAM.Location = new System.Drawing.Point(138, 114);
            this.lblStandByRAM.Name = "lblStandByRAM";
            this.lblStandByRAM.Size = new System.Drawing.Size(11, 15);
            this.lblStandByRAM.TabIndex = 8;
            this.lblStandByRAM.Text = "-";
            // 
            // tmrRam
            // 
            this.tmrRam.Enabled = true;
            this.tmrRam.Interval = 900000;
            this.tmrRam.Tick += new System.EventHandler(this.tmrRam_Tick);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(500, 12);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(82, 15);
            this.label2.TabIndex = 9;
            this.label2.Text = "Current Timer";
            // 
            // lblrestmr
            // 
            this.lblrestmr.AutoSize = true;
            this.lblrestmr.Location = new System.Drawing.Point(576, 12);
            this.lblrestmr.Name = "lblrestmr";
            this.lblrestmr.Size = new System.Drawing.Size(11, 15);
            this.lblrestmr.TabIndex = 10;
            this.lblrestmr.Text = "-";
            // 
            // btnTmrResTime
            // 
            this.btnTmrResTime.Location = new System.Drawing.Point(570, 31);
            this.btnTmrResTime.Name = "btnTmrResTime";
            this.btnTmrResTime.Size = new System.Drawing.Size(61, 23);
            this.btnTmrResTime.TabIndex = 12;
            this.btnTmrResTime.Text = "Set";
            this.btnTmrResTime.UseVisualStyleBackColor = true;
            this.btnTmrResTime.Click += new System.EventHandler(this.btnTmrResTime_Click);
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(501, 31);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(63, 20);
            this.textBox2.TabIndex = 13;
            this.textBox2.Text = "0.5";
            // 
            // chckbxNaggleAlgo
            // 
            this.chckbxNaggleAlgo.AutoSize = true;
            this.chckbxNaggleAlgo.Location = new System.Drawing.Point(501, 91);
            this.chckbxNaggleAlgo.Name = "chckbxNaggleAlgo";
            this.chckbxNaggleAlgo.Size = new System.Drawing.Size(169, 19);
            this.chckbxNaggleAlgo.TabIndex = 14;
            this.chckbxNaggleAlgo.Text = "Disable Naggle Algorithm";
            this.chckbxNaggleAlgo.UseVisualStyleBackColor = true;
            this.chckbxNaggleAlgo.CheckedChanged += new System.EventHandler(this.chckbxNaggleAlgo_CheckedChanged);
            // 
            // chckBoxGamePriority
            // 
            this.chckBoxGamePriority.AutoSize = true;
            this.chckBoxGamePriority.Location = new System.Drawing.Point(501, 110);
            this.chckBoxGamePriority.Name = "chckBoxGamePriority";
            this.chckBoxGamePriority.Size = new System.Drawing.Size(213, 19);
            this.chckBoxGamePriority.TabIndex = 15;
            this.chckBoxGamePriority.Text = "Increase Game Priority In Registry";
            this.chckBoxGamePriority.UseVisualStyleBackColor = true;
            this.chckBoxGamePriority.CheckedChanged += new System.EventHandler(this.chckBoxGamePriority_CheckedChanged);
            // 
            // chckbxTimerRes
            // 
            this.chckbxTimerRes.AutoSize = true;
            this.chckbxTimerRes.Location = new System.Drawing.Point(501, 57);
            this.chckbxTimerRes.Name = "chckbxTimerRes";
            this.chckbxTimerRes.Size = new System.Drawing.Size(204, 19);
            this.chckbxTimerRes.TabIndex = 16;
            this.chckbxTimerRes.Text = "Set Timer Resolution Every 15m";
            this.chckbxTimerRes.UseVisualStyleBackColor = true;
            this.chckbxTimerRes.CheckedChanged += new System.EventHandler(this.ChckbxTimerRes_CheckedChanged);
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Items.AddRange(new object[] {
            "-",
            "NewReno",
            "ctcp",
            "dctcp",
            "cubic"});
            this.comboBox1.Location = new System.Drawing.Point(501, 133);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(121, 21);
            this.comboBox1.TabIndex = 17;
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.ComboBox1_SelectedIndexChanged);
            // 
            // chckboxPriority
            // 
            this.chckboxPriority.AutoSize = true;
            this.chckboxPriority.Location = new System.Drawing.Point(30, 75);
            this.chckboxPriority.Name = "chckboxPriority";
            this.chckboxPriority.Size = new System.Drawing.Size(175, 19);
            this.chckboxPriority.TabIndex = 18;
            this.chckboxPriority.Text = "Set Priority every 5 minutes";
            this.chckboxPriority.UseVisualStyleBackColor = true;
            this.chckboxPriority.CheckedChanged += new System.EventHandler(this.ChckboxPriority_CheckedChanged);
            // 
            // tmrPriority
            // 
            this.tmrPriority.Interval = 300000;
            this.tmrPriority.Tick += new System.EventHandler(this.TmrPriority_Tick);
            // 
            // btnProcess
            // 
            this.btnProcess.Location = new System.Drawing.Point(30, 158);
            this.btnProcess.Name = "btnProcess";
            this.btnProcess.Size = new System.Drawing.Size(75, 23);
            this.btnProcess.TabIndex = 19;
            this.btnProcess.Text = "Process List";
            this.btnProcess.UseVisualStyleBackColor = true;
            this.btnProcess.Click += new System.EventHandler(this.btnProcess_Click);
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Enabled = false;
            this.dataGridView1.Location = new System.Drawing.Point(30, 231);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowHeadersWidth = 51;
            this.dataGridView1.Size = new System.Drawing.Size(464, 213);
            this.dataGridView1.TabIndex = 20;
            this.dataGridView1.Visible = false;
            // 
            // btnKill
            // 
            this.btnKill.Location = new System.Drawing.Point(30, 450);
            this.btnKill.Name = "btnKill";
            this.btnKill.Size = new System.Drawing.Size(75, 23);
            this.btnKill.TabIndex = 21;
            this.btnKill.Text = "Kill Process";
            this.btnKill.UseVisualStyleBackColor = true;
            this.btnKill.Click += new System.EventHandler(this.btnKill_Click);
            // 
            // enableClnStandby
            // 
            this.enableClnStandby.AutoSize = true;
            this.enableClnStandby.Checked = true;
            this.enableClnStandby.CheckState = System.Windows.Forms.CheckState.Checked;
            this.enableClnStandby.Location = new System.Drawing.Point(15, 130);
            this.enableClnStandby.Name = "enableClnStandby";
            this.enableClnStandby.Size = new System.Drawing.Size(250, 19);
            this.enableClnStandby.TabIndex = 22;
            this.enableClnStandby.Text = "Enable automatic RAM StandBy clearing";
            this.enableClnStandby.UseVisualStyleBackColor = true;
            this.enableClnStandby.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // tmrResSet
            // 
            this.tmrResSet.Tick += new System.EventHandler(this.tmrResSet_Tick);
            // 
            // btnClearWorkingSet
            // 
            this.btnClearWorkingSet.Location = new System.Drawing.Point(226, 155);
            this.btnClearWorkingSet.Name = "btnClearWorkingSet";
            this.btnClearWorkingSet.Size = new System.Drawing.Size(113, 25);
            this.btnClearWorkingSet.TabIndex = 23;
            this.btnClearWorkingSet.Text = "Clr WorkingSet";
            this.btnClearWorkingSet.UseVisualStyleBackColor = true;
            this.btnClearWorkingSet.Click += new System.EventHandler(this.btnClearWorkingSet_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(118, 155);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(103, 25);
            this.button1.TabIndex = 24;
            this.button1.Text = "Clr StandBy RAM";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // btnRenewIP
            // 
            this.btnRenewIP.Location = new System.Drawing.Point(418, 186);
            this.btnRenewIP.Name = "btnRenewIP";
            this.btnRenewIP.Size = new System.Drawing.Size(75, 23);
            this.btnRenewIP.TabIndex = 25;
            this.btnRenewIP.Text = "Renew IP";
            this.btnRenewIP.UseVisualStyleBackColor = true;
            this.btnRenewIP.Click += new System.EventHandler(this.btnRenewIP_Click);
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.notifyIcon1.BalloonTipText = "Process is running on Background";
            this.notifyIcon1.BalloonTipTitle = "You can restore the window double-clicking the icon on the tray bar";
            this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
            this.notifyIcon1.Text = "notifyIcon1";
            this.notifyIcon1.Visible = true;
            this.notifyIcon1.DoubleClick += new System.EventHandler(this.notifyIcon1_DoubleClick);
            // 
            // lblInternet
            // 
            this.lblInternet.AutoSize = true;
            this.lblInternet.Location = new System.Drawing.Point(508, 167);
            this.lblInternet.Name = "lblInternet";
            this.lblInternet.Size = new System.Drawing.Size(11, 15);
            this.lblInternet.TabIndex = 26;
            this.lblInternet.Text = "-";
            // 
            // lblDlSpeed
            // 
            this.lblDlSpeed.AutoSize = true;
            this.lblDlSpeed.Location = new System.Drawing.Point(508, 195);
            this.lblDlSpeed.Name = "lblDlSpeed";
            this.lblDlSpeed.Size = new System.Drawing.Size(11, 15);
            this.lblDlSpeed.TabIndex = 27;
            this.lblDlSpeed.Text = "-";
            // 
            // lblRamUsage
            // 
            this.lblRamUsage.AutoSize = true;
            this.lblRamUsage.Location = new System.Drawing.Point(123, 198);
            this.lblRamUsage.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblRamUsage.Name = "lblRamUsage";
            this.lblRamUsage.Size = new System.Drawing.Size(11, 15);
            this.lblRamUsage.TabIndex = 28;
            this.lblRamUsage.Text = "-";
            // 
            // nudAutRamFreeup
            // 
            this.nudAutRamFreeup.Location = new System.Drawing.Point(30, 193);
            this.nudAutRamFreeup.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.nudAutRamFreeup.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.nudAutRamFreeup.Name = "nudAutRamFreeup";
            this.nudAutRamFreeup.Size = new System.Drawing.Size(75, 20);
            this.nudAutRamFreeup.TabIndex = 29;
            // 
            // GameOptimizerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(689, 219);
            this.Controls.Add(this.nudAutRamFreeup);
            this.Controls.Add(this.lblRamUsage);
            this.Controls.Add(this.lblDlSpeed);
            this.Controls.Add(this.lblInternet);
            this.Controls.Add(this.btnRenewIP);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.btnClearWorkingSet);
            this.Controls.Add(this.enableClnStandby);
            this.Controls.Add(this.btnKill);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.btnProcess);
            this.Controls.Add(this.chckboxPriority);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.chckbxTimerRes);
            this.Controls.Add(this.chckBoxGamePriority);
            this.Controls.Add(this.chckbxNaggleAlgo);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.btnTmrResTime);
            this.Controls.Add(this.lblrestmr);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.lblStandByRAM);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cmbxAffinityCount);
            this.Controls.Add(this.rdoNoSMT);
            this.Controls.Add(this.rdoAuto);
            this.Controls.Add(this.rdoHalfPhysical);
            this.Controls.Add(this.selectPath);
            this.Controls.Add(this.textBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "GameOptimizerForm";
            this.Text = "Game Optimizer";
            this.TransparencyKey = System.Drawing.SystemColors.WindowFrame;
            this.Resize += new System.EventHandler(this.Form1_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudAutRamFreeup)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button selectPath;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.RadioButton rdoHalfPhysical;
        private System.Windows.Forms.RadioButton rdoAuto;
        private System.Windows.Forms.RadioButton rdoNoSMT;
        private System.Windows.Forms.ComboBox cmbxAffinityCount;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblStandByRAM;
        private System.Windows.Forms.Timer tmrRam;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblrestmr;
        private System.Windows.Forms.Button btnTmrResTime;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.CheckBox chckbxNaggleAlgo;
        private System.Windows.Forms.CheckBox chckBoxGamePriority;
        private System.Windows.Forms.CheckBox chckbxTimerRes;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.CheckBox chckboxPriority;
        private System.Windows.Forms.Timer tmrPriority;
        private System.Windows.Forms.Button btnProcess;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Button btnKill;
        private System.Windows.Forms.CheckBox enableClnStandby;
        private System.Windows.Forms.Timer tmrResSet;
        private System.Windows.Forms.Button btnClearWorkingSet;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button btnRenewIP;
        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.Label lblInternet;
        private System.Windows.Forms.Timer timer;
        private System.Windows.Forms.Label lblDlSpeed;
        private System.Windows.Forms.Label lblRamUsage;
        private System.Windows.Forms.NumericUpDown nudAutRamFreeup;
    }
}

