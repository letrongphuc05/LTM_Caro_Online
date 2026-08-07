using System;
using System.Drawing;
using System.Windows.Forms;
using CaroOnline.Logic;

namespace CaroOnline
{
    public partial class FormMain : Form
    {
        private BoardManager boardManager;

        public FormMain()
        {
            InitializeComponent();

            boardManager = new BoardManager(pnlChessBoard);
            boardManager.PlayerMarked += BoardManager_PlayerMarked;
            boardManager.GameEnded += BoardManager_GameEnded;

            boardManager.DrawChessBoard();
            UpdateStatus("Sẵn sàng.");

            // Không dùng AutoSize để cho phép resize tự do
            this.AutoSize = false;
            this.AutoSizeMode = AutoSizeMode.GrowOnly;

            // Set minimum size để tránh resize quá nhỏ
            this.MinimumSize = new Size(1200, 700);

            // Set size mặc định
            this.Size = new Size(1600, 950);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Styling form
            this.BackColor = Color.FromArgb(240, 240, 245);
        }


        // ==========================================
        // SỰ KIỆN TỪ UI -> GỬI XUỐNG BACKEND
        // ==========================================

        private void btnConnect_Click(object? sender, EventArgs e)
        {
            string ip = txtIP.Text.Trim();
            if (!int.TryParse(txtPort.Text.Trim(), out int port))
            {
                MessageBox.Show("Port không hợp lệ!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            UpdateStatus("Đang kết nối...");
            btnConnect.Enabled = false;

            // [BACKEND HOOK]: Gọi hàm kết nối TCP/UDP ở đây.
            // LƯU Ý CHO BACKEND: Phải chạy Connect trên Task/Thread riêng để không làm treo UI.
            // Ví dụ: Task.Run(() => SocketManager.Connect(ip, port));
        }

        private void BoardManager_PlayerMarked(object? sender, Point point)
        {
            UpdateStatus("Đối thủ đang suy nghĩ...");

            // [BACKEND HOOK]: Gọi hàm gửi tọa độ vừa đánh qua Socket.
            // Cấu trúc gói tin theo thỏa thuận (VD: "MOVE|x|y")
            // Ví dụ: SocketManager.Send($"MOVE|{point.X}|{point.Y}");
        }

        private void BoardManager_GameEnded(object? sender, string result)
        {
            boardManager.IsMyTurn = false;

            if (result == "YOU_WIN")
            {
                MessageBox.Show("Chúc mừng! Bạn đã chiến thắng.", "Kết thúc", MessageBoxButtons.OK, MessageBoxIcon.Information);
                // [BACKEND HOOK]: Gửi thông báo ENDGAME cho Client còn lại nếu làm Server
            }
            else if (result == "YOU_LOSE")
            {
                MessageBox.Show("Bạn đã thua. Chúc may mắn lần sau!", "Kết thúc", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // ==========================================
        // CÁC HÀM CUNG CẤP CHO BACKEND -> GỌI LÊN UI
        // LƯU Ý: Đã xử lý Thread-Safe (Invoke) chống treo UI
        // ==========================================

        // [BACKEND HOOK]: Gọi hàm này khi kết nối thành công hoặc thất bại
        public void Network_OnConnectionChanged(bool isConnected, string message)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => Network_OnConnectionChanged(isConnected, message)));
                return;
            }

            UpdateStatus(message);
            btnConnect.Enabled = !isConnected;

            if (isConnected)
            {
                // [BACKEND HOOK]: Set quyền đi trước. Ví dụ Server đi trước thì truyền true, Client truyền false.
                boardManager.IsMyTurn = true;
                UpdateStatus(boardManager.IsMyTurn ? "Tới lượt bạn đánh" : "Chờ đối thủ đánh");
            }
        }

        // [BACKEND HOOK]: Gọi hàm này khi nhận được tọa độ đối thủ gửi tới
        public void Network_OnReceiveMove(int x, int y)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => Network_OnReceiveMove(x, y)));
                return;
            }

            boardManager.ReceiveOpponentMove(x, y);
            UpdateStatus("Đến lượt bạn.");
        }

        // [BACKEND HOOK]: Gọi hàm này khi đối thủ thoát đột ngột (Socket Disconnect)
        public void Network_OnOpponentDisconnected()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => Network_OnOpponentDisconnected()));
                return;
            }

            boardManager.IsMyTurn = false;
            MessageBox.Show("Đối thủ đã ngắt kết nối!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            UpdateStatus("Mất kết nối.");
            btnConnect.Enabled = true;
        }

        // Hàm tiện ích cập nhật Label Trạng thái
        private void UpdateStatus(string statusMessage)
        {
            if (lblStatus.InvokeRequired)
            {
                lblStatus.Invoke(new Action(() => lblStatus.Text = statusMessage));
            }
            else
            {
                lblStatus.Text = statusMessage;
            }
        }

        private void InitializeComponent()
        {
            pnlChessBoard = new Panel();
            lblStatus = new Label();
            btnConnect = new Button();
            txtIP = new TextBox();
            txtPort = new TextBox();
            panel1 = new Panel();
            lblTitle = new Label();
            lblPortLabel = new Label();
            lblIPLabel = new Label();
            label1 = new Label();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // pnlChessBoard
            // 
            pnlChessBoard.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            pnlChessBoard.AutoScroll = true;
            pnlChessBoard.BackColor = Color.FromArgb(245, 245, 245);
            pnlChessBoard.BorderStyle = BorderStyle.Fixed3D;
            pnlChessBoard.Location = new Point(0, 0);
            pnlChessBoard.Name = "pnlChessBoard";
            pnlChessBoard.Size = new Size(1000, 900);
            pnlChessBoard.TabIndex = 0;
            // 
             // lblStatus
             // 
             lblStatus.BackColor = Color.FromArgb(230, 255, 230);
             lblStatus.BorderStyle = BorderStyle.FixedSingle;
             lblStatus.Font = new Font("Arial", 9F);
             lblStatus.ForeColor = Color.DarkGreen;
             lblStatus.Location = new Point(10, 230);
             lblStatus.Name = "lblStatus";
             lblStatus.Padding = new Padding(5);
             lblStatus.Size = new Size(280, 50);
             lblStatus.TabIndex = 4;
             lblStatus.Text = "Sẵn sàng.";
             lblStatus.TextAlign = ContentAlignment.MiddleLeft;
             lblStatus.Click += lblStatus_Click;
            // 
             // btnConnect
             // 
             btnConnect.BackColor = Color.FromArgb(70, 130, 180);
             btnConnect.Cursor = Cursors.Hand;
             btnConnect.FlatAppearance.BorderSize = 0;
             btnConnect.FlatStyle = FlatStyle.Flat;
             btnConnect.Font = new Font("Arial", 10F, FontStyle.Bold);
             btnConnect.ForeColor = Color.White;
             btnConnect.Location = new Point(10, 160);
             btnConnect.Name = "btnConnect";
             btnConnect.Size = new Size(280, 35);
             btnConnect.TabIndex = 3;
             btnConnect.Text = "Kết nối";
            btnConnect.UseVisualStyleBackColor = false;
            btnConnect.Click += btnConnect_Click_1;
            // 
             // txtIP
             // 
             txtIP.Font = new Font("Arial", 9F);
             txtIP.Location = new Point(10, 75);
             txtIP.Name = "txtIP";
             txtIP.Size = new Size(280, 20);
             txtIP.TabIndex = 1;
             txtIP.Text = "127.0.0.1";
             txtIP.TextChanged += textBox1_TextChanged;
            // 
             // txtPort
             // 
             txtPort.Font = new Font("Arial", 9F);
             txtPort.Location = new Point(10, 125);
             txtPort.Name = "txtPort";
             txtPort.Size = new Size(280, 20);
             txtPort.TabIndex = 2;
             txtPort.Text = "8080";
            // 
            // panel1
            // 
            panel1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            panel1.AutoScroll = true;
            panel1.BackColor = Color.FromArgb(248, 248, 255);
            panel1.BorderStyle = BorderStyle.Fixed3D;
            panel1.Controls.Add(lblTitle);
            panel1.Controls.Add(lblPortLabel);
            panel1.Controls.Add(lblIPLabel);
            panel1.Controls.Add(label1);
            panel1.Controls.Add(txtPort);
            panel1.Controls.Add(lblStatus);
            panel1.Controls.Add(txtIP);
            panel1.Controls.Add(btnConnect);
            panel1.Location = new Point(1000, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(300, 900);
            panel1.TabIndex = 5;
            panel1.Paint += panel1_Paint;
            // 
             // lblTitle
             // 
             lblTitle.Font = new Font("Arial", 16F, FontStyle.Bold);
             lblTitle.ForeColor = Color.FromArgb(70, 130, 180);
             lblTitle.Location = new Point(10, 10);
             lblTitle.Name = "lblTitle";
             lblTitle.Size = new Size(280, 30);
             lblTitle.TabIndex = 6;
             lblTitle.Text = "Caro Online";
             lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            // 
             // lblPortLabel
             // 
             lblPortLabel.AutoSize = true;
             lblPortLabel.Font = new Font("Arial", 9F, FontStyle.Bold);
             lblPortLabel.ForeColor = Color.FromArgb(70, 130, 180);
             lblPortLabel.Location = new Point(10, 105);
             lblPortLabel.Name = "lblPortLabel";
             lblPortLabel.Size = new Size(40, 16);
             lblPortLabel.TabIndex = 8;
             lblPortLabel.Text = "Port:";
            // 
             // lblIPLabel
             // 
             lblIPLabel.AutoSize = true;
             lblIPLabel.Font = new Font("Arial", 9F, FontStyle.Bold);
             lblIPLabel.ForeColor = Color.FromArgb(70, 130, 180);
             lblIPLabel.Location = new Point(10, 55);
             lblIPLabel.Name = "lblIPLabel";
             lblIPLabel.Size = new Size(75, 16);
             lblIPLabel.TabIndex = 7;
             lblIPLabel.Text = "Địa chỉ IP:";
            // 
             // label1
             // 
             label1.AutoSize = true;
             label1.Font = new Font("Arial", 9F, FontStyle.Bold);
             label1.ForeColor = Color.FromArgb(70, 130, 180);
             label1.Location = new Point(10, 210);
             label1.Name = "label1";
             label1.Size = new Size(132, 16);
             label1.TabIndex = 5;
             label1.Text = "Trạng thái:";
            // 
            // FormMain
            // 
            ClientSize = new Size(1300, 900);
            Controls.Add(pnlChessBoard);
            Controls.Add(panel1);
            Font = new Font("Arial", 10F);
            Name = "FormMain";
            Text = "Caro Online - Tic Tac Toe";
            Load += pnlChessBoard_Load;
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ResumeLayout(false);

        }

        // ==========================================
        // DỌN DẸP TÀI NGUYÊN (Yêu cầu của đề tài)
        // ==========================================
        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            // [BACKEND HOOK]: Gọi hàm đóng Socket, giải phóng tài nguyên mạng trước khi tắt form
            // Ví dụ: SocketManager.CloseConnection();
        }

        private void textBox1_TextChanged(object? sender, EventArgs e)
        {

        }

        private void pnlChessBoard_Load(object? sender, EventArgs e)
        {
            // Thêm event handler cho Form Resize
            this.Resize += FormMain_Resize;
            pnlChessBoard.Paint += PnlChessBoard_Paint;
            // Removed: pnlChessBoard.Resize handler - causes lag and glitches
        }

        private void PnlChessBoard_Paint(object? sender, PaintEventArgs e)
        {
            // Draw border around chessboard
            using (Pen pen = new Pen(Color.FromArgb(139, 69, 19), 3))
            {
                e.Graphics.DrawRectangle(pen, 0, 0, pnlChessBoard.Width - 1, pnlChessBoard.Height - 1);
            }
        }

        private void FormMain_Resize(object? sender, EventArgs e)
        {
            // Redraw bảng cờ khi form resize
            if (boardManager != null && pnlChessBoard != null && panel1 != null)
            {
                // Keep panel1 width fixed at 300, pnlChessBoard takes the rest
                int panelWidth = 300;
                int chessboardWidth = this.ClientSize.Width - panelWidth;
                int chessboardHeight = this.ClientSize.Height;

                pnlChessBoard.Size = new Size(Math.Max(400, chessboardWidth), Math.Max(400, chessboardHeight));
                panel1.Location = new Point(chessboardWidth, 0);
                panel1.Size = new Size(panelWidth, chessboardHeight);
            }
        }

        private Panel pnlChessBoard;
        private Label lblStatus;
        private Button btnConnect;
        private TextBox txtIP;
        private Panel panel1;
        private Label label1;
        private TextBox txtPort;
        private Label lblIPLabel;
        private Label lblPortLabel;
        private Label lblTitle;

        private void btnConnect_Click_1(object sender, EventArgs e)
        {

        }

        private void lblStatus_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            // Draw gradient background
            using (Brush brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                panel1.ClientRectangle,
                Color.FromArgb(248, 248, 255),
                Color.FromArgb(230, 240, 250),
                System.Drawing.Drawing2D.LinearGradientMode.Vertical))
            {
                e.Graphics.FillRectangle(brush, panel1.ClientRectangle);
            }

            // Draw border
            using (Pen pen = new Pen(Color.FromArgb(70, 130, 180), 2))
            {
                e.Graphics.DrawRectangle(pen, 0, 0, panel1.Width - 1, panel1.Height - 1);
            }
        }
    }
}