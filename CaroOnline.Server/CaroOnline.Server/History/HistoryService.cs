using System;
using System.Collections.Generic;

namespace CaroOnline.Server.History
{
    internal class HistoryService
    {
        private readonly HistoryStorage storage;
        private readonly List<GameHistory> histories;

        public HistoryService()
        {
            storage = new HistoryStorage();
            histories = storage.Load();
        }

        // Lay tat ca lich su
        public List<GameHistory> GetAll()
        {
            return new List<GameHistory>(histories);
        }

        // Them mot tran vao lich su
        public void AddGame(GameHistory history)
        {
            if (history == null)
                return;

            histories.Add(history);

            storage.Save(histories);
        }

        // Cap nhat mot tran
        public void UpdateGame(GameHistory history)
        {
            if (history == null)
                return;

            for (int i = 0; i < histories.Count; i++)
            {
                if (string.Equals(
                    histories[i].GameId,
                    history.GameId,
                    StringComparison.OrdinalIgnoreCase))
                {
                    histories[i] = history;
                    storage.Save(histories);
                    return;
                }
            }

            AddGame(history);
        }

        // Xoa lich su
        public void Clear()
        {
            histories.Clear();
            storage.Save(histories);
        }

        // Tim theo GameId
        public GameHistory GetByGameId(string gameId)
        {
            if (string.IsNullOrWhiteSpace(gameId))
                return null;

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

        // Tim theo RoomId
        public List<GameHistory> GetByRoomId(string roomId)
        {
            List<GameHistory> result =
                new List<GameHistory>();

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

        // Tim theo nguoi choi
        public List<GameHistory> GetByPlayer(string playerName)
        {
            List<GameHistory> result =
                new List<GameHistory>();

            foreach (GameHistory history in histories)
            {
                bool playerX =
                    string.Equals(
                        history.PlayerX,
                        playerName,
                        StringComparison.OrdinalIgnoreCase);

                bool playerO =
                    string.Equals(
                        history.PlayerO,
                        playerName,
                        StringComparison.OrdinalIgnoreCase);

                if (playerX || playerO)
                {
                    result.Add(history);
                }
            }

            return result;
        }
    }
}
