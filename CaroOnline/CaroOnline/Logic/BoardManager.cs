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

        public bool IsMyTurn { get; set; } = false;
        public string MySymbol { get; set; } = "X";

        // Thêm dấu ? để sửa lỗi Non-nullable event
        public event EventHandler<Point>? PlayerMarked;
        public event EventHandler<string>? GameEnded;

        public BoardManager(Panel panel)
        {
            this.chessBoard = panel;
            this.matrix = new List<List<Button>>(); // Khởi tạo matrix ngay trong constructor
        }

        public void DrawChessBoard()
        {
            chessBoard.Controls.Clear();
            matrix.Clear();

            // Board background
            chessBoard.BackColor = Color.FromArgb(220, 190, 150);

            // Fixed size buttons - 40x40 px
            int buttonWidth = Cfg.CHESS_WIDTH;
            int buttonHeight = Cfg.CHESS_HEIGHT;

            // Calculate total board size
            int totalWidth = buttonWidth * Cfg.CHESS_BOARD_WIDTH;
            int totalHeight = buttonHeight * Cfg.CHESS_BOARD_HEIGHT;

            // Get panel size, ensure it's valid
            int panelWidth = chessBoard.Width;
            int panelHeight = chessBoard.Height;

            // If panel size is invalid/zero, use default
            if (panelWidth <= 0) panelWidth = 1000;
            if (panelHeight <= 0) panelHeight = 900;

            // Calculate starting position to center board on panel
            int startX = Math.Max(10, (panelWidth - totalWidth) / 2);
            int startY = Math.Max(10, (panelHeight - totalHeight) / 2);

            Button oldButton = new Button() { Width = 0, Location = new Point(startX, startY) };

            for (int i = 0; i < Cfg.CHESS_BOARD_HEIGHT; i++)
            {
                matrix.Add(new List<Button>());
                for (int j = 0; j < Cfg.CHESS_BOARD_WIDTH; j++)
                {
                    Button btn = new Button()
                    {
                        Width = buttonWidth,
                        Height = buttonHeight,
                        Location = new Point(oldButton.Location.X + oldButton.Width, oldButton.Location.Y),
                        Tag = new Point(j, i),
                        Font = new Font("Arial", 11, FontStyle.Bold),
                        FlatStyle = FlatStyle.Flat,
                        BackColor = Color.FromArgb(245, 222, 179), // Wheat color
                        ForeColor = Color.Black,
                        Cursor = Cursors.Hand,
                        Text = ""
                    };

                    // Border styling
                    btn.FlatAppearance.BorderSize = 1;
                    btn.FlatAppearance.BorderColor = Color.FromArgb(200, 165, 110);
                    btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(210, 180, 130);
                    btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(235, 210, 170);
                    btn.Margin = new Padding(0);

                    btn.Click += Btn_Click;
                    chessBoard.Controls.Add(btn);
                    matrix[i].Add(btn);
                    oldButton = btn;
                }
                // Move to next row
                oldButton.Location = new Point(startX, oldButton.Location.Y + buttonHeight);
                oldButton.Width = 0;
                oldButton.Height = 0;
            }
        }

        // Thêm object? sender để sửa lỗi Delegate mismatch
        private void Btn_Click(object? sender, EventArgs e)
        {
            if (!IsMyTurn) return;

            Button? btn = sender as Button;
            if (btn == null || btn.Text != "") return;

            Mark(btn, MySymbol);
            IsMyTurn = false;

            if (btn.Tag != null)
            {
                Point location = (Point)btn.Tag;
                PlayerMarked?.Invoke(this, location);
            }

            if (CheckWin(btn))
            {
                GameEnded?.Invoke(this, "YOU_WIN");
            }
        }

        public void ReceiveOpponentMove(int x, int y)
        {
            Button btn = matrix[y][x];
            string opponentSymbol = MySymbol == "X" ? "O" : "X";

            Mark(btn, opponentSymbol);
            IsMyTurn = true;

            if (CheckWin(btn))
            {
                GameEnded?.Invoke(this, "YOU_LOSE");
            }
        }

        private void Mark(Button btn, string symbol)
        {
            btn.Text = symbol;
            btn.Enabled = false;
            btn.Cursor = Cursors.Default;

            if (symbol == "X")
            {
                btn.ForeColor = Color.FromArgb(178, 34, 34); // Firebrick red
                btn.Font = new Font("Arial", 13, FontStyle.Bold);
                btn.BackColor = Color.FromArgb(255, 245, 238); // Seashell - very light red
                btn.FlatAppearance.BorderColor = Color.FromArgb(200, 100, 100);
            }
            else // "O"
            {
                btn.ForeColor = Color.FromArgb(25, 25, 112); // Midnight blue
                btn.Font = new Font("Arial", 13, FontStyle.Bold);
                btn.BackColor = Color.FromArgb(240, 248, 255); // Alice blue - very light blue
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
            int countLeft = 0, countRight = 0;

            for (int i = point.X; i >= 0; i--)
            {
                if (matrix[point.Y][i].Text == btn.Text) countLeft++;
                else break;
            }
            for (int i = point.X + 1; i < Cfg.CHESS_BOARD_WIDTH; i++)
            {
                if (matrix[point.Y][i].Text == btn.Text) countRight++;
                else break;
            }
            return countLeft + countRight >= 5;
        }

        private bool isEndVertical(Button btn)
        {
            Point point = GetPoint(btn);
            int countTop = 0, countBottom = 0;

            for (int i = point.Y; i >= 0; i--)
            {
                if (matrix[i][point.X].Text == btn.Text) countTop++;
                else break;
            }
            for (int i = point.Y + 1; i < Cfg.CHESS_BOARD_HEIGHT; i++)
            {
                if (matrix[i][point.X].Text == btn.Text) countBottom++;
                else break;
            }
            return countTop + countBottom >= 5;
        }

        private bool isEndPrimary(Button btn)
        {
            Point point = GetPoint(btn);
            int countTop = 0, countBottom = 0;

            for (int i = 0; i <= point.X; i++)
            {
                if (point.X - i < 0 || point.Y - i < 0) break;
                if (matrix[point.Y - i][point.X - i].Text == btn.Text) countTop++;
                else break;
            }
            for (int i = 1; i <= Cfg.CHESS_BOARD_WIDTH - point.X; i++)
            {
                if (point.Y + i >= Cfg.CHESS_BOARD_HEIGHT || point.X + i >= Cfg.CHESS_BOARD_WIDTH) break;
                if (matrix[point.Y + i][point.X + i].Text == btn.Text) countBottom++;
                else break;
            }
            return countTop + countBottom >= 5;
        }

        private bool isEndSub(Button btn)
        {
            Point point = GetPoint(btn);
            int countTop = 0, countBottom = 0;

            for (int i = 0; i <= point.X; i++)
            {
                if (point.X - i < 0 || point.Y + i >= Cfg.CHESS_BOARD_HEIGHT) break;
                if (matrix[point.Y + i][point.X - i].Text == btn.Text) countTop++;
                else break;
            }
            for (int i = 1; i <= Cfg.CHESS_BOARD_WIDTH - point.X; i++)
            {
                if (point.Y - i < 0 || point.X + i >= Cfg.CHESS_BOARD_WIDTH) break;
                if (matrix[point.Y - i][point.X + i].Text == btn.Text) countBottom++;
                else break;
            }
            return countTop + countBottom >= 5;
        }
    }
}