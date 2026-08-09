namespace CaroServerAdmin
{
    partial class FrmAdminDashboard
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            grpServerConfig = new GroupBox();
            lblServerStatus = new Label();
            lblStatusTitle = new Label();
            btnStopServer = new Button();
            btnStartServer = new Button();
            txtPort = new TextBox();
            lblPort = new Label();
            txtIP = new TextBox();
            lblIP = new Label();
            grpServerLog = new GroupBox();
            lstServerLog = new ListBox();
            grpServerConfig.SuspendLayout();
            grpServerLog.SuspendLayout();
            SuspendLayout();
            // 
            // grpServerConfig
            // 
            grpServerConfig.Controls.Add(lblServerStatus);
            grpServerConfig.Controls.Add(lblStatusTitle);
            grpServerConfig.Controls.Add(btnStopServer);
            grpServerConfig.Controls.Add(btnStartServer);
            grpServerConfig.Controls.Add(txtPort);
            grpServerConfig.Controls.Add(lblPort);
            grpServerConfig.Controls.Add(txtIP);
            grpServerConfig.Controls.Add(lblIP);
            grpServerConfig.Location = new Point(36, 51);
            grpServerConfig.Name = "grpServerConfig";
            grpServerConfig.Size = new Size(613, 231);
            grpServerConfig.TabIndex = 0;
            grpServerConfig.TabStop = false;
            grpServerConfig.Text = "Server Configuration";
            grpServerConfig.Enter += groupBox1_Enter;
            // 
            // lblServerStatus
            // 
            lblServerStatus.AutoSize = true;
            lblServerStatus.ForeColor = Color.Red;
            lblServerStatus.Location = new Point(164, 130);
            lblServerStatus.Name = "lblServerStatus";
            lblServerStatus.Size = new Size(70, 20);
            lblServerStatus.TabIndex = 7;
            lblServerStatus.Text = "STOPPED";
            // 
            // lblStatusTitle
            // 
            lblStatusTitle.AutoSize = true;
            lblStatusTitle.Location = new Point(49, 130);
            lblStatusTitle.Name = "lblStatusTitle";
            lblStatusTitle.Size = new Size(97, 20);
            lblStatusTitle.TabIndex = 6;
            lblStatusTitle.Text = "Server Status:";
            // 
            // btnStopServer
            // 
            btnStopServer.Enabled = false;
            btnStopServer.Location = new Point(213, 181);
            btnStopServer.Name = "btnStopServer";
            btnStopServer.Size = new Size(94, 29);
            btnStopServer.TabIndex = 5;
            btnStopServer.Text = "STOP SERVER";
            btnStopServer.UseVisualStyleBackColor = true;
            btnStopServer.Click += btnStopServer_Click;
            // 
            // btnStartServer
            // 
            btnStartServer.Location = new Point(49, 181);
            btnStartServer.Name = "btnStartServer";
            btnStartServer.Size = new Size(94, 29);
            btnStartServer.TabIndex = 4;
            btnStartServer.Text = "START SERVER";
            btnStartServer.UseVisualStyleBackColor = true;
            btnStartServer.Click += btnStartServer_Click;
            // 
            // txtPort
            // 
            txtPort.Location = new Point(136, 84);
            txtPort.Name = "txtPort";
            txtPort.Size = new Size(125, 27);
            txtPort.TabIndex = 3;
            txtPort.Text = "8888";
            // 
            // lblPort
            // 
            lblPort.AutoSize = true;
            lblPort.Location = new Point(49, 87);
            lblPort.Name = "lblPort";
            lblPort.Size = new Size(38, 20);
            lblPort.TabIndex = 2;
            lblPort.Text = "Port:";
            // 
            // txtIP
            // 
            txtIP.Location = new Point(136, 42);
            txtIP.Name = "txtIP";
            txtIP.Size = new Size(125, 27);
            txtIP.TabIndex = 1;
            txtIP.Text = "127.0.0.1";
            // 
            // lblIP
            // 
            lblIP.AutoSize = true;
            lblIP.Location = new Point(49, 45);
            lblIP.Name = "lblIP";
            lblIP.Size = new Size(81, 20);
            lblIP.TabIndex = 0;
            lblIP.Text = "IP Address:";
            // 
            // grpServerLog
            // 
            grpServerLog.Controls.Add(lstServerLog);
            grpServerLog.Location = new Point(36, 297);
            grpServerLog.Name = "grpServerLog";
            grpServerLog.Size = new Size(700, 230);
            grpServerLog.TabIndex = 8;
            grpServerLog.TabStop = false;
            grpServerLog.Text = "Server Log";
            // 
            // lstServerLog
            // 
            lstServerLog.FormattingEnabled = true;
            lstServerLog.Location = new Point(6, 26);
            lstServerLog.Name = "lstServerLog";
            lstServerLog.Size = new Size(498, 164);
            lstServerLog.TabIndex = 0;
            // 
            // FrmAdminDashboard
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1082, 653);
            Controls.Add(grpServerLog);
            Controls.Add(grpServerConfig);
            MinimumSize = new Size(900, 600);
            Name = "FrmAdminDashboard";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "CARO SERVER - ADMIN DASHBOARD";
            grpServerConfig.ResumeLayout(false);
            grpServerConfig.PerformLayout();
            grpServerLog.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private GroupBox grpServerConfig;
        private Label lblIP;
        private TextBox txtPort;
        private Label lblPort;
        private TextBox txtIP;
        private Button btnStopServer;
        private Button btnStartServer;
        private Label lblServerStatus;
        private Label lblStatusTitle;
        private GroupBox grpServerLog;
        private ListBox lstServerLog;
    }
}
