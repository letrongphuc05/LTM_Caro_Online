using System;

namespace CaroOnline.History
{
    public class HistoryNetworkBridge
    {
        private readonly HistoryNetworkClient historyNetworkClient;

        public HistoryNetworkBridge()
        {
            historyNetworkClient =
                new HistoryNetworkClient();
            Network.NetworkManager.Instance.OnReceiveHistory =
                Receive;
        }

        // Gui yeu cau lay tat ca lich su
        public void GetAllHistory()
        {
            historyNetworkClient.RequestAll();
        }

        // Gui yeu cau theo GameId
        public void GetHistoryByGameId(string gameId)
        {
            historyNetworkClient.RequestByGameId(gameId);
        }

        // Gui yeu cau theo nguoi choi
        public void GetHistoryByPlayer(string playerName)
        {
            historyNetworkClient.RequestByPlayer(playerName);
        }

        // Gui yeu cau theo phong
        public void GetHistoryByRoom(string roomId)
        {
            historyNetworkClient.RequestByRoomId(roomId);
        }

        // Chuyen message tu SocketManager vao HistoryClient
        public void Receive(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;

            historyNetworkClient.HandleServerMessage(message);
        }

        // Lay danh sach lich su
        public System.Collections.Generic.List<GameHistoryInfo>
            GetHistories()
        {
            return historyNetworkClient.GetAllHistory();
        }
    }
}