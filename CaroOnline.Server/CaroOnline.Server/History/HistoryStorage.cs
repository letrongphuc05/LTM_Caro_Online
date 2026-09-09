using System;
using System.Collections.Generic;

namespace CaroOnline.Server.History
{
    internal class HistoryStorage
    {
        // Lịch sử chỉ tồn tại trong RAM của phiên Server hiện tại.
        // Khi Server dừng, ServerController sẽ gọi Clear() và toàn bộ lịch sử biến mất.
        private static readonly object syncRoot = new object();
        private static List<GameHistory> runtimeHistories = new List<GameHistory>();

        public HistoryStorage(string filePath = "game_history.json")
        {
            // Giữ tham số để tương thích với code cũ. Không ghi file.
        }

        public void Save(List<GameHistory> histories)
        {
            if (histories == null)
                return;

            lock (syncRoot)
            {
                runtimeHistories = new List<GameHistory>(histories);
            }
        }

        public List<GameHistory> Load()
        {
            lock (syncRoot)
            {
                return new List<GameHistory>(runtimeHistories);
            }
        }

        public void Clear()
        {
            lock (syncRoot)
            {
                runtimeHistories.Clear();
            }
        }

        public bool Exists()
        {
            lock (syncRoot)
            {
                return runtimeHistories.Count > 0;
            }
        }
    }
}
