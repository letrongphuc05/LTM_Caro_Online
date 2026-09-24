using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Threading;
using CaroOnline.Logic;
using CaroOnline.Network;

namespace CaroOnline
{
    public partial class FormMain : Form
    {
        // Biến lưu trữ lịch sử để biết ai đi trước ở ván tái đấu
        private bool amIWinner = false;
        private bool isGameEnded = false;

        // Biến đếm số lượt bỏ qua để xử lý logic AFK
        private int myMissedTurns = 0;
        private int opponentMissedTurns = 0;

        // Biến phục vụ tính năng thoát trận và chờ kết nối (Đếm lùi 60 giây)
        private Button btnSurrender;
        private System.Windows.Forms.Timer? disconnectWaitTimer;
        private int disconnectWaitTime = 60;

        private readonly CaroOnline.History.HistoryManagerClient historyManager;
        private BoardManager boardManager;
        private Label lblTimer;
        private Panel pnlTimerBackground;
        private Panel pnlStatusBorder;
        private Color statusBorderDefaultColor = Color.FromArgb(50, 130, 0);

        // Timer dùng để chớp đỏ màn hình cảnh báo khi sắp hết giờ đánh cờ
        private System.Windows.Forms.Timer? uiUpdateTimer;
        private System.Windows.Forms.Timer? blinkingTimer;
        private bool isBlinking = false;
        private int blinkCounter = 0;

        private bool isRematchRequested = false;
        private bool isClosing = false;
        private bool isPlayer1 = true;
        private bool isSpectator = false;
        private bool historyResultSent = false;

        // UI lịch sử trận đấu
        private Panel pnlHistory;
        private Label lblHistoryTitle;
        private DataGridView dgvHistory;

        // UI Bảng hỏi tái đấu
        private Panel pnlRematch;
        private Label lblRematchMessage;
        private Button btnRematchYes;
        private Button btnRematchNo;

        public FormMain(bool isPlayer1Param = true, string opponentName = "Khách")
        {
            InitializeComponent();

            try
            {
                txtIP.Text = SocketManager.Instance.IP;
                txtPort.Text = SocketManager.Instance.Port.ToString();
            }
            catch { }

            isPlayer1 = isPlayer1Param;
            isSpectator = opponentName == "Khán giả";
            historyManager = new CaroOnline.History.HistoryManagerClient();
            boardManager = new BoardManager(pnlChessBoard);

            SetupTimerUI();
            SetupRematchDialogUI();
            SetupHistoryUI();
            SetupSurrenderUI(); // Khởi tạo nút Đầu hàng ở góc phải dưới

            // Gán các Event cho Bàn cờ và Mạng
            historyManager.HistoryUpdated += RefreshHistoryPanel;
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

            // Phân bổ lượt đánh ngay khi mới vào phòng
            if (isSpectator)
            {
                // Nếu là khán giả thì vô hiệu hóa quyền thao tác bàn cờ
                boardManager.IsMyTurn = false;
                boardManager.MySymbol = "";
                UpdateStatus("Đang xem trận đấu...");
            }
            else if (isPlayer1Param)
            {
                // Player 1 (Chủ phòng) Random để tung đồng xu quyết định ai đi trước
                bool p1GoesFirst = new Random().Next(0, 2) == 0;

                // Gửi kết quả ngẫu nhiên sang Player 2 thông qua tọa độ đặc biệt -5
                SocketManager.Instance.Send($"MOVE|-5|{(p1GoesFirst ? 1 : 0)}");
                ApplyTurnLogic(p1GoesFirst, false);
            }
            else
            {
                UpdateStatus("Đang tung đồng xu quyết định người đi trước...");
                boardManager.IsMyTurn = false; // Tạm khóa bàn cờ chờ dữ liệu tung đồng xu từ P1
            }

            // Fix kích thước cố định để tránh vỡ giao diện
            this.AutoSize = false;
            this.AutoSizeMode = AutoSizeMode.GrowOnly;
            this.MinimumSize = new Size(1200, 700);
            this.Size = new Size(1600, 950);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(240, 240, 245);

            uiUpdateTimer = new System.Windows.Forms.Timer();
            uiUpdateTimer.Interval = 100;
            uiUpdateTimer.Tick += UIUpdateTimer_Tick;
            uiUpdateTimer.Start();

            this.Load += FormMain_Load;
            pnlChessBoard.Resize += PnlChessBoard_Resize;
        }

