using System.Collections.Generic;

namespace CaroOnline.History
{
    public class HistoryManagerClient
    {
        private readonly HistoryNetworkBridge networkBridge;

        public HistoryManagerClient()
        {
            networkBridge = new HistoryNetworkBridge();
        }

        // Yeu cau tat ca lich su
        public void LoadAllHistory()
        {
            networkBridge.GetAllHistory();
        }

        // Yeu cau lich su theo GameId
        public void LoadByGameId(string gameId)
        {
            networkBridge.GetHistoryByGameId(gameId);
        }

        // Yeu cau lich su theo nguoi choi
        public void LoadByPlayer(string playerName)
        {
            networkBridge.GetHistoryByPlayer(playerName);
        }

        // Yeu cau lich su theo phong
        public void LoadByRoom(string roomId)
        {
            networkBridge.GetHistoryByRoom(roomId);
        }

        // Lay lich su da nhan
        public List<GameHistoryInfo> GetHistory()
        {
            return networkBridge.GetHistories();
        }

        // Nhan response tu Server
        public void HandleServerMessage(string message)
        {
            networkBridge.Receive(message);
        }
    }
}