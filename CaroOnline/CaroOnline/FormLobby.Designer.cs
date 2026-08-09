namespace CaroOnline
{
    partial class FormLobby
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
            lstOnlinePlayers = new ListBox();
            lstMatchRooms = new ListBox();
            btnSendChallenge = new Button();
            btnWatchMatch = new Button();
            label1 = new Label();
            label2 = new Label();
            SuspendLayout();
            // 
            // lstOnlinePlayers
            // 
            lstOnlinePlayers.BackColor = SystemColors.ActiveCaption;
            lstOnlinePlayers.Cursor = Cursors.IBeam;
            lstOnlinePlayers.FormattingEnabled = true;
            lstOnlinePlayers.Location = new Point(173, 137);
            lstOnlinePlayers.Name = "lstOnlinePlayers";
            lstOnlinePlayers.Size = new Size(210, 184);
            lstOnlinePlayers.TabIndex = 0;
            lstOnlinePlayers.SelectedIndexChanged += lstOnlinePlayers_SelectedIndexChanged;
            // 
            // lstMatchRooms
            // 
            lstMatchRooms.BackColor = SystemColors.ActiveCaption;
            lstMatchRooms.FormattingEnabled = true;
            lstMatchRooms.Location = new Point(468, 137);
            lstMatchRooms.Name = "lstMatchRooms";
            lstMatchRooms.Size = new Size(205, 184);
            lstMatchRooms.TabIndex = 1;
            // 
            // btnSendChallenge
            // 
            btnSendChallenge.Location = new Point(226, 332);
            btnSendChallenge.Name = "btnSendChallenge";
            btnSendChallenge.Size = new Size(121, 29);
            btnSendChallenge.TabIndex = 2;
            btnSendChallenge.Text = "Thách đấu ⚔️";
            btnSendChallenge.UseVisualStyleBackColor = true;
            // 
            // btnWatchMatch
            // 
            btnWatchMatch.Location = new Point(512, 332);
            btnWatchMatch.Name = "btnWatchMatch";
            btnWatchMatch.Size = new Size(117, 29);
            btnWatchMatch.TabIndex = 3;
            btnWatchMatch.Text = "Xem trận 👁️";
            btnWatchMatch.UseVisualStyleBackColor = true;
            btnWatchMatch.Click += btnWatchMatch_Click_1;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(173, 114);
            label1.Name = "label1";
            label1.Size = new Size(193, 20);
            label1.TabIndex = 4;
            label1.Text = "\U0001f7e2 Người chơi đang Online";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(468, 114);
            label2.Name = "label2";
            label2.Size = new Size(205, 20);
            label2.TabIndex = 5;
            label2.Text = "🎮 Các trận đấu đang diễn ra";
            label2.Click += label2_Click;
            // 
            // FormLobby
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(btnWatchMatch);
            Controls.Add(btnSendChallenge);
            Controls.Add(lstMatchRooms);
            Controls.Add(lstOnlinePlayers);
            Name = "FormLobby";
            Text = "Sảnh chờ";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ListBox lstOnlinePlayers;
        private ListBox lstMatchRooms;
        private Button btnSendChallenge;
        private Button btnWatchMatch;
        private Label label1;
        private Label label2;
    }
}