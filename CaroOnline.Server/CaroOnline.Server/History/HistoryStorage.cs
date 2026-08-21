using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace CaroOnline.Server.History
{
    internal class HistoryStorage
    {
        private readonly string filePath;

        public HistoryStorage(string filePath = "game_history.json")
        {
            this.filePath = filePath;
        }

        // Luu danh sach lich su vao file JSON
        public void Save(List<GameHistory> histories)
        {
            if (histories == null)
                return;

            try
            {
                string json = JsonSerializer.Serialize(
                    histories,
                    new JsonSerializerOptions
                    {
                        WriteIndented = true
                    });

                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Khong the luu lich su: {ex.Message}");
            }
        }

        // Doc lich su tu file JSON
        public List<GameHistory> Load()
        {
            try
            {
                if (!File.Exists(filePath))
                    return new List<GameHistory>();

                string json =
                    File.ReadAllText(filePath);

                if (string.IsNullOrWhiteSpace(json))
                    return new List<GameHistory>();

                List<GameHistory>? histories =
                    JsonSerializer.Deserialize<List<GameHistory>>(json);

                return histories ??
                       new List<GameHistory>();
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Khong the doc lich su: {ex.Message}");

                return new List<GameHistory>();
            }
        }

        // Xoa file lich su
        public void Clear()
        {
            try
            {
                if (File.Exists(filePath))
                    File.Delete(filePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Khong the xoa lich su: {ex.Message}");
            }
        }

        // Kiem tra file lich su co ton tai
        public bool Exists()
        {
            return File.Exists(filePath);
        }
    }
}