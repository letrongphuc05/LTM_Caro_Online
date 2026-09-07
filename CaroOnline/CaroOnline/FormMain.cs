using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using CaroOnline.Logic;
using CaroOnline.Network;

namespace CaroOnline
{
    public partial class FormMain : Form
    {
        private readonly CaroOnline.History.HistoryManagerClient historyManager;
        private BoardManager boardManager;
        private Label lblTimer;
        private System.Windows.Forms.Timer? uiUpdateTimer;

        private bool isRematchRequested = false;
        private bool isClosing = false; // Biến chống kẹt hộp thoại khi đang đóng Form

        // UI Bảng hỏi tái đấu
        private Panel pnlRematch;
        private Label lblRematchMessage;
        private Button btnRematchYes;
        private Button btnRematchNo;

        public FormMain(bool isPlayer1 = true, string opponentName = "Khách")
        {
            InitializeComponent();
            historyManager = new CaroOnline.History.HistoryManagerClient();
            boardManager = new BoardManager(pnlChessBoard);
            boardManager.PlayerMarked += BoardManager_PlayerMarked;
            boardManager.GameEnded += BoardManager_GameEnded;
            boardManager.TimerExpired += BoardManager_TimerExpired;
            boardManager.TimerTick += BoardManager_TimerTick;
            boardManager.DrawChessBoard();

            SocketManager.Instance.OnReceiveMove += Network_OnReceiveMove;
            SocketManager.Instance.OnOpponentDisconnected += Network_OnOpponentDisconnected;
            SocketManager.Instance.OnConnectionChanged += Network_OnConnectionChanged;

            txtIP.Enabled = false;
            txtPort.Enabled = false;
            btnConnect.Enabled = false;
            btnConnect.Text = "Đang trong trận...";

            boardManager.IsMyTurn = isPlayer1;
            boardManager.MySymbol = isPlayer1 ? "X" : "O";

            if (isPlayer1)
            {
                UpdateStatus($"Bạn đi trước (X). Đối thủ: {opponentName}");
                boardManager.StartTurnTimer();
            }
            else
            {
                UpdateStatus($"Bạn đi sau (O). Chờ {opponentName} đánh...");
            }

            this.AutoSize = false;
            this.AutoSizeMode = AutoSizeMode.GrowOnly;
            this.MinimumSize = new Size(1200, 700);
            this.Size = new Size(1600, 950);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(240, 240, 245);

            SetupTimerUI();
            SetupSkinSelectorUI();
            SetupRematchDialogUI();

            uiUpdateTimer = new System.Windows.Forms.Timer();
            uiUpdateTimer.Interval = 100;
            uiUpdateTimer.Tick += UIUpdateTimer_Tick;
            uiUpdateTimer.Start();

            this.Load += FormMain_Load;
        }

        private void FormMain_Load(object? sender, EventArgs e)
        {
            if (SocketManager.Instance.PendingMoveX >= 0 && SocketManager.Instance.PendingMoveY >= 0)
            {
                Network_OnReceiveMove(SocketManager.Instance.PendingMoveX, SocketManager.Instance.PendingMoveY);
                SocketManager.Instance.PendingMoveX = -1;
                SocketManager.Instance.PendingMoveY = -1;
            }
        }

        private void LoadAllHistory()
        {
            historyManager.LoadAllHistory();
        }

        private void btnHistory_Click(object? sender, EventArgs e)
        {
            LoadAllHistory();
        }

        private void btnConnect_Click(object? sender, EventArgs e) { }

        private void BoardManager_PlayerMarked(object? sender, Point point)
        {
            UpdateStatus("Đối thủ đang suy nghĩ...");
            string payload = $"MOVE|{point.X}|{point.Y}";
            SocketManager.Instance.Send(payload);
        }

        private void BoardManager_TimerExpired(object? sender, EventArgs e)
        {
            boardManager.IsMyTurn = false;
            UpdateStatus("Hết giờ! Bạn đã mất lượt đi.");
            SocketManager.Instance.Send("MOVE|-1|-1");
        }

