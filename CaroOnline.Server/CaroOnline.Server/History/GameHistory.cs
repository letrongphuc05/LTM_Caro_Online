using System;
using System.Collections.Generic;

namespace CaroOnline.Server.History
{
    internal class GameHistory
    {
        public string GameId { get; set; } = "";

        public string RoomId { get; set; } = "";

        public string PlayerX { get; set; } = "";

        public string PlayerO { get; set; } = "";

        public string Winner { get; set; } = "";

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public List<GameMove> Moves { get; set; }
            = new List<GameMove>();

        public int MoveCount
        {
            get
            {
                return Moves.Count;
            }
        }
    }

    internal class GameMove
    {
        public int MoveNumber { get; set; }

        public string Player { get; set; } = "";

        public int Row { get; set; }

        public int Column { get; set; }

        public DateTime Time { get; set; }
    }
}