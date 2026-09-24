using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CaroOnline.Constants;

namespace CaroOnline.Logic
{
    public class BoardManager
    {
        private Panel chessBoard;
        private List<List<Button>> matrix;
        private int currentButtonWidth = 40;
        private int currentButtonHeight = 40;

        private Point lastOpponentMove = new Point(-1, -1);
        private Button? lastOpponentMoveButton = null;

        private System.Windows.Forms.Timer? resizeDebounceTimer;
        private int lastPanelWidth = 0;
        private int lastPanelHeight = 0;

        private System.Windows.Forms.Timer? turnTimer;
        private int timeRemaining = 30;

        // Biến đếm số nước đi để xét Hòa
        private int totalMoves = 0;

        public bool IsMyTurn { get; set; } = false;
        public string MySymbol { get; set; } = "X";
        public Point LastOpponentMove => lastOpponentMove;

        public List<Button> WinButtons { get; private set; } = new List<Button>();
        public Point WinLineStart { get; private set; } = Point.Empty;
        public Point WinLineEnd { get; private set; } = Point.Empty;

        public event EventHandler<Point>? PlayerMarked;
        public event EventHandler<string>? GameEnded;
        public event EventHandler? TimerExpired;
        public event EventHandler<int>? TimerTick;

        public BoardManager(Panel panel)
        {
            this.chessBoard = panel;
            this.matrix = new List<List<Button>>();
        }

        public void DrawChessBoard()
        {
            lastOpponentMove = new Point(-1, -1);
            lastOpponentMoveButton = null;
            WinButtons.Clear();

            // Reset đếm nước đi khi ván mới bắt đầu
            totalMoves = 0;

            chessBoard.SuspendLayout();

            // Nếu bàn cờ đã được tạo, chỉ reset Text để tối ưu, không tạo lại 400 nút
            if (matrix.Count == Cfg.CHESS_BOARD_HEIGHT)
            {
                for (int i = 0; i < Cfg.CHESS_BOARD_HEIGHT; i++)
                {
                    for (int j = 0; j < Cfg.CHESS_BOARD_WIDTH; j++)
                    {
                        Button btn = matrix[i][j];
                        btn.Text = "";
                        btn.Cursor = Cursors.Hand;
                        btn.BackColor = Color.FromArgb(245, 222, 179);
                        btn.FlatAppearance.BorderColor = Color.FromArgb(200, 165, 110);
                        btn.FlatAppearance.BorderSize = 1;

                        // Reset lại hiệu ứng hover mặc định
                        btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(235, 210, 170);
                        btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(210, 180, 130);

                        // XÓA ĐƯỜNG KẺ CHIẾN THẮNG KHI TÁI ĐẤU
                        btn.Paint -= WinButton_Paint;
                    }
                }
                chessBoard.ResumeLayout(true);
                return;
            }

            chessBoard.Controls.Clear();
            matrix.Clear();
            chessBoard.BackColor = Color.FromArgb(220, 190, 150);

            int panelWidth = chessBoard.Width <= 0 ? 1000 : chessBoard.Width;
            int panelHeight = chessBoard.Height <= 0 ? 900 : chessBoard.Height;

            int availableWidth = panelWidth - 40;
            int availableHeight = panelHeight - 40;

            int buttonWidthByWidth = availableWidth / Cfg.CHESS_BOARD_WIDTH;
            int buttonHeightByHeight = availableHeight / Cfg.CHESS_BOARD_HEIGHT;

            int buttonWidth = Math.Min(buttonWidthByWidth, buttonHeightByHeight);
            int buttonHeight = buttonWidth;

            if (buttonWidth < 30) buttonWidth = 30;
            if (buttonHeight < 30) buttonHeight = 30;

            currentButtonWidth = buttonWidth;
            currentButtonHeight = buttonHeight;

            int totalWidth = buttonWidth * Cfg.CHESS_BOARD_WIDTH;
            int totalHeight = buttonHeight * Cfg.CHESS_BOARD_HEIGHT;

            int startX = Math.Max(10, (panelWidth - totalWidth) / 2);
            int startY = Math.Max(10, (panelHeight - totalHeight) / 2);

            int currentY = startY;

            Font defaultFont = new Font("Arial", 11, FontStyle.Bold);

            for (int i = 0; i < Cfg.CHESS_BOARD_HEIGHT; i++)
            {
                matrix.Add(new List<Button>());
                int currentX = startX;

                for (int j = 0; j < Cfg.CHESS_BOARD_WIDTH; j++)
                {
                    Button btn = new Button()
                    {
                        Bounds = new Rectangle(currentX, currentY, buttonWidth, buttonHeight),
                        Tag = new Point(j, i),
                        Font = defaultFont,
                        FlatStyle = FlatStyle.Flat,
                        BackColor = Color.FromArgb(245, 222, 179),
                        ForeColor = Color.Black,
                        Cursor = Cursors.Hand,
                        Text = "",
                        Margin = new Padding(0)
                    };

                    btn.FlatAppearance.BorderSize = 1;
                    btn.FlatAppearance.BorderColor = Color.FromArgb(200, 165, 110);
                    btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(210, 180, 130);
                    btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(235, 210, 170);

                    btn.Click += Btn_Click;
                    chessBoard.Controls.Add(btn);
                    matrix[i].Add(btn);

                    currentX += buttonWidth;
                }
                currentY += buttonHeight;
            }
            chessBoard.ResumeLayout(true);
        }