        private void UIUpdateTimer_Tick(object? sender, EventArgs e)
        {
            if (boardManager == null || lblTimer == null)
                return;

            int timeRemaining = boardManager.GetTimeRemaining();
            lblTimer.Text = timeRemaining.ToString();

            if (timeRemaining <= 0)
                lblTimer.ForeColor = Color.FromArgb(128, 128, 128);
            else if (timeRemaining <= 5)
                lblTimer.ForeColor = Color.FromArgb(255, 0, 0);
            else if (timeRemaining <= 10)
                lblTimer.ForeColor = Color.FromArgb(255, 85, 0);
            else if (timeRemaining <= 15)
                lblTimer.ForeColor = Color.FromArgb(255, 165, 0);
            else
                lblTimer.ForeColor = Color.FromArgb(0, 128, 0);
        }

        private void BoardManager_TimerTick(object? sender, int timeRemaining)
        {
            if (lblTimer.InvokeRequired)
            {
                lblTimer.Invoke(new Action(() => BoardManager_TimerTick(sender, timeRemaining)));
                return;
            }

            lblTimer.Text = timeRemaining.ToString();

            if (timeRemaining <= 10)
                lblTimer.ForeColor = Color.FromArgb(255, 0, 0);
            else if (timeRemaining <= 20)
                lblTimer.ForeColor = Color.FromArgb(255, 165, 0);
            else
                lblTimer.ForeColor = Color.FromArgb(34, 139, 34);
        }

        private void BoardManager_GameEnded(object? sender, string result)
        {
            boardManager.IsMyTurn = false;
            boardManager.StopTurnTimer();

            string message = result == "YOU_WIN" ? "Chúc mừng! Bạn đã chiến thắng." : "Bạn đã thua. Chúc may mắn lần sau!";
            isRematchRequested = false;

            lblRematchMessage.Text = message + "\n\nBạn có muốn tái đấu không?";
            btnRematchYes.Visible = true;
            btnRematchNo.Visible = true;
            pnlRematch.Visible = true;
            pnlRematch.BringToFront();
        }

        // TÁI ĐẤU DÙNG TỌA ĐỘ ẢO XUYÊN QUA SERVER
        private void HandleRematchRequest()
        {
            if (isRematchRequested)
            {
                SocketManager.Instance.Send("MOVE|-4|-4"); // ĐỒNG Ý TÁI ĐẤU
                PerformRematch();
            }
            else
            {
                lblRematchMessage.Text = "Đối thủ muốn tái đấu.\nBạn có đồng ý không?";
                btnRematchYes.Visible = true;
                btnRematchNo.Visible = true;
                pnlRematch.Visible = true;
                pnlRematch.BringToFront();
            }
        }

        private void PerformRematch()
        {
            pnlRematch.Visible = false;
            boardManager.StopTurnTimer();
            boardManager.DrawChessBoard();
            boardManager.IsMyTurn = false;
            boardManager.StartTurnTimer();

            lblTimer.Text = "30";
            lblTimer.ForeColor = Color.FromArgb(0, 128, 0);

            UpdateStatus("Trận mới bắt đầu...");
        }

