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
            components = new System.ComponentModel.Container();
            grpServerConfig = new GroupBox();
            grpTestTools = new GroupBox();
            btnDisconnectTestClient = new Button();
            btnConnectTestClient = new Button();
            grpStatistics = new GroupBox();
            lblTotalConnections = new Label();
            lblTotalTitle = new Label();
            lblClientsOnline = new Label();
            lblClientsTitle = new Label();
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
            timerStatistics = new System.Windows.Forms.Timer(components);
            grpServerConfig.SuspendLayout();
            grpTestTools.SuspendLayout();
            grpStatistics.SuspendLayout();
            grpServerLog.SuspendLayout();
            SuspendLayout();
            // 
            // grpServerConfig
            // 
            grpServerConfig.Controls.Add(grpTestTools);
            grpServerConfig.Controls.Add(grpStatistics);
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
            grpServerConfig.Size = new Size(896, 461);
            grpServerConfig.TabIndex = 0;
            grpServerConfig.TabStop = false;
            grpServerConfig.Text = "Server Configuration";
            grpServerConfig.Enter += groupBox1_Enter;
            // 
            // grpTestTools
            // 
            grpTestTools.Controls.Add(btnDisconnectTestClient);
            grpTestTools.Controls.Add(btnConnectTestClient);
            grpTestTools.Location = new Point(485, 233);
            grpTestTools.Name = "grpTestTools";
            grpTestTools.Size = new Size(400, 180);
            grpTestTools.TabIndex = 9;
            grpTestTools.TabStop = false;
            grpTestTools.Text = "Test Tools";
            // 
            // btnDisconnectTestClient
            // 
            btnDisconnectTestClient.Enabled = false;
            btnDisconnectTestClient.Location = new Point(6, 109);
            btnDisconnectTestClient.Name = "btnDisconnectTestClient";
            btnDisconnectTestClient.Size = new Size(94, 29);
            btnDisconnectTestClient.TabIndex = 1;
            btnDisconnectTestClient.Text = "DISCONNECT TEST CLIENT";
            btnDisconnectTestClient.UseVisualStyleBackColor = true;
            btnDisconnectTestClient.Click += btnDisconnectTestClient_Click;
            // 
            // btnConnectTestClient
            // 
            btnConnectTestClient.Location = new Point(6, 46);
            btnConnectTestClient.Name = "btnConnectTestClient";
            btnConnectTestClient.Size = new Size(94, 29);
            btnConnectTestClient.TabIndex = 0;
            btnConnectTestClient.Text = "CONNECT TEST CLIENT";
            btnConnectTestClient.UseVisualStyleBackColor = true;
            btnConnectTestClient.Click += btnConnectTestClient_Click;
            // 
            // grpStatistics
            // 
            grpStatistics.Controls.Add(lblTotalConnections);
            grpStatistics.Controls.Add(lblTotalTitle);
            grpStatistics.Controls.Add(lblClientsOnline);
            grpStatistics.Controls.Add(lblClientsTitle);
            grpStatistics.Location = new Point(485, 17);
            grpStatistics.Name = "grpStatistics";
            grpStatistics.Size = new Size(300, 180);
            grpStatistics.TabIndex = 8;
            grpStatistics.TabStop = false;
            grpStatistics.Text = "Connection Statistics";
            // 
            // lblTotalConnections
            // 
            lblTotalConnections.AutoSize = true;
            lblTotalConnections.Location = new Point(181, 75);
            lblTotalConnections.Name = "lblTotalConnections";
            lblTotalConnections.Size = new Size(17, 20);
            lblTotalConnections.TabIndex = 3;
            lblTotalConnections.Text = "0";
            // 
            // lblTotalTitle
            // 
            lblTotalTitle.AutoSize = true;
            lblTotalTitle.Location = new Point(26, 75);
            lblTotalTitle.Name = "lblTotalTitle";
            lblTotalTitle.Size = new Size(130, 20);
            lblTotalTitle.TabIndex = 2;
            lblTotalTitle.Text = "Total Connections:";
            // 
            // lblClientsOnline
            // 
            lblClientsOnline.AutoSize = true;
            lblClientsOnline.Location = new Point(181, 32);
            lblClientsOnline.Name = "lblClientsOnline";
            lblClientsOnline.Size = new Size(17, 20);
            lblClientsOnline.TabIndex = 1;
            lblClientsOnline.Text = "0";
            // 
            // lblClientsTitle
            // 
            lblClientsTitle.AutoSize = true;
            lblClientsTitle.Location = new Point(26, 32);
            lblClientsTitle.Name = "lblClientsTitle";
            lblClientsTitle.Size = new Size(103, 20);
            lblClientsTitle.TabIndex = 0;
            lblClientsTitle.Text = "Clients Online:";
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
            grpServerLog.Location = new Point(36, 534);
            grpServerLog.Name = "grpServerLog";
            grpServerLog.Size = new Size(785, 230);
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
            // timerStatistics
            // 
            timerStatistics.Enabled = true;
            timerStatistics.Interval = 500;
            timerStatistics.Tick += timerStatistics_Tick;
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
            grpTestTools.ResumeLayout(false);
            grpStatistics.ResumeLayout(false);
            grpStatistics.PerformLayout();
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
        private GroupBox grpStatistics;
        private Label lblClientsTitle;
        private Label lblTotalConnections;
        private Label lblTotalTitle;
        private Label lblClientsOnline;
        private System.Windows.Forms.Timer timerStatistics;
        private GroupBox grpTestTools;
        private Button btnDisconnectTestClient;
        private Button btnConnectTestClient;
    }
}
