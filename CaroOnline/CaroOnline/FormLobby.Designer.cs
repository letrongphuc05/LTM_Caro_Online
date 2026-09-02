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
            lblStatus = new Label();
            SuspendLayout();
            // 
            // lblStatus
            // 
            lblStatus.BackColor = Color.Transparent; // Trong suốt để lộ hiệu ứng hạt X O rơi
            lblStatus.Dock = DockStyle.Fill; // Bám sát toàn bộ màn hình
            lblStatus.Font = new Font("Segoe UI", 24F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblStatus.ForeColor = Color.Cyan; // Chữ xanh Neon chuẩn phong cách Cyberpunk
            lblStatus.Location = new Point(0, 0);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(800, 450);
            lblStatus.TabIndex = 6;
            lblStatus.Text = "Đang tìm kiếm đối thủ...";
            lblStatus.TextAlign = ContentAlignment.MiddleCenter; // Luôn nằm chính giữa bất kể thu phóng
            // 
            // FormLobby
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(30, 30, 47); // Nền tối đồng bộ với FormMain
            ClientSize = new Size(800, 450);
            Controls.Add(lblStatus);
            DoubleBuffered = true; // Bật chống giật nháy cho đồ họa
            Name = "FormLobby";
            StartPosition = FormStartPosition.CenterScreen; // Hiển thị ngay giữa màn hình
            ResumeLayout(false);
        }

        #endregion
        private Label lblStatus;
    }
}