        private void HandleRematchDeclined()
        {
            if (isClosing) return; // Nếu đang đóng rồi thì không hiện hộp thoại nữa
            isClosing = true;

            MessageBox.Show("Đối thủ không muốn tái đấu. Trận đấu kết thúc.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close();
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            isClosing = true;
            boardManager.StopTurnTimer();
            if (uiUpdateTimer != null)
            {
                uiUpdateTimer.Stop();
                uiUpdateTimer.Dispose();
            }
            SocketManager.Instance.OnReceiveMove -= Network_OnReceiveMove;
            SocketManager.Instance.OnOpponentDisconnected -= Network_OnOpponentDisconnected;
            SocketManager.Instance.OnConnectionChanged -= Network_OnConnectionChanged;

            // Bắn mật mã TỪ CHỐI TÁI ĐẤU khi tắt ngang cửa sổ
            SocketManager.Instance.Send("MOVE|-2|-2");
        }

        public void Network_OnConnectionChanged(bool isConnected, string message)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => Network_OnConnectionChanged(isConnected, message)));
                return;
            }
            UpdateStatus(message);
        }

        // TRUNG TÂM GIẢI MÃ TỌA ĐỘ ẢO
        public void Network_OnReceiveMove(int x, int y)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => Network_OnReceiveMove(x, y)));
                return;
            }

            if (x == -1 && y == -1) // ĐỐI THỦ HẾT GIỜ
            {
                boardManager.IsMyTurn = true;
                boardManager.StartTurnTimer();
                UpdateStatus("Đối thủ bị hết giờ. Đã chuyển lượt cho bạn!");
                return;
            }
            else if (x == -2 && y == -2) // ĐỐI THỦ TỪ CHỐI / THOÁT
            {
                HandleRematchDeclined();
                return;
            }
            else if (x == -3 && y == -3) // ĐỐI THỦ XIN TÁI ĐẤU
            {
                HandleRematchRequest();
                return;
            }
            else if (x == -4 && y == -4) // ĐỐI THỦ ĐỒNG Ý
            {
                PerformRematch();
                return;
            }

            boardManager.ReceiveOpponentMove(x, y);
            boardManager.IsMyTurn = true;
            pnlChessBoard.Invalidate();
            UpdateStatus("Đến lượt bạn.");
        }

        public void Network_OnOpponentDisconnected()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => Network_OnOpponentDisconnected()));
                return;
            }

            if (isClosing) return;
            isClosing = true;

            boardManager.IsMyTurn = false;
            MessageBox.Show("Đối thủ đã ngắt kết nối hoặc thoát phòng!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            UpdateStatus("Đối thủ đã thoát.");
            this.Close();
        }

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

        private ComboBox cbSkinSelector;

        private void SetupRematchDialogUI()
        {
            pnlRematch = new Panel();
            pnlRematch.Size = new Size(350, 150);
            pnlRematch.BackColor = Color.White;
            pnlRematch.BorderStyle = BorderStyle.FixedSingle;
            pnlRematch.Visible = false;

            lblRematchMessage = new Label();
            lblRematchMessage.Font = new Font("Arial", 11, FontStyle.Bold);
            lblRematchMessage.TextAlign = ContentAlignment.MiddleCenter;
            lblRematchMessage.Dock = DockStyle.Top;
            lblRematchMessage.Height = 80;

            btnRematchYes = new Button();
            btnRematchYes.Text = "Tái Đấu";
            btnRematchYes.Size = new Size(100, 40);
            btnRematchYes.Location = new Point(50, 90);
            btnRematchYes.BackColor = Color.FromArgb(70, 130, 180);
            btnRematchYes.ForeColor = Color.White;
            btnRematchYes.FlatStyle = FlatStyle.Flat;
            btnRematchYes.Cursor = Cursors.Hand;
            btnRematchYes.Click += (s, e) => {
                isRematchRequested = true;
                SocketManager.Instance.Send("MOVE|-3|-3"); // XIN TÁI ĐẤU
                lblRematchMessage.Text = "Đang chờ đối thủ xác nhận...";
                btnRematchYes.Visible = false;
                btnRematchNo.Visible = false;
            };

            btnRematchNo = new Button();
            btnRematchNo.Text = "Thoát";
            btnRematchNo.Size = new Size(100, 40);
            btnRematchNo.Location = new Point(200, 90);
            btnRematchNo.BackColor = Color.IndianRed;
            btnRematchNo.ForeColor = Color.White;
            btnRematchNo.FlatStyle = FlatStyle.Flat;
            btnRematchNo.Cursor = Cursors.Hand;
            btnRematchNo.Click += (s, e) => {
                SocketManager.Instance.Send("MOVE|-2|-2"); // TỪ CHỐI
                this.Close();
            };

            pnlRematch.Controls.Add(lblRematchMessage);
            pnlRematch.Controls.Add(btnRematchYes);
            pnlRematch.Controls.Add(btnRematchNo);
            this.Controls.Add(pnlRematch);
        }

        private void SetupTimerUI()
        {
            Panel pnlTimerBackground = new Panel();
            pnlTimerBackground.Size = new Size(130, 100);
            pnlTimerBackground.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            pnlTimerBackground.BackColor = Color.FromArgb(255, 250, 205);
            pnlTimerBackground.BorderStyle = BorderStyle.FixedSingle;
            pnlTimerBackground.BringToFront();
            pnlTimerBackground.Margin = new Padding(10);
            this.Controls.Add(pnlTimerBackground);

            Label lblTimerTitle = new Label();
            lblTimerTitle.Text = "Thời Gian";
            lblTimerTitle.Location = new Point(5, 5);
            lblTimerTitle.Size = new Size(120, 20);
            lblTimerTitle.Font = new Font("Arial", 10, FontStyle.Bold);
            lblTimerTitle.ForeColor = Color.FromArgb(0, 0, 0);
            lblTimerTitle.TextAlign = ContentAlignment.MiddleCenter;
            pnlTimerBackground.Controls.Add(lblTimerTitle);

            lblTimer = new Label();
            lblTimer.Text = "30";
            lblTimer.Location = new Point(5, 25);
            lblTimer.Size = new Size(120, 70);
            lblTimer.Font = new Font("Arial", 48, FontStyle.Bold);
            lblTimer.ForeColor = Color.FromArgb(220, 20, 60);
            lblTimer.TextAlign = ContentAlignment.MiddleCenter;
            lblTimer.BackColor = Color.Transparent;
            lblTimer.BorderStyle = BorderStyle.None;
            pnlTimerBackground.Controls.Add(lblTimer);
        }

        private void SetupSkinSelectorUI()
        {
            Label lblSkinTitle = new Label();
            lblSkinTitle.Text = "Chọn Skin Quân Cờ:";
            lblSkinTitle.Location = new Point(10, 300);
            lblSkinTitle.AutoSize = true;
            lblSkinTitle.Font = new Font("Arial", 9, FontStyle.Bold);
            lblSkinTitle.ForeColor = Color.FromArgb(70, 130, 180);

            cbSkinSelector = new ComboBox();
            cbSkinSelector.Location = new Point(10, 325);
            cbSkinSelector.Size = new Size(280, 25);
            cbSkinSelector.DropDownStyle = ComboBoxStyle.DropDownList;

            cbSkinSelector.Items.Add("Mặc định (X / O)");
            cbSkinSelector.Items.Add("Skin CS2 (CT / T)");
            cbSkinSelector.Items.Add("Hoạt hình (Tom / Jerry)");
            cbSkinSelector.SelectedIndex = 0;

            cbSkinSelector.SelectedIndexChanged += (s, e) => {
                MessageBox.Show("Bạn đang chọn: " + cbSkinSelector.SelectedItem.ToString());
            };

            if (panel1 != null)
            {
                panel1.Controls.Add(lblSkinTitle);
                panel1.Controls.Add(cbSkinSelector);
            }
        }

        private void InitializeComponent()
        {
            pnlChessBoard = new Panel();
            lblStatus = new Label();
            btnConnect = new Button();
            btnHistory = new Button();
            txtIP = new TextBox();
            txtPort = new TextBox();
            panel1 = new Panel();
            lblTitle = new Label();
            lblPortLabel = new Label();
            lblIPLabel = new Label();
            label1 = new Label();
            panel1.SuspendLayout();
            SuspendLayout();

            pnlChessBoard.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            pnlChessBoard.AutoScroll = true;
            pnlChessBoard.BackColor = Color.FromArgb(245, 245, 245);
            pnlChessBoard.BorderStyle = BorderStyle.Fixed3D;
            pnlChessBoard.Location = new Point(0, 0);
            pnlChessBoard.Name = "pnlChessBoard";
            pnlChessBoard.Size = new Size(1000, 900);
            pnlChessBoard.TabIndex = 0;

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

            btnHistory.BackColor = Color.FromArgb(70, 130, 180);
            btnHistory.Cursor = Cursors.Hand;
            btnHistory.FlatAppearance.BorderSize = 0;
            btnHistory.FlatStyle = FlatStyle.Flat;
            btnHistory.Font = new Font("Arial", 10F, FontStyle.Bold);
            btnHistory.ForeColor = Color.White;
            btnHistory.Location = new Point(10, 285);
            btnHistory.Name = "btnHistory";
            btnHistory.Size = new Size(280, 35);
            btnHistory.TabIndex = 4;
            btnHistory.Text = "Xem lịch sử";
            btnHistory.UseVisualStyleBackColor = false;
            btnHistory.Click += btnHistory_Click;

            txtIP.Font = new Font("Arial", 9F);
            txtIP.Location = new Point(10, 75);
            txtIP.Name = "txtIP";
            txtIP.Size = new Size(280, 20);
            txtIP.TabIndex = 1;
            txtIP.Text = "127.0.0.1";

            txtPort.Font = new Font("Arial", 9F);
            txtPort.Location = new Point(10, 125);
            txtPort.Name = "txtPort";
            txtPort.Size = new Size(280, 20);
            txtPort.TabIndex = 2;
            txtPort.Text = "8080";

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
            panel1.Controls.Add(btnHistory);
            panel1.Location = new Point(1000, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(300, 900);
            panel1.TabIndex = 5;
            panel1.Paint += panel1_Paint;

            lblTitle.Font = new Font("Arial", 16F, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(70, 130, 180);
            lblTitle.Location = new Point(10, 10);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(280, 30);
            lblTitle.TabIndex = 6;
            lblTitle.Text = "Caro Online";
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;

            lblPortLabel.AutoSize = true;
            lblPortLabel.Font = new Font("Arial", 9F, FontStyle.Bold);
            lblPortLabel.ForeColor = Color.FromArgb(70, 130, 180);
            lblPortLabel.Location = new Point(10, 105);
            lblPortLabel.Name = "lblPortLabel";
            lblPortLabel.Size = new Size(40, 15);
            lblPortLabel.TabIndex = 8;
            lblPortLabel.Text = "Port:";

            lblIPLabel.AutoSize = true;
            lblIPLabel.Font = new Font("Arial", 9F, FontStyle.Bold);
            lblIPLabel.ForeColor = Color.FromArgb(70, 130, 180);
            lblIPLabel.Location = new Point(10, 55);
            lblIPLabel.Name = "lblIPLabel";
            lblIPLabel.Size = new Size(61, 15);
            lblIPLabel.TabIndex = 7;
            lblIPLabel.Text = "Địa chỉ IP:";

            label1.AutoSize = true;
            label1.Font = new Font("Arial", 9F, FontStyle.Bold);
            label1.ForeColor = Color.FromArgb(70, 130, 180);
            label1.Location = new Point(10, 210);
            label1.Name = "label1";
            label1.Size = new Size(67, 15);
            label1.TabIndex = 5;
            label1.Text = "Trạng thái:";

            ClientSize = new Size(1300, 900);
            Controls.Add(pnlChessBoard);
            Controls.Add(panel1);
            Font = new Font("Arial", 10F);
            Name = "FormMain";
            Text = "Caro Online - Tic Tac Toe";
            FormClosing += FormMain_FormClosing;
            Load += pnlChessBoard_Load;
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ResumeLayout(false);
        }

        private void pnlChessBoard_Load(object? sender, EventArgs e)
        {
            this.Resize += FormMain_Resize;
            pnlChessBoard.Paint += PnlChessBoard_Paint;
        }

        private void PnlChessBoard_Paint(object? sender, PaintEventArgs e)
        {
            using (Pen pen = new Pen(Color.FromArgb(139, 69, 19), 3))
            {
                e.Graphics.DrawRectangle(pen, 0, 0, pnlChessBoard.Width - 1, pnlChessBoard.Height - 1);
            }
        }

        private void FormMain_Resize(object? sender, EventArgs e)
        {
            if (boardManager != null && pnlChessBoard != null && panel1 != null)
            {
                int panelWidth = 300;
                int chessboardWidth = this.ClientSize.Width - panelWidth;
                int chessboardHeight = this.ClientSize.Height;

                pnlChessBoard.Size = new Size(Math.Max(400, chessboardWidth), Math.Max(400, chessboardHeight));
                panel1.Location = new Point(chessboardWidth, 0);
                panel1.Size = new Size(panelWidth, chessboardHeight);

                if (pnlRematch != null)
                {
                    pnlRematch.Location = new Point(
                        pnlChessBoard.Location.X + (pnlChessBoard.Width - pnlRematch.Width) / 2,
                        pnlChessBoard.Location.Y + (pnlChessBoard.Height - pnlRematch.Height) / 2
                    );
                }
            }
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            using (Brush brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                panel1.ClientRectangle,
                Color.FromArgb(248, 248, 255),
                Color.FromArgb(230, 240, 250),
                System.Drawing.Drawing2D.LinearGradientMode.Vertical))
            {
                e.Graphics.FillRectangle(brush, panel1.ClientRectangle);
            }

            using (Pen pen = new Pen(Color.FromArgb(70, 130, 180), 2))
            {
                e.Graphics.DrawRectangle(pen, 0, 0, panel1.Width - 1, panel1.Height - 1);
            }
        }

        private Panel pnlChessBoard;
        private Label lblStatus;
        private Button btnConnect;
        private Button btnHistory;
        private TextBox txtIP;
        private Panel panel1;
        private Label label1;
        private TextBox txtPort;
        private Label lblIPLabel;
        private Label lblPortLabel;
        private Label lblTitle;
    }
}