using System;
using System.Collections.Generic;

namespace CaroOnline.Server.History
{
    internal class HistoryQuery
    {
        private readonly HistoryStorage storage;

        public HistoryQuery(HistoryStorage storage)
        {
            this.storage = storage;
        }

        // Lay toan bo lich su
        public List<GameHistory> GetAll()
        {
            return storage.Load();
        }

        // Tim tran theo GameId
        public GameHistory GetByGameId(string gameId)
        {
            if (string.IsNullOrWhiteSpace(gameId))
                return null;

            List<GameHistory> histories =
                storage.Load();

            foreach (GameHistory history in histories)
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

        // Tim lich su theo RoomId
        public List<GameHistory> GetByRoomId(string roomId)
        {
            List<GameHistory> result =
                new List<GameHistory>();

            if (string.IsNullOrWhiteSpace(roomId))
                return result;

            List<GameHistory> histories =
                storage.Load();

            foreach (GameHistory history in histories)
            {
                if (string.Equals(
                    history.RoomId,
                    roomId,
                    StringComparison.OrdinalIgnoreCase))
                {
                    result.Add(history);
                }
            }

            return result;
        }

        // Tim lich su cua nguoi choi
        public List<GameHistory> GetByPlayer(string playerName)
        {
            List<GameHistory> result =
                new List<GameHistory>();

            if (string.IsNullOrWhiteSpace(playerName))
                return result;

            List<GameHistory> histories =
                storage.Load();

            foreach (GameHistory history in histories)
            {
                bool isPlayerX =
                    string.Equals(
                        history.PlayerX,
                        playerName,
                        StringComparison.OrdinalIgnoreCase);

                bool isPlayerO =
                    string.Equals(
                        history.PlayerO,
                        playerName,
                        StringComparison.OrdinalIgnoreCase);

                if (isPlayerX || isPlayerO)
                {
                    result.Add(history);
                }
            }

            return result;
        }

        // Tim cac tran da ket thuc
        public List<GameHistory> GetFinishedGames()
        {
            List<GameHistory> result =
                new List<GameHistory>();

            List<GameHistory> histories =
                storage.Load();

            foreach (GameHistory history in histories)
            {
                if (string.Equals(
                    history.Status,
                    "FINISHED",
                    StringComparison.OrdinalIgnoreCase))
                {
                    result.Add(history);
                }
            }

            return result;
        }
    }
}