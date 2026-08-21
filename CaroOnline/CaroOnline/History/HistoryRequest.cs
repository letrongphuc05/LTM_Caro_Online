namespace CaroOnline.History
{
    public class HistoryRequest
    {
        // Yeu cau lay tat ca lich su
        public string GetAll()
        {
            return "HISTORY_ALL";
        }

        // Yeu cau lich su theo GameId
        public string GetByGameId(string gameId)
        {
            if (string.IsNullOrWhiteSpace(gameId))
                return "";

            return $"HISTORY_GAME|{gameId}";
        }

        // Yeu cau lich su theo nguoi choi
        public string GetByPlayer(string playerName)
        {
            if (string.IsNullOrWhiteSpace(playerName))
                return "";

            return $"HISTORY_PLAYER|{playerName}";
        }

        // Yeu cau lich su theo phong
        public string GetByRoomId(string roomId)
        {
            if (string.IsNullOrWhiteSpace(roomId))
                return "";

            return $"HISTORY_ROOM|{roomId}";
        }
    }
}