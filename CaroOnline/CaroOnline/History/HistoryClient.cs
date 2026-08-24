using System;
using System.Collections.Generic;

namespace CaroOnline.History
{
    public class HistoryClient
    {
        private readonly List<GameHistoryInfo> histories
            = new List<GameHistoryInfo>();

        // Them mot tran vao lich su
        public void AddHistory(GameHistoryInfo history)
        {
            if (history == null)
                return;

            histories.Add(history);
        }

        // Xoa toan bo lich su
        public void ClearHistory()
        {
            histories.Clear();
        }

        // Lay tat ca lich su
        public List<GameHistoryInfo> GetAllHistory()
        {
            return new List<GameHistoryInfo>(histories);
        }

        // Tim tran theo GameId
        public GameHistoryInfo GetById(string gameId)
        {
            foreach (GameHistoryInfo history in histories)
            {
                if (string.Equals(
                    history.GameId,
                    gameId,
                    StringComparison.OrdinalIgnoreCase))
                {
                    return history;
                }
            }

            return null;
        }

        // Tim lich su cua nguoi choi
        public List<GameHistoryInfo> GetByPlayer(
            string playerName)
        {
            List<GameHistoryInfo> result =
                new List<GameHistoryInfo>();

            foreach (GameHistoryInfo history in histories)
            {
                if (
                    string.Equals(
                        history.PlayerX,
                        playerName,
                        StringComparison.OrdinalIgnoreCase)
                    ||
                    string.Equals(
                        history.PlayerO,
                        playerName,
                        StringComparison.OrdinalIgnoreCase)
                )
                {
                    result.Add(history);
                }
            }

            return result;
        }

        // Xu ly message lich su tu server
        public void HandleServerMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;

            string[] parts = message.Split('|');

            if (parts.Length == 0)
                return;

            string command = parts[0];

            switch (command)
            {
                case "HISTORY":
                    ParseHistory(parts);
                    break;

                case "HISTORY_EMPTY":
                    ClearHistory();
                    break;

                case "HISTORY_ERROR":
                    Console.WriteLine(
                        $"History error: {string.Join("|", parts)}"
                    );
                    break;
            }
        }

        // Phan tich message HISTORY
        private void ParseHistory(string[] parts)
        {
            if (parts.Length < 7)
                return;

            GameHistoryInfo history =
                new GameHistoryInfo();

            history.GameId = parts[1];
            history.RoomId = parts[2];
            history.PlayerX = parts[3];
            history.PlayerO = parts[4];
            history.Winner = parts[5];

            DateTime startTime;

            if (DateTime.TryParse(
                parts[6],
                out startTime))
            {
                history.StartTime = startTime;
            }

            if (parts.Length >= 8)
            {
                DateTime endTime;

                if (DateTime.TryParse(
                    parts[7],
                    out endTime))
                {
                    history.EndTime = endTime;
                }
            }

            AddHistory(history);
        }
    }

    public class GameHistoryInfo
    {
        public string GameId { get; set; } = "";

        public string RoomId { get; set; } = "";

        public string PlayerX { get; set; } = "";

        public string PlayerO { get; set; } = "";

        public string Winner { get; set; } = "";

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public List<MoveInfo> Moves { get; set; }
            = new List<MoveInfo>();
    }

    public class MoveInfo
    {
        public int MoveNumber { get; set; }

        public string Player { get; set; } = "";

        public int Row { get; set; }

        public int Column { get; set; }

        public DateTime Time { get; set; }
    }
}