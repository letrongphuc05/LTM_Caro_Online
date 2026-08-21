using System;

namespace CaroOnline.History
{
    public class HistoryNetworkClient
    {
        private readonly HistoryClient historyClient;
        private readonly HistoryRequest historyRequest;

        public HistoryNetworkClient()
        {
            historyClient = new HistoryClient();
            historyRequest = new HistoryRequest();
        }

        // Lay tat ca lich su
        public void RequestAll()
        {
            string message =
                historyRequest.GetAll();

            if (!string.IsNullOrWhiteSpace(message))
            {
                Network.NetworkManager.Instance.Send(message);
            }
        }

        // Lay lich su theo GameId
        public void RequestByGameId(string gameId)
        {
            string message =
                historyRequest.GetByGameId(gameId);

            if (!string.IsNullOrWhiteSpace(message))
            {
                Network.NetworkManager.Instance.Send(message);
            }
        }

        // Lay lich su theo nguoi choi
        public void RequestByPlayer(string playerName)
        {
            string message =
                historyRequest.GetByPlayer(playerName);

            if (!string.IsNullOrWhiteSpace(message))
            {
                Network.NetworkManager.Instance.Send(message);
            }
        }

        // Lay lich su theo RoomId
        public void RequestByRoomId(string roomId)
        {
            string message =
                historyRequest.GetByRoomId(roomId);

            if (!string.IsNullOrWhiteSpace(message))
            {
                Network.NetworkManager.Instance.Send(message);
            }
        }

        // Xu ly du lieu nhan tu Server
        public void HandleServerMessage(string message)
        {
            historyClient.HandleServerMessage(message);
        }

        // Lay lich su hien tai
        public System.Collections.Generic.List<GameHistoryInfo>
            GetAllHistory()
        {
            return historyClient.GetAllHistory();
        }

        // Tim theo GameId
        public GameHistoryInfo GetById(string gameId)
        {
            return historyClient.GetById(gameId);
        }

        // Tim theo nguoi choi
        public System.Collections.Generic.List<GameHistoryInfo>
            GetByPlayer(string playerName)
        {
            return historyClient.GetByPlayer(playerName);
        }
    }
}