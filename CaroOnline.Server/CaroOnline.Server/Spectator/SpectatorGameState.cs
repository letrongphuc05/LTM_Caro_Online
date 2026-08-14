using System;

namespace CaroOnline.Server.Spectator
{
    public class SpectatorGameState
    {
        // Ma phong
        public string RoomId { get; set; } = "";

        // Luot hien tai: X hoac O
        public string CurrentPlayer { get; set; } = "";

        // Trang thai tran dau
        public string Status { get; set; } = "PLAYING";

        // Nguoi thang
        public string Winner { get; set; } = "";

        // Ban co
        public string[,] Board { get; private set; }

        public SpectatorGameState()
        {
            Board = new string[15, 15];

            ClearBoard();
        }

        // Xoa ban co
        public void ClearBoard()
        {
            for (int row = 0; row < 15; row++)
            {
                for (int column = 0; column < 15; column++)
                {
                    Board[row, column] = "";
                }
            }
        }

        // Them mot nuoc di
        public bool AddMove(
            int row,
            int column,
            string player)
        {
            if (row < 0 || row >= 15)
                return false;

            if (column < 0 || column >= 15)
                return false;

            if (string.IsNullOrWhiteSpace(player))
                return false;

            // O nay da duoc danh
            if (!string.IsNullOrEmpty(
                Board[row, column]))
            {
                return false;
            }

            Board[row, column] = player;

            // Doi luot
            if (player.Equals(
                "X",
                StringComparison.OrdinalIgnoreCase))
            {
                CurrentPlayer = "O";
            }
            else
            {
                CurrentPlayer = "X";
            }

            return true;
        }

        // Lay gia tri tai mot o
        public string GetCell(
            int row,
            int column)
        {
            if (row < 0 || row >= 15)
                return "";

            if (column < 0 || column >= 15)
                return "";

            return Board[row, column];
        }

        // Ket thuc tran
        public void EndGame(string winner)
        {
            Status = "FINISHED";
            Winner = winner ?? "";
        }

        // Dat lai trang thai tran
        public void Reset()
        {
            ClearBoard();

            CurrentPlayer = "";
            Status = "PLAYING";
            Winner = "";
        }

        // Chuyen trang thai thanh message
        public string ToMessage()
        {
            string boardData = "";

            for (int row = 0; row < 15; row++)
            {
                for (int column = 0; column < 15; column++)
                {
                    string cell = Board[row, column];

                    if (string.IsNullOrEmpty(cell))
                    {
                        boardData += ".";
                    }
                    else
                    {
                        boardData += cell;
                    }
                }
            }

            return
                $"GAME_STATE|{RoomId}|{CurrentPlayer}|{Status}|{Winner}|{boardData}";
        }
    }
}