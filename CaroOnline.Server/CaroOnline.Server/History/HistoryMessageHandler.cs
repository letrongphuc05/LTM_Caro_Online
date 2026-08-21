using System;
using System.Collections.Generic;

namespace CaroOnline.Server.History
{
    internal class HistoryMessageHandler
    {
        private readonly HistoryQuery historyQuery;

        public HistoryMessageHandler(HistoryQuery historyQuery)
        {
            this.historyQuery = historyQuery;
        }

        public string HandleMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return "";

            string[] parts = message.Split('|');

            if (parts.Length == 0)
                return "";

            string command = parts[0];

            switch (command)
            {
                case "HISTORY_ALL":
                    return HandleGetAll();

                case "HISTORY_GAME":
                    if (parts.Length < 2)
                        return "HISTORY_ERROR|INVALID_GAME_ID";

                    return HandleGetByGameId(parts[1]);

                case "HISTORY_PLAYER":
                    if (parts.Length < 2)
                        return "HISTORY_ERROR|INVALID_PLAYER";

                    return HandleGetByPlayer(parts[1]);

                case "HISTORY_ROOM":
                    if (parts.Length < 2)
                        return "HISTORY_ERROR|INVALID_ROOM";

                    return HandleGetByRoomId(parts[1]);

                default:
                    return "UNKNOWN_COMMAND";
            }
        }

        private string HandleGetAll()
        {
            List<GameHistory> histories =
                historyQuery.GetAll();

            return BuildResponse(histories);
        }

        private string HandleGetByGameId(string gameId)
        {
            GameHistory history =
                historyQuery.GetByGameId(gameId);

            if (history == null)
                return "HISTORY_ERROR|NOT_FOUND";

            return BuildHistoryMessage(history);
        }

        private string HandleGetByPlayer(string playerName)
        {
            List<GameHistory> histories =
                historyQuery.GetByPlayer(playerName);

            return BuildResponse(histories);
        }

        private string HandleGetByRoomId(string roomId)
        {
            List<GameHistory> histories =
                historyQuery.GetByRoomId(roomId);

            return BuildResponse(histories);
        }

        private string BuildResponse(
            List<GameHistory> histories)
        {
            if (histories == null ||
                histories.Count == 0)
            {
                return "HISTORY_EMPTY";
            }

            List<string> result =
                new List<string>();

            foreach (GameHistory history in histories)
            {
                result.Add(
                    BuildHistoryMessage(history));
            }

            return string.Join("\n", result);
        }

        private string BuildHistoryMessage(
            GameHistory history)
        {
            return
                $"HISTORY|" +
                $"{history.GameId}|" +
                $"{history.RoomId}|" +
                $"{history.PlayerX}|" +
                $"{history.PlayerO}|" +
                $"{history.Winner}|" +
                $"{history.StartTime:O}|" +
                $"{history.EndTime:O}";
        }
    }
}