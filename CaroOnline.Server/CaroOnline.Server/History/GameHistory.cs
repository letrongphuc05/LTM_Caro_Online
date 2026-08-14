using System;
using System.Collections.Generic;

namespace CaroOnline.Server.History
{
    public class GameHistory
    {
        // Thong tin tran dau
        public string GameId { get; set; } = "";

        public string RoomId { get; set; } = "";

        public string PlayerX { get; set; } = "";

        public string PlayerO { get; set; } = "";

        // Ket qua
        public string Winner { get; set; } = "";

        public string Status { get; set; } = "PLAYING";

        // Thoi gian
        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        // Danh sach nuoc di
        public List<GameMove> Moves { get; set; }
            = new List<GameMove>();

        public GameHistory()
        {
            StartTime = DateTime.Now;
        }

        // Them mot nuoc di
        public void AddMove(
            string player,
            int row,
            int column)
        {
            if (string.IsNullOrWhiteSpace(player))
                return;

            GameMove move = new GameMove
            {
                MoveNumber = Moves.Count + 1,
                Player = player,
                Row = row,
                Column = column,
                Time = DateTime.Now
            };

            Moves.Add(move);
        }

        // Ket thuc tran
        public void FinishGame(string winner)
        {
            Winner = winner ?? "";
            Status = "FINISHED";
            EndTime = DateTime.Now;
        }
    }

    public class GameMove
    {
        public int MoveNumber { get; set; }

        public string Player { get; set; } = "";

        public int Row { get; set; }

        public int Column { get; set; }

        public DateTime Time { get; set; }
    }
}