        // Tạo nút "Đầu hàng" hiển thị ở giao diện cạnh bàn cờ
        private void SetupSurrenderUI()
        {
            btnSurrender = new Button();
            btnSurrender.Text = "Đầu hàng"; // Đã đổi từ "Thoát trận" thành "Đầu hàng"
            btnSurrender.Size = new Size(120, 35);
            btnSurrender.Location = new Point(160, 840);
            btnSurrender.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnSurrender.BackColor = Color.IndianRed;
            btnSurrender.ForeColor = Color.White;
            btnSurrender.FlatStyle = FlatStyle.Flat;
            btnSurrender.Font = new Font("Arial", 10F, FontStyle.Bold);
            btnSurrender.Cursor = Cursors.Hand;

            // Ẩn nút với khán giả
            btnSurrender.Visible = !isSpectator;

            btnSurrender.Click += (s, e) =>
            {
                DialogResult res = MessageBox.Show("Bạn có chắc chắn muốn đầu hàng? Bạn sẽ bị xử thua ngay lập tức.", "Xác nhận đầu hàng", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (res == DialogResult.Yes)
                {
                    // Tọa độ -6 là tín hiệu báo đầu hàng
                    SocketManager.Instance.Send("MOVE|-6|-6");
                    MessageBox.Show("Bạn đã đầu hàng và bị xử thua.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    isGameEnded = true;
                    isClosing = true;
                    this.Close();
                }
            };
            panel1.Controls.Add(btnSurrender);
        }

        // Hàm được gọi từ FormLobby khi người chơi bấm "Kết nối lại"
        public void ReconnectFromLobby()
        {
            SocketManager.Instance.Send("MOVE|-8|-8"); // Gỡ timer đếm 60s bên máy đối thủ
        }

        // Hàm được gọi từ FormLobby khi người chơi bấm "Đầu hàng"
        public void SurrenderFromLobby()
        {
            SocketManager.Instance.Send("MOVE|-6|-6"); // Xử thua mình
            isGameEnded = true;
            isClosing = true;
            this.Close();
        }

        // Hàm đồng bộ lượt đi, thiết lập chữ (X hoặc O) và Text Status thông báo
        private void ApplyTurnLogic(bool amIFirst, bool isRematch)
        {
            boardManager.IsMyTurn = amIFirst;
            boardManager.MySymbol = amIFirst ? "X" : "O"; // Người đi trước luôn là chữ X

            if (amIFirst)
            {
                boardManager.StartTurnTimer();
                string msg = isRematch ? "Bạn đã thắng ván trước. Bạn được đi trước (X)!" : "Kết quả Random: Bạn đi trước (X)!";
                UpdateStatus(msg);
            }
            else
            {
                boardManager.StopTurnTimer();
                lblTimer.Text = "30";
                lblTimer.ForeColor = Color.FromArgb(0, 128, 0);

                string msg = isRematch ? "Đối thủ thắng ván trước. Chờ đối thủ đánh (X)..." : "Kết quả Random: Đối thủ đi trước (X). Đang chờ...";
                UpdateStatus(msg);
            }
        }

        // Bắt trường hợp nhận được nước cờ từ Server nhưng FormMain chưa kịp Load xong giao diện
        private void FormMain_Load(object? sender, EventArgs e)
        {
            if (SocketManager.Instance.PendingMoveX >= 0 && SocketManager.Instance.PendingMoveY >= 0 || SocketManager.Instance.PendingMoveX == -5)
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
            if (pnlHistory.Visible)
            {
                pnlHistory.Visible = false;
                btnHistory.Text = "Xem lịch sử";
                return;
            }

            pnlHistory.Visible = true;
            pnlHistory.BringToFront();
            btnHistory.Text = "Ẩn lịch sử";
            RenderHistory(historyManager.GetHistory());
            LoadAllHistory();
        }

        private void SetupHistoryUI()
        {
            pnlHistory = new Panel();
            pnlHistory.Location = new Point(10, 330);
            pnlHistory.Size = new Size(280, 550);
            pnlHistory.BorderStyle = BorderStyle.FixedSingle;
            pnlHistory.BackColor = Color.White;
            pnlHistory.Visible = false;
            pnlHistory.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            lblHistoryTitle = new Label();
            lblHistoryTitle.Text = "LỊCH SỬ TRẬN ĐẤU";
            lblHistoryTitle.Font = new Font("Arial", 10F, FontStyle.Bold);
            lblHistoryTitle.TextAlign = ContentAlignment.MiddleCenter;
            lblHistoryTitle.Dock = DockStyle.Top;
            lblHistoryTitle.Height = 34;
            lblHistoryTitle.BackColor = Color.FromArgb(70, 130, 180);
            lblHistoryTitle.ForeColor = Color.White;

            dgvHistory = new DataGridView();
            dgvHistory.Dock = DockStyle.Fill;
            dgvHistory.ReadOnly = true;
            dgvHistory.AllowUserToAddRows = false;
            dgvHistory.AllowUserToDeleteRows = false;
            dgvHistory.AllowUserToResizeRows = false;
            dgvHistory.RowHeadersVisible = false;
            dgvHistory.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvHistory.MultiSelect = false;
            dgvHistory.AutoGenerateColumns = false;
            dgvHistory.BackgroundColor = Color.White;
            dgvHistory.BorderStyle = BorderStyle.None;
            dgvHistory.EnableHeadersVisualStyles = true;
            dgvHistory.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;

            dgvHistory.Columns.Add(new DataGridViewTextBoxColumn
            { Name = "colGame", HeaderText = "Trận", Width = 42 });
            dgvHistory.Columns.Add(new DataGridViewTextBoxColumn
            { Name = "colTime", HeaderText = "Thời gian", Width = 60 });
            dgvHistory.Columns.Add(new DataGridViewTextBoxColumn
            { Name = "colPlayers", HeaderText = "Người chơi", Width = 105 });
            dgvHistory.Columns.Add(new DataGridViewTextBoxColumn
            { Name = "colResult", HeaderText = "Kết quả", Width = 68 });

            pnlHistory.Controls.Add(dgvHistory);
            pnlHistory.Controls.Add(lblHistoryTitle);
            panel1.Controls.Add(pnlHistory);
            pnlHistory.BringToFront();
        }

        private void RefreshHistoryPanel()
        {
            if (IsDisposed || !IsHandleCreated)
                return;

            if (InvokeRequired)
            {
                BeginInvoke(new Action(RefreshHistoryPanel));
                return;
            }

            if (pnlHistory.Visible)
                RenderHistory(historyManager.GetHistory());
        }

        private void RenderHistory(
            System.Collections.Generic.List<CaroOnline.History.GameHistoryInfo> histories)
        {
            if (dgvHistory == null)
                return;

            dgvHistory.Rows.Clear();

            var finishedHistories = histories
                .Where(h => !string.IsNullOrWhiteSpace(h.Winner))
                .OrderBy(h => h.StartTime)
                .ToList();

            if (finishedHistories.Count == 0)
            {
                dgvHistory.Rows.Add("-", "-", "Chưa có", "trận");
                return;
            }

            for (int i = 0; i < finishedHistories.Count; i++)
            {
                var history = finishedHistories[i];
                string time = history.EndTime != default
                    ? history.EndTime.ToString("HH:mm:ss")
                    : history.StartTime.ToString("HH:mm:ss");

                string players =
                    $"{history.PlayerX} vs {history.PlayerO}";

                dgvHistory.Rows.Add(
                    $"Game {i + 1}",
                    time,
                    players,
                    $"{history.Winner} thắng");
            }
        }

        private void btnConnect_Click(object? sender, EventArgs e) { }

        private void PnlChessBoard_Resize(object? sender, EventArgs e)
        {
            if (boardManager != null)
            {
                boardManager.OnBoardResized();
            }
        }

        // Bắt sự kiện khi click đánh 1 ô cờ
        private void BoardManager_PlayerMarked(object? sender, Point point)
        {
            if (isSpectator) return;

            // Đánh cờ hợp lệ thì reset đếm lỗi bỏ lượt
            myMissedTurns = 0;

            UpdateStatus("Đối thủ đang suy nghĩ...");

            // Đóng gói tọa độ thành lệnh và gửi đi
            string payload = $"MOVE|{point.X}|{point.Y}";
            SocketManager.Instance.Send(payload);
        }

        // Xử lý khi hết 30s suy nghĩ mà chưa đánh cờ
        private void BoardManager_TimerExpired(object? sender, EventArgs e)
        {
            boardManager.IsMyTurn = false;
            myMissedTurns++;

            // Gửi tọa độ ảo -1 báo cho đối phương biết mình bỏ lượt
            SocketManager.Instance.Send("MOVE|-1|-1");

            // Cơ chế xử thua nếu cố tình AFK quá 2 lượt liên tiếp
            if (myMissedTurns >= 2)
            {
                isGameEnded = true;
                isClosing = true;
                MessageBox.Show("Bạn đã bỏ qua 2 lượt liên tiếp (AFK) và bị xử thua!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.Close();
            }
            else
            {
                UpdateStatus("Hết giờ! Bạn đã mất lượt đi. (Cảnh báo: Bỏ 2 lượt sẽ bị xử thua)");
            }
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
            lblStatus.Text = $"Thời gian còn lại: {timeRemaining}s";

            // Hiệu ứng cảnh báo bằng cách kích hoạt nhấp nháy UI
            if (timeRemaining <= 5)
            {
                isBlinking = true;
                if (!blinkingTimer?.Enabled ?? true)
                {
                    if (blinkingTimer == null)
                    {
                        blinkingTimer = new System.Windows.Forms.Timer();
                        blinkingTimer.Interval = 300;
                        blinkingTimer.Tick += BlinkingTimer_Tick;
                    }
                    blinkingTimer.Start();
                }
                lblTimer.ForeColor = Color.FromArgb(255, 0, 0);
                lblStatus.ForeColor = Color.FromArgb(255, 0, 0);
            }
            else if (timeRemaining <= 10)
            {
                isBlinking = true;
                if (!blinkingTimer?.Enabled ?? true)
                {
                    if (blinkingTimer == null)
                    {
                        blinkingTimer = new System.Windows.Forms.Timer();
                        blinkingTimer.Interval = 500;
                        blinkingTimer.Tick += BlinkingTimer_Tick;
                    }
                    blinkingTimer.Start();
                }
                lblTimer.ForeColor = Color.FromArgb(255, 165, 0);
                lblStatus.ForeColor = Color.FromArgb(255, 165, 0);
            }
            else
            {
                if (blinkingTimer?.Enabled ?? false)
                {
                    blinkingTimer.Stop();
                }
                isBlinking = false;
                blinkCounter = 0;
                lblTimer.ForeColor = Color.FromArgb(34, 139, 34);
                lblStatus.ForeColor = Color.DarkGreen;
                if (pnlTimerBackground != null)
                {
                    pnlTimerBackground.BackColor = Color.FromArgb(255, 250, 205);
                }
                if (pnlStatusBorder != null)
                {
                    pnlStatusBorder.BackColor = statusBorderDefaultColor;
                }
            }
        }

        private void BlinkingTimer_Tick(object? sender, EventArgs e)
        {
            if (pnlTimerBackground == null && pnlStatusBorder == null) return;

            blinkCounter++;
            bool isBlink = (blinkCounter % 2 == 0);

            if (int.TryParse(lblTimer.Text, out int timeRemaining))
            {
                if (timeRemaining <= 5)
                {
                    Color redColor = isBlink
                        ? Color.FromArgb(255, 0, 0)
                        : Color.FromArgb(139, 0, 0);

                    if (pnlTimerBackground != null)
                        pnlTimerBackground.BackColor = redColor;

                    if (pnlStatusBorder != null)
                        pnlStatusBorder.BackColor = redColor;
                }
                else if (timeRemaining <= 10)
                {
                    Color yellowColor = isBlink
                        ? Color.FromArgb(255, 255, 0)
                        : Color.FromArgb(255, 250, 205);

                    if (pnlTimerBackground != null)
                        pnlTimerBackground.BackColor = yellowColor;

                    if (pnlStatusBorder != null)
                        pnlStatusBorder.BackColor = yellowColor;
                }
                else
                {
                    if (pnlTimerBackground != null)
                        pnlTimerBackground.BackColor = Color.FromArgb(255, 250, 205);

                    if (pnlStatusBorder != null)
                        pnlStatusBorder.BackColor = statusBorderDefaultColor;
                }
            }
        }

        // Được kích hoạt bởi Engine Bàn Cờ khi có chuỗi 5 ô liên tiếp
        private void BoardManager_GameEnded(object? sender, string result)
        {
            boardManager.IsMyTurn = false;
            boardManager.StopTurnTimer();

            if (blinkingTimer?.Enabled ?? false)
            {
                blinkingTimer.Stop();
            }
            isBlinking = false;
            blinkCounter = 0;
            isGameEnded = true;

            // Nếu là khán giả xem xong trận thì báo và đóng form ra Lobby
            if (isSpectator)
            {
                MessageBox.Show("Trận đấu đã kết thúc!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
                return;
            }

            // LƯU LẠI LỊCH SỬ THẮNG THUA ĐỂ TÍNH QUYỀN ĐI TRƯỚC VÁN TÁI ĐẤU
            if (result == "YOU_WIN" || result == "YOU_LOSE")
            {
                amIWinner = (result == "YOU_WIN");
            }

            // Gửi dữ liệu cho HistoryServer lưu lịch sử. Đảm bảo chỉ 1 máy (máy Thắng) gửi.
            if (!isSpectator &&
                result == "YOU_WIN" &&
                !historyResultSent)
            {
                historyResultSent = true;
                SocketManager.Instance.Send("GAME_END|WIN");
            }

            string message = result == "YOU_WIN" ? "Chúc mừng! Bạn đã chiến thắng." : "Bạn đã thua. Chúc may mắn lần sau!";
            isRematchRequested = false;

            lblRematchMessage.Text = message + "\n\nBạn có muốn tái đấu không?";
            btnRematchYes.Visible = true;
            btnRematchNo.Visible = true;
            pnlRematch.Visible = true;
            pnlRematch.BringToFront();
        }

        private void HandleRematchRequest()
        {
            if (isRematchRequested)
            {
                SocketManager.Instance.Send("MOVE|-4|-4");
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
            historyResultSent = false;
            isGameEnded = false;
            myMissedTurns = 0;         // Xóa bộ nhớ vi phạm AFK
            opponentMissedTurns = 0;   // Xóa bộ nhớ vi phạm AFK đối thủ
            pnlRematch.Visible = false;

            boardManager.StopTurnTimer();
            boardManager.DrawChessBoard();

            // Người thắng ván trước sẽ được cấp quyền đi trước ở ván này
            ApplyTurnLogic(amIWinner, true);
        }

        private void HandleRematchDeclined()
        {
            if (isClosing) return;
            isClosing = true;

            MessageBox.Show("Đối thủ không muốn tái đấu. Trận đấu kết thúc.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close();
        }

        // Bắt sự kiện khi người dùng bấm nút [X] màu đỏ tắt cửa sổ Form
        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (isClosing) return;

            if (isSpectator)
            {
                isClosing = true;
                string roomId = Tag?.ToString() ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(roomId))
                {
                    SocketManager.Instance.Send($"LEAVE_SPECTATE|{roomId}");
                }
                return;
            }

            // Nếu người dùng lỡ bấm [X] tắt cửa sổ khi chưa hết trận, Form sẽ không thoát mà chuyển về Lobby dưới dạng tạm ẩn
            if (!isGameEnded)
            {
                e.Cancel = true; // Hủy lệnh tự động Close Form của Windows
                this.Hide();     // Chỉ ẩn Form đi

                // -7 là lệnh báo cho đối phương biết mình đang ra Lobby
                SocketManager.Instance.Send("MOVE|-7|-7");

                // Mở bảng giao diện Kết nối lại ở Form Sảnh
                if (FormLobby.Instance != null)
                {
                    FormLobby.Instance.ShowReconnectPrompt(this);
                }
                return;
            }
            else
            {
                // Nếu trận đấu đã có kết quả (GameEnded), tắt hẳn thì báo -2 (Không tái đấu)
                isClosing = true;
                SocketManager.Instance.Send("MOVE|-2|-2");
            }
        }

        // Giải phóng triệt để tài nguyên khi Form chính thức bị tiêu hủy
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            boardManager?.StopTurnTimer();
            if (uiUpdateTimer != null)
            {
                uiUpdateTimer.Stop();
                uiUpdateTimer.Dispose();
            }
            if (disconnectWaitTimer != null)
            {
                disconnectWaitTimer.Stop();
                disconnectWaitTimer.Dispose();
            }
            SocketManager.Instance.OnReceiveMove -= Network_OnReceiveMove;
            SocketManager.Instance.OnOpponentDisconnected -= Network_OnOpponentDisconnected;
            SocketManager.Instance.OnConnectionChanged -= Network_OnConnectionChanged;

            base.OnFormClosed(e);
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

        // Hàm Core của Game: Xử lý mọi lệnh từ Server đổ về dưới dạng chuỗi `MOVE|x|y`
        public void Network_OnReceiveMove(int x, int y)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => Network_OnReceiveMove(x, y)));
                return;
            }

            // KHÁN GIẢ: chỉ nhận (x,y) và vẽ lên bàn cờ, không xét các lệnh điều khiển khác
            if (isSpectator)
            {
                if (x >= 0 && y >= 0)
                {
                    boardManager.ReceiveOpponentMove(x, y);
                    pnlChessBoard.Invalidate();
                }
                boardManager.IsMyTurn = false;
                UpdateStatus("Đang xem trận đấu...");
                return;
            }

            // CHUỖI LỆNH ĐIỀU KHIỂN (x và y mang giá trị âm là các tín hiệu hệ thống)
            if (x == -1 && y == -1)
            {
                // Tọa độ (-1, -1): Tín hiệu đối thủ bị hết thời gian (mất lượt)
                opponentMissedTurns++;

                if (opponentMissedTurns >= 2)
                {
                    isGameEnded = true;
                    isClosing = true;
                    MessageBox.Show("Đối thủ đã bỏ qua 2 lượt liên tiếp (AFK). Bạn được xử thắng!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    if (!this.Visible && FormLobby.Instance != null) FormLobby.Instance.HideReconnectPrompt();
                    this.Close();
                    return;
                }

                boardManager.IsMyTurn = true;
                boardManager.StartTurnTimer();
                UpdateStatus($"Đối thủ bị hết giờ (Lần {opponentMissedTurns}). Đã chuyển lượt cho bạn!");
                return;
            }
            else if (x == -2 && y == -2)
            {
                // Tọa độ (-2, -2): Tín hiệu đối thủ từ chối tái đấu
                HandleRematchDeclined();
                return;
            }
            else if (x == -3 && y == -3)
            {
                // Tọa độ (-3, -3): Tín hiệu đối thủ gửi lời mời tái đấu
                HandleRematchRequest();
                return;
            }
            else if (x == -4 && y == -4)
            {
                // Tọa độ (-4, -4): Tín hiệu đối thủ đồng ý tái đấu
                PerformRematch();
                return;
            }
            else if (x == -5)
            {
                // Tọa độ (-5, y): Tín hiệu mã hóa kết quả tung đồng xu từ Server/Chủ phòng
                bool p1GoesFirst = (y == 1);
                bool amIFirst = !p1GoesFirst; // Mình là Khách nên lấy logic ngược lại Chủ Phòng
                ApplyTurnLogic(amIFirst, false);
                return;
            }
            else if (x == -6 && y == -6)
            {
                // Tọa độ (-6, -6): Tín hiệu đối thủ bấm nút Đầu hàng hoặc chọn Không kết nối lại
                if (isClosing) return;
                isClosing = true;
                isGameEnded = true;
                MessageBox.Show("Đối thủ đã đầu hàng. Bạn được xử thắng!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Đóng hộp thoại ở sảnh (nếu có)
                if (!this.Visible && FormLobby.Instance != null) FormLobby.Instance.HideReconnectPrompt();
                this.Close();
                return;
            }
            else if (x == -7 && y == -7)
            {
                // Tọa độ (-7, -7): Tín hiệu đối thủ tạm thoát ra sảnh (ẩn game hoặc rớt mạng nhẹ)
                TriggerDisconnectWait();
                return;
            }
            else if (x == -8 && y == -8)
            {
                // Tọa độ (-8, -8): Tín hiệu đối thủ đã bấm nút Kết nối lại vào trận
                if (disconnectWaitTimer != null && disconnectWaitTimer.Enabled)
                {
                    disconnectWaitTimer.Stop(); // Hủy đồng hồ đếm ngược xử thua
                    UpdateStatus("Đối thủ đã kết nối lại. Trận đấu tiếp tục!");
                }
                return;
            }
            else if (x == -9 && y == -9)
            {
                // Tọa độ (-9, -9): Tín hiệu báo đối thủ chọn nút Thoát (Không chờ mình reconnect) -> xử mình thua
                if (isClosing) return;
                isClosing = true;
                isGameEnded = true;
                MessageBox.Show("Đối thủ đã không chấp nhận chờ đợi thêm. Bạn bị xử thua!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                if (!this.Visible && FormLobby.Instance != null) FormLobby.Instance.HideReconnectPrompt();
                this.Close();
                return;
            }
            else if (x >= 0 && y >= 0)
            {
                // Nếu là tọa độ đánh cờ hợp lệ thì xóa bộ đếm AFK của đối phương
                opponentMissedTurns = 0;
            }

            // Nếu không phải là lệnh điều khiển (Tọa độ >=0) thì vẽ cờ lên bàn
            boardManager.ReceiveOpponentMove(x, y);
            pnlChessBoard.Invalidate();

            boardManager.IsMyTurn = true;
            Point lastMove = boardManager.LastOpponentMove;
            int timeRemaining = boardManager.GetTimeRemaining();

            UpdateStatus($"Đến lượt bạn. Đối thủ đánh ô ({lastMove.X}, {lastMove.Y}). Suy nghĩ còn lại: {timeRemaining}s");
        }

        // Logic hỏi người dùng muốn chờ đối thủ (Bị ẩn game) 60 giây hay là xử thắng nhận giải luôn
        private void TriggerDisconnectWait()
        {
            DialogResult result = MessageBox.Show(
                "Đối thủ đã tạm thoát ra sảnh (hoặc gián đoạn mạng). Bạn có muốn chờ 1 phút để đối thủ kết nối lại không?\n\n- Yes: Chờ trong 1 phút.\n- No: Thoát và xử thắng cho bạn.",
                "Gián đoạn kết nối", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                UpdateStatus("Đang chờ đối thủ kết nối lại (60s)...");
                disconnectWaitTime = 60;
                if (disconnectWaitTimer == null)
                {
                    disconnectWaitTimer = new System.Windows.Forms.Timer();
                    disconnectWaitTimer.Interval = 1000;
                    disconnectWaitTimer.Tick += DisconnectWaitTimer_Tick;
                }
                disconnectWaitTimer.Start();
            }
            else
            {
                SocketManager.Instance.Send("MOVE|-9|-9"); // Gửi tín hiệu xử thua (-9) cho bên kia
                isGameEnded = true;
                isClosing = true;
                MessageBox.Show("Bạn đã chọn không chờ. Bạn được xử thắng do đối thủ bỏ trận!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                if (!this.Visible && FormLobby.Instance != null) FormLobby.Instance.HideReconnectPrompt();
                this.Close();
            }
        }

        // Đồng hồ đếm ngược chờ kết nối lại. Khi về 0 sẽ tự động gửi -9 và kết thúc form
        private void DisconnectWaitTimer_Tick(object? sender, EventArgs e)
        {
            disconnectWaitTime--;
            UpdateStatus($"Đang chờ đối thủ kết nối lại ({disconnectWaitTime}s)...");

            if (disconnectWaitTime <= 0)
            {
                disconnectWaitTimer?.Stop();

                SocketManager.Instance.Send("MOVE|-9|-9"); // Xử thua cho người kia
                isGameEnded = true;
                isClosing = true;
                MessageBox.Show("Đã hết 1 phút chờ đợi. Bạn được xử thắng do đối thủ đã bỏ cuộc!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
        }

        // Xử lý khi Socket bị lỗi ngắt đột ngột (Lỗi đường truyền Internet, sập server)
        public void Network_OnOpponentDisconnected()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => Network_OnOpponentDisconnected()));
                return;
            }

            if (isClosing) return;

            isGameEnded = true;
            isClosing = true;

            if (!this.Visible && FormLobby.Instance != null) FormLobby.Instance.HideReconnectPrompt();

            MessageBox.Show("Đường truyền bị mất kết nối (Mạng yếu/Rớt mạng). Trò chơi sẽ thoát về sảnh!", "Lỗi mạng", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                SocketManager.Instance.Send("MOVE|-3|-3");
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
                SocketManager.Instance.Send("MOVE|-2|-2");
                this.Close();
            };

            pnlRematch.Controls.Add(lblRematchMessage);
            pnlRematch.Controls.Add(btnRematchYes);
            pnlRematch.Controls.Add(btnRematchNo);
            this.Controls.Add(pnlRematch);
        }

        private void SetupTimerUI()
        {
            pnlTimerBackground = new Panel();
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

      

        private void InitializeComponent()
        {
            pnlChessBoard = new Panel();
            lblStatus = new Label();
            pnlStatusBorder = new Panel();
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
            lblStatus.BorderStyle = BorderStyle.None;
            lblStatus.Font = new Font("Arial", 9F);
            lblStatus.ForeColor = Color.DarkGreen;
            lblStatus.Location = new Point(2, 2);
            lblStatus.Name = "lblStatus";
            lblStatus.Padding = new Padding(5);
            lblStatus.Size = new Size(276, 46);
            lblStatus.TabIndex = 4;
            lblStatus.Text = "Sẵn sàng.";
            lblStatus.TextAlign = ContentAlignment.MiddleLeft;

            pnlStatusBorder.BackColor = statusBorderDefaultColor;
            pnlStatusBorder.BorderStyle = BorderStyle.None;
            pnlStatusBorder.Location = new Point(10, 230);
            pnlStatusBorder.Name = "pnlStatusBorder";
            pnlStatusBorder.Size = new Size(280, 50);
            pnlStatusBorder.Padding = new Padding(3);
            pnlStatusBorder.TabIndex = 4;
            pnlStatusBorder.Controls.Add(lblStatus);

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
            txtIP.Text = "";

            txtPort.Font = new Font("Arial", 9F);
            txtPort.Location = new Point(10, 125);
            txtPort.Name = "txtPort";
            txtPort.Size = new Size(280, 20);
            txtPort.TabIndex = 2;
            txtPort.Text = "";

            panel1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            panel1.AutoScroll = true;
            panel1.BackColor = Color.FromArgb(248, 248, 255);
            panel1.BorderStyle = BorderStyle.Fixed3D;
            panel1.Controls.Add(lblTitle);
            panel1.Controls.Add(lblPortLabel);
            panel1.Controls.Add(lblIPLabel);
            panel1.Controls.Add(label1);
            panel1.Controls.Add(txtPort);
            panel1.Controls.Add(pnlStatusBorder);
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
            if (panel1.ClientRectangle.Width <= 0 ||
                panel1.ClientRectangle.Height <= 0)
            {
                return;
            }

            using (Brush brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                panel1.ClientRectangle,
                Color.FromArgb(248, 248, 255),
                Color.FromArgb(230, 240, 250),
                System.Drawing.Drawing2D.LinearGradientMode.Vertical))
            {
                e.Graphics.FillRectangle(
                    brush,
                    panel1.ClientRectangle
                );
            }

            if (panel1.Width > 1 && panel1.Height > 1)
            {
                using (Pen pen = new Pen(
                    Color.FromArgb(70, 130, 180), 2))
                {
                    e.Graphics.DrawRectangle(
                        pen,
                        0,
                        0,
                        panel1.Width - 1,
                        panel1.Height - 1
                    );
                }
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