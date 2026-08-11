using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace CaroOnline.Server.History
{
    internal class HistoryManager
    {
        private readonly string filePath;

        private readonly List<GameHistory> histories;

        public HistoryManager()
        {
            string dataFolder =
                Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "Data"
                );

            Directory.CreateDirectory(dataFolder);

            filePath =
                Path.Combine(
                    dataFolder,
                    "game_history.json"
                );

            histories = Load();
        }

        // Tao tran moi
        public GameHistory StartGame(
            string roomId,
            string playerX,
            string playerO)
        {
            GameHistory game = new GameHistory
            {
                GameId = Guid.NewGuid().ToString(),
                RoomId = roomId,
                PlayerX = playerX,
                PlayerO = playerO,
                StartTime = DateTime.Now
            };

            histories.Add(game);

            Save();

            return game;
        }

        // Luu mot nuoc di
        public void AddMove(
            GameHistory game,
            string player,
            int row,
            int column)
        {
            if (game == null)
                return;

            GameMove move = new GameMove
            {
                MoveNumber = game.Moves.Count + 1,
                Player = player,
                Row = row,
                Column = column,
                Time = DateTime.Now
            };

            game.Moves.Add(move);

            Save();
        }

        // Ket thuc tran
        public void FinishGame(
            GameHistory game,
            string winner)
        {
            if (game == null)
                return;

            game.Winner = winner;
            game.EndTime = DateTime.Now;

            Save();
        }

        // Lay tat ca lich su
        public List<GameHistory> GetAll()
        {
            return new List<GameHistory>(histories);
        }

        // Lay lich su theo nguoi choi
        public List<GameHistory> GetByPlayer(string player)
        {
            return histories
                .Where(x =>
                    x.PlayerX.Equals(
                        player,
                        StringComparison.OrdinalIgnoreCase
                    )
                    ||
                    x.PlayerO.Equals(
                        player,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                .ToList();
        }

        // Tim tran theo ID
        public GameHistory GetById(string gameId)
        {
            return histories.FirstOrDefault(
                x => x.GameId == gameId
            );
        }

        // Luu du lieu vao JSON
        private void Save()
        {
            try
            {
                JsonSerializerOptions options =
                    new JsonSerializerOptions
                    {
                        WriteIndented = true
                    };

                string json =
                    JsonSerializer.Serialize(
                        histories,
                        options
                    );

                File.WriteAllText(
                    filePath,
                    json
                );
            }
            catch
            {
            }
        }

        // Doc du lieu tu JSON
        private List<GameHistory> Load()
        {
            try
            {
                if (!File.Exists(filePath))
                    return new List<GameHistory>();

                string json =
                    File.ReadAllText(filePath);

                return JsonSerializer.Deserialize<
                    List<GameHistory>
                >(json)
                ?? new List<GameHistory>();
            }
            catch
            {
                return new List<GameHistory>();
            }
        }
    }
}