        private void Btn_Click(object? sender, EventArgs e)
        {
            if (!IsMyTurn) return;

            Button? btn = sender as Button;
            if (btn == null || btn.Text != "") return;

            StopTurnTimer();
            Mark(btn, MySymbol);
            IsMyTurn = false;

            if (btn.Tag != null)
            {
                Point location = (Point)btn.Tag;
                PlayerMarked?.Invoke(this, location);
            }

            // Tăng biến đếm mỗi khi bạn đánh cờ
            totalMoves++;

            if (CheckWin(btn))
            {
                HighlightWin("YOU_WIN");
                StopTurnTimer();
                GameEnded?.Invoke(this, "YOU_WIN");
            }
            // Kiểm tra hòa: Đánh kín 400 ô (20x20) mà chưa Win thì báo Draw
            else if (totalMoves == Cfg.CHESS_BOARD_WIDTH * Cfg.CHESS_BOARD_HEIGHT)
            {
                StopTurnTimer();
                GameEnded?.Invoke(this, "DRAW");
            }
        }

        public void ReceiveOpponentMove(int x, int y)
        {
            Button btn = matrix[y][x];
            string opponentSymbol;

            if (string.IsNullOrEmpty(MySymbol))
            {
                int moveCount = 0;
                foreach (var row in matrix)
                {
                    foreach (Button b in row)
                    {
                        if (!string.IsNullOrEmpty(b.Text))
                            moveCount++;
                    }
                }
                opponentSymbol = (moveCount % 2 == 0) ? "X" : "O";
            }
            else
            {
                opponentSymbol = MySymbol == "X" ? "O" : "X";
            }

            Mark(btn, opponentSymbol);

            lastOpponentMove = new Point(x, y);

            HighlightLastMove(btn);

            if (!string.IsNullOrEmpty(MySymbol))
            {
                IsMyTurn = true;
                StartTurnTimer();
            }

            // Tăng biến đếm mỗi khi đối thủ đánh cờ
            totalMoves++;

            if (CheckWin(btn))
            {
                HighlightWin("YOU_LOSE");
                GameEnded?.Invoke(this, "YOU_LOSE");
            }
            // Kiểm tra hòa cho đối thủ
            else if (totalMoves == Cfg.CHESS_BOARD_WIDTH * Cfg.CHESS_BOARD_HEIGHT)
            {
                GameEnded?.Invoke(this, "DRAW");
            }
        }

        public void HighlightWin(string result)
        {
            Color winBackColor = (result == "YOU_WIN") ? Color.LimeGreen : Color.Crimson;
            Color winForeColor = Color.White;

            foreach (Button btn in WinButtons)
            {
                btn.BackColor = winBackColor;
                btn.ForeColor = winForeColor;
                btn.FlatAppearance.BorderColor = Color.Black;
                btn.FlatAppearance.BorderSize = 2;

                // Gắn sự kiện vẽ đường kẻ đè lên nút
                btn.Paint -= WinButton_Paint;
                btn.Paint += WinButton_Paint;
            }
        }

        // Highlight nước mới và xóa Highlight nước cũ của đối thủ
        private void HighlightLastMove(Button? btn)
        {
            if (btn == null) return;

            // Xóa màu viền và nền của ô cờ cũ trước đó
            if (lastOpponentMoveButton != null && lastOpponentMoveButton != btn)
            {
                if (lastOpponentMoveButton.Text == "X")
                {
                    lastOpponentMoveButton.BackColor = Color.FromArgb(255, 245, 238);
                    lastOpponentMoveButton.FlatAppearance.BorderColor = Color.FromArgb(200, 100, 100);
                    lastOpponentMoveButton.FlatAppearance.BorderSize = 1;
                    lastOpponentMoveButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(255, 245, 238);
                }
                else if (lastOpponentMoveButton.Text == "O")
                {
                    lastOpponentMoveButton.BackColor = Color.FromArgb(240, 248, 255);
                    lastOpponentMoveButton.FlatAppearance.BorderColor = Color.FromArgb(100, 149, 237);
                    lastOpponentMoveButton.FlatAppearance.BorderSize = 1;
                    lastOpponentMoveButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(240, 248, 255);
                }
            }

            // Tô màu xanh lá và viền đậm cho ô cờ mới nhất
            btn.BackColor = Color.FromArgb(144, 238, 144);
            btn.FlatAppearance.BorderSize = 2;
            btn.FlatAppearance.BorderColor = Color.FromArgb(34, 139, 34);
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(144, 238, 144);

            // Ghi nhớ lại ô vừa đánh để xoá ở lượt sau
            lastOpponentMoveButton = btn;
        }

        public void OnBoardResized()
        {
            if (chessBoard.Width == lastPanelWidth && chessBoard.Height == lastPanelHeight)
                return;

            if (resizeDebounceTimer != null)
            {
                resizeDebounceTimer.Stop();
                resizeDebounceTimer.Dispose();
            }

            resizeDebounceTimer = new System.Windows.Forms.Timer();
            resizeDebounceTimer.Interval = 300;
            resizeDebounceTimer.Tick += (s, e) =>
            {
                resizeDebounceTimer?.Stop();
                RedrawBoardOnResize();
            };
            resizeDebounceTimer.Start();
        }

        public void RedrawBoardOnResize()
        {
            if (matrix.Count == 0) return;

            if (resizeDebounceTimer != null)
            {
                resizeDebounceTimer.Stop();
                resizeDebounceTimer.Dispose();
                resizeDebounceTimer = null;
            }

            int panelWidth = chessBoard.Width;
            int panelHeight = chessBoard.Height;
            if (panelWidth <= 0 || panelHeight <= 0) return;

            lastPanelWidth = panelWidth;
            lastPanelHeight = panelHeight;

            int availableWidth = panelWidth - 40;
            int availableHeight = panelHeight - 40;

            int buttonWidthByWidth = availableWidth / Cfg.CHESS_BOARD_WIDTH;
            int buttonHeightByHeight = availableHeight / Cfg.CHESS_BOARD_HEIGHT;

            int buttonWidth = Math.Min(buttonWidthByWidth, buttonHeightByHeight);
            int buttonHeight = buttonWidth;
            if (buttonWidth < 30) buttonWidth = 30;
            if (buttonHeight < 30) buttonHeight = 30;

            if (currentButtonWidth == buttonWidth && currentButtonHeight == buttonHeight) return;

            currentButtonWidth = buttonWidth;
            currentButtonHeight = buttonHeight;

            int totalWidth = buttonWidth * Cfg.CHESS_BOARD_WIDTH;
            int totalHeight = buttonHeight * Cfg.CHESS_BOARD_HEIGHT;
            int startX = Math.Max(10, (panelWidth - totalWidth) / 2);
            int startY = Math.Max(10, (panelHeight - totalHeight) / 2);

            chessBoard.SuspendLayout();

            float newSize = Math.Max(8f, buttonWidth / 3.5f);
            Font newFont = new Font("Arial", newSize, FontStyle.Bold);

            for (int i = 0; i < Cfg.CHESS_BOARD_HEIGHT; i++)
            {
                for (int j = 0; j < Cfg.CHESS_BOARD_WIDTH; j++)
                {
                    Button btn = matrix[i][j];

                    Rectangle newBounds = new Rectangle(startX + (j * buttonWidth), startY + (i * buttonHeight), buttonWidth, buttonHeight);
                    if (btn.Bounds != newBounds)
                    {
                        btn.Bounds = newBounds;
                    }

                    if (Math.Abs(btn.Font.Size - newSize) > 0.1f)
                    {
                        btn.Font = newFont;
                    }
                }
            }

            chessBoard.ResumeLayout(true);
        }

