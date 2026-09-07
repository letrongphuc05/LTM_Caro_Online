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

        public bool IsMyTurn { get; set; } = false;
        public string MySymbol { get; set; } = "X";
        public Point LastOpponentMove => lastOpponentMove;

        // CHUẨN MỚI: Danh sách lưu lại đúng 5 ô chiến thắng để đổi màu nổi bật
        public List<Button> WinButtons { get; private set; } = new List<Button>();

        // Giữ lại 2 biến này rỗng (Empty) để tương thích, FormMain không cần dùng Graphics để vẽ đè nữa
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

            // BẬT CHẾ ĐỘ CHỐNG LAG GIAO DIỆN
            chessBoard.SuspendLayout();

            // TỐI ƯU TÁI ĐẤU: Nếu bàn cờ đã được tạo, chỉ reset Text thay vì hủy và tạo lại 400 nút (Tránh lag)
            if (matrix.Count == Cfg.CHESS_BOARD_HEIGHT)
            {
                for (int i = 0; i < Cfg.CHESS_BOARD_HEIGHT; i++)
                {
                    for (int j = 0; j < Cfg.CHESS_BOARD_WIDTH; j++)
                    {
                        Button btn = matrix[i][j];
                        btn.Text = "";
                        btn.Enabled = true;
                        btn.Cursor = Cursors.Hand;
                        btn.BackColor = Color.FromArgb(245, 222, 179);
                        btn.FlatAppearance.BorderColor = Color.FromArgb(200, 165, 110);
                        btn.FlatAppearance.BorderSize = 1;
                    }
                }
                chessBoard.ResumeLayout(true);
                return;
            }

            // Nếu là lần đầu chạy (chưa có nút nào) thì mới khởi tạo
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

            // TỐI ƯU BỘ NHỚ RAM: Chỉ tạo 1 đối tượng Font dùng chung cho 400 ô
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

            if (CheckWin(btn))
            {
                HighlightWin("YOU_WIN");
                StopTurnTimer();
                GameEnded?.Invoke(this, "YOU_WIN");
            }
        }

        public void ReceiveOpponentMove(int x, int y)
        {
            Button btn = matrix[y][x];
            string opponentSymbol = MySymbol == "X" ? "O" : "X";

            Mark(btn, opponentSymbol);

            lastOpponentMove = new Point(x, y);
            lastOpponentMoveButton = btn;

            HighlightLastMove(btn);

            IsMyTurn = true;
            StartTurnTimer();

            if (CheckWin(btn))
            {
                HighlightWin("YOU_LOSE");
                GameEnded?.Invoke(this, "YOU_LOSE");
            }
        }

        public void HighlightWin(string result)
        {
            // Bôi Xanh nếu thắng, Bôi Đỏ nếu thua trực tiếp lên Button
            Color winBackColor = (result == "YOU_WIN") ? Color.LimeGreen : Color.Crimson;
            Color winForeColor = Color.White;

            foreach (Button btn in WinButtons)
            {
                btn.BackColor = winBackColor;
                btn.ForeColor = winForeColor;
                btn.FlatAppearance.BorderColor = Color.Black;
                btn.FlatAppearance.BorderSize = 2;
            }
        }

        private void HighlightLastMove(Button? btn)
        {
            if (btn == null) return;
            btn.BackColor = Color.FromArgb(144, 238, 144);
            btn.FlatAppearance.BorderSize = 2;
            btn.FlatAppearance.BorderColor = Color.FromArgb(34, 139, 34);
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
            resizeDebounceTimer.Interval = 300; // Tăng delay lên 300ms để đợi người dùng thả hẳn chuột ra
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

            // Bỏ qua nếu kích thước không thực sự thay đổi
            if (currentButtonWidth == buttonWidth && currentButtonHeight == buttonHeight) return;

            currentButtonWidth = buttonWidth;
            currentButtonHeight = buttonHeight;

            int totalWidth = buttonWidth * Cfg.CHESS_BOARD_WIDTH;
            int totalHeight = buttonHeight * Cfg.CHESS_BOARD_HEIGHT;
            int startX = Math.Max(10, (panelWidth - totalWidth) / 2);
            int startY = Math.Max(10, (panelHeight - totalHeight) / 2);

            chessBoard.SuspendLayout();

            // TỐI ƯU CỰC KỲ QUAN TRỌNG: Chỉ tạo 1 đối tượng Font mới dùng chung khi Zoom
            float newSize = Math.Max(8f, buttonWidth / 3.5f);
            Font newFont = new Font("Arial", newSize, FontStyle.Bold);

            for (int i = 0; i < Cfg.CHESS_BOARD_HEIGHT; i++)
            {
                for (int j = 0; j < Cfg.CHESS_BOARD_WIDTH; j++)
                {
                    Button btn = matrix[i][j];

                    // Gom chung vào 1 hàm set Bounds thay vì set rời rạc Width/Height/Location
                    Rectangle newBounds = new Rectangle(startX + (j * buttonWidth), startY + (i * buttonHeight), buttonWidth, buttonHeight);
                    if (btn.Bounds != newBounds)
                    {
                        btn.Bounds = newBounds;
                    }

                    // Chỉ gán Font mới nếu size bị lệch để tránh giật
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
            btn.Enabled = false;
            btn.Cursor = Cursors.Default;

            if (symbol == "X")
            {
                btn.ForeColor = Color.FromArgb(178, 34, 34);
                btn.Font = new Font("Arial", 13, FontStyle.Bold);
                btn.BackColor = Color.FromArgb(255, 245, 238);
                btn.FlatAppearance.BorderColor = Color.FromArgb(200, 100, 100);
            }
            else
            {
                btn.ForeColor = Color.FromArgb(25, 25, 112);
                btn.Font = new Font("Arial", 13, FontStyle.Bold);
                btn.BackColor = Color.FromArgb(240, 248, 255);
                btn.FlatAppearance.BorderColor = Color.FromArgb(100, 149, 237);
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