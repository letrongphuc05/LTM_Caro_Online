namespace CaroOnline
{
    partial class FormConnect
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormConnect));
            txtUsername = new TextBox();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            label4 = new Label();
            btnConnect = new Button();
            txtPort = new ComboBox();
            txtIP = new ComboBox();
            SuspendLayout();
            // 
            // txtUsername
            // 
            txtUsername.Location = new Point(335, 240);
            txtUsername.Name = "txtUsername";
            txtUsername.Size = new Size(187, 27);
            txtUsername.TabIndex = 2;
            txtUsername.TextChanged += txtUsername_TextChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.BackColor = Color.Transparent;
            label1.Font = new Font("Showcard Gothic", 18F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label1.ForeColor = Color.Coral;
            label1.Location = new Point(249, 47);
            label1.Name = "label1";
            label1.Size = new Size(293, 37);
            label1.TabIndex = 3;
            label1.Text = "GAME CARO ONLINE";
            label1.Click += label1_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.BackColor = Color.Transparent;
            label2.Font = new Font("Segoe UI Black", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label2.ForeColor = Color.White;
            label2.Location = new Point(203, 161);
            label2.Name = "label2";
            label2.Size = new Size(82, 20);
            label2.TabIndex = 4;
            label2.Text = "Địa chỉ IP:";
            label2.Click += label2_Click;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.BackColor = Color.Transparent;
            label3.Font = new Font("Segoe UI Black", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label3.ForeColor = Color.White;
            label3.Location = new Point(203, 205);
            label3.Name = "label3";
            label3.Size = new Size(101, 20);
            label3.TabIndex = 5;
            label3.Text = "Port (Cổng):";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.BackColor = Color.Transparent;
            label4.Font = new Font("Segoe UI Black", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label4.ForeColor = Color.White;
            label4.Location = new Point(203, 247);
            label4.Name = "label4";
            label4.Size = new Size(125, 20);
            label4.TabIndex = 6;
            label4.Text = "Tên người chơi:";
            // 
            // btnConnect
            // 
            btnConnect.BackColor = Color.Transparent;
            btnConnect.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnConnect.Location = new Point(280, 326);
            btnConnect.Name = "btnConnect";
            btnConnect.Size = new Size(213, 41);
            btnConnect.TabIndex = 7;
            btnConnect.Text = "Kết Nối Trực Tuyến 🚀";
            btnConnect.TextImageRelation = TextImageRelation.ImageAboveText;
            btnConnect.UseVisualStyleBackColor = false;
            btnConnect.Click += btnConnect_Click;
            // 
            // txtPort
            // 
            txtPort.FormattingEnabled = true;
            txtPort.Items.AddRange(new object[] { "9999", "8080", "8888" });
            txtPort.Location = new Point(312, 197);
            txtPort.Name = "txtPort";
            txtPort.Size = new Size(210, 28);
            txtPort.TabIndex = 8;
            // 
            // txtIP
            // 
            txtIP.FormattingEnabled = true;
            txtIP.Items.AddRange(new object[] { "127.0.0.1", "192.168.1.10", "192.168.1.100" });
            txtIP.Location = new Point(291, 153);
            txtIP.Name = "txtIP";
            txtIP.Size = new Size(231, 28);
            txtIP.TabIndex = 9;
            txtIP.SelectedIndexChanged += txtIP_SelectedIndexChanged;
            // 
            // FormConnect
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImage = (Image)resources.GetObject("$this.BackgroundImage");
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(800, 450);
            Controls.Add(txtIP);
            Controls.Add(txtPort);
            Controls.Add(btnConnect);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(txtUsername);
            Name = "FormConnect";
            Text = "FormConnect";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private TextBox txtUsername;
        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private Button btnConnect;
        private ComboBox txtPort;
        private ComboBox txtIP;
    }
}