        private void Mark(Button btn, string symbol)
        {
            btn.Text = symbol;
            // Đã bỏ dòng btn.Enabled = false; để không bị mờ màu
            btn.Cursor = Cursors.Default;

            if (symbol == "X")
            {
                btn.ForeColor = Color.FromArgb(178, 34, 34);
                btn.Font = new Font("Arial", 13, FontStyle.Bold);
                btn.BackColor = Color.FromArgb(255, 245, 238);
                btn.FlatAppearance.BorderColor = Color.FromArgb(200, 100, 100);

                // Tắt hiệu ứng đổi màu khi rà chuột qua nút X
                btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(255, 245, 238);
                btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(255, 245, 238);
            }
            else
            {
                btn.ForeColor = Color.FromArgb(25, 25, 112);
                btn.Font = new Font("Arial", 13, FontStyle.Bold);
                btn.BackColor = Color.FromArgb(240, 248, 255);
                btn.FlatAppearance.BorderColor = Color.FromArgb(100, 149, 237);

                // Tắt hiệu ứng đổi màu khi rà chuột qua nút O
                btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(240, 248, 255);
                btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(240, 248, 255);
            }
        }

        private bool CheckWin(Button btn)
        {
            return isEndHorizontal(btn) || isEndVertical(btn) || isEndPrimary(btn) || isEndSub(btn);
        }

        private Point GetPoint(Button btn) => btn.Tag != null ? (Point)btn.Tag : new Point(0, 0);

        private bool isEndHorizontal(Button btn)
        {
            Point point = GetPoint(btn);
            List<Button> tempWin = new List<Button>();

            for (int i = point.X; i >= 0; i--)
            {
                if (matrix[point.Y][i].Text == btn.Text) tempWin.Add(matrix[point.Y][i]);
                else break;
            }
            for (int i = point.X + 1; i < Cfg.CHESS_BOARD_WIDTH; i++)
            {
                if (matrix[point.Y][i].Text == btn.Text) tempWin.Add(matrix[point.Y][i]);
                else break;
            }

            if (tempWin.Count >= 5)
            {
                WinButtons = tempWin;
                return true;
            }
            return false;
        }

        private bool isEndVertical(Button btn)
        {
            Point point = GetPoint(btn);
            List<Button> tempWin = new List<Button>();

            for (int i = point.Y; i >= 0; i--)
            {
                if (matrix[i][point.X].Text == btn.Text) tempWin.Add(matrix[i][point.X]);
                else break;
            }
            for (int i = point.Y + 1; i < Cfg.CHESS_BOARD_HEIGHT; i++)
            {
                if (matrix[i][point.X].Text == btn.Text) tempWin.Add(matrix[i][point.X]);
                else break;
            }

            if (tempWin.Count >= 5)
            {
                WinButtons = tempWin;
                return true;
            }
            return false;
        }

        private bool isEndPrimary(Button btn)
        {
            Point point = GetPoint(btn);
            List<Button> tempWin = new List<Button>();

            for (int i = 0; i <= point.X; i++)
            {
                if (point.X - i < 0 || point.Y - i < 0) break;
                if (matrix[point.Y - i][point.X - i].Text == btn.Text) tempWin.Add(matrix[point.Y - i][point.X - i]);
                else break;
            }
            for (int i = 1; i <= Cfg.CHESS_BOARD_WIDTH - point.X; i++)
            {
                if (point.Y + i >= Cfg.CHESS_BOARD_HEIGHT || point.X + i >= Cfg.CHESS_BOARD_WIDTH) break;
                if (matrix[point.Y + i][point.X + i].Text == btn.Text) tempWin.Add(matrix[point.Y + i][point.X + i]);
                else break;
            }

            if (tempWin.Count >= 5)
            {
                WinButtons = tempWin;
                return true;
            }
            return false;
        }

        private bool isEndSub(Button btn)
        {
            Point point = GetPoint(btn);
            List<Button> tempWin = new List<Button>();

            for (int i = 0; i <= point.X; i++)
            {
                if (point.X - i < 0 || point.Y + i >= Cfg.CHESS_BOARD_HEIGHT) break;
                if (matrix[point.Y + i][point.X - i].Text == btn.Text) tempWin.Add(matrix[point.Y + i][point.X - i]);
                else break;
            }
            for (int i = 1; i <= Cfg.CHESS_BOARD_WIDTH - point.X; i++)
            {
                if (point.Y - i < 0 || point.X + i >= Cfg.CHESS_BOARD_WIDTH) break;
                if (matrix[point.Y - i][point.X + i].Text == btn.Text) tempWin.Add(matrix[point.Y - i][point.X + i]);
                else break;
            }

            if (tempWin.Count >= 5)
            {
                WinButtons = tempWin;
                return true;
            }
            return false;
        }

        // HÀM VẼ ĐƯỜNG KẺ CHIẾN THẮNG TRỰC TIẾP LÊN NÚT
        private void WinButton_Paint(object? sender, PaintEventArgs e)
        {
            Button? btn = sender as Button;
            if (btn == null || WinButtons.Count < 5) return;

            // Bật chế độ khử răng cưa để đường kẻ chéo trông mượt mà
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            Point first = (Point)WinButtons[0].Tag;
            Point last = (Point)WinButtons[WinButtons.Count - 1].Tag;

            // Bút vẽ: Màu đen, độ dày 5px
            using (Pen linePen = new Pen(Color.Black, 5))
            {
                int w = btn.Width;
                int h = btn.Height;

                if (first.Y == last.Y) // Ngang
                {
                    e.Graphics.DrawLine(linePen, 0, h / 2, w, h / 2);
                }
                else if (first.X == last.X) // Dọc
                {
                    e.Graphics.DrawLine(linePen, w / 2, 0, w / 2, h);
                }
                else if ((first.X < last.X && first.Y < last.Y) || (first.X > last.X && first.Y > last.Y))
                {
                    // Chéo chính (\)
                    e.Graphics.DrawLine(linePen, 0, 0, w, h);
                }
                else
                {
                    // Chéo phụ (/)
                    e.Graphics.DrawLine(linePen, 0, h, w, 0);
                }
            }
        }

        public void StartTurnTimer()
        {
            StopTurnTimer();
            turnTimer = new System.Windows.Forms.Timer();
            turnTimer.Interval = 1000;
            timeRemaining = 30;
            turnTimer.Tick += TurnTimer_Tick;
            turnTimer.Start();
        }

        public void StopTurnTimer()
        {
            if (turnTimer != null)
            {
                turnTimer.Stop();
                turnTimer.Dispose();
                turnTimer = null;
            }
        }

        private void TurnTimer_Tick(object? sender, EventArgs e)
        {
            timeRemaining--;
            TimerTick?.Invoke(this, timeRemaining);

            if (timeRemaining <= 0)
            {
                StopTurnTimer();
                IsMyTurn = false;
                TimerExpired?.Invoke(this, EventArgs.Empty);
            }
        }

        public int GetTimeRemaining() => Math.Max(0, timeRemaining);
    }
}