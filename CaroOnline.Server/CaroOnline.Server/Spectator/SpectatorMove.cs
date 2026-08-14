using System;

namespace CaroOnline.Server.Spectator
{
    public class SpectatorMove
    {
        // Thu tu nuoc di
        public int MoveNumber { get; set; }

        // Nguoi danh: X hoac O
        public string Player { get; set; } = "";

        // Vi tri hang
        public int Row { get; set; }

        // Vi tri cot
        public int Column { get; set; }

        // Thoi gian danh
        public DateTime Time { get; set; }

        public SpectatorMove()
        {
            Time = DateTime.Now;
        }

        public SpectatorMove(
            int moveNumber,
            string player,
            int row,
            int column)
        {
            MoveNumber = moveNumber;
            Player = player ?? "";
            Row = row;
            Column = column;
            Time = DateTime.Now;
        }

        // Chuyen nuoc di thanh message
        public string ToMessage()
        {
            return
                $"MOVE|{MoveNumber}|{Player}|{Row}|{Column}|{Time:O}";
        }
    }
}