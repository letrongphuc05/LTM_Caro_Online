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
            lblStatus.AutoSize = true;
            lblStatus.Font = new Font("Segoe UI", 18F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblStatus.Location = new Point(227, 177);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(360, 41);
            lblStatus.TabIndex = 6;
            lblStatus.Text = "Đang tìm kiếm đối thủ...";
            // 
            // FormLobby
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(800, 450);
            Controls.Add(lblStatus);
            DoubleBuffered = true;
            Name = "FormLobby";
            Text = "Sảnh chờ";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Label lblStatus;
    }
}