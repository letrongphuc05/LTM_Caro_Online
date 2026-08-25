using System;

namespace CaroOnline.Server.History
{
    internal class HistoryController
    {
        private readonly HistoryService historyService;
        private readonly HistoryQuery historyQuery;
        private readonly HistoryMessageHandler messageHandler;

        public HistoryController()
        {
            historyService = new HistoryService();

            HistoryStorage storage = new HistoryStorage();

            historyQuery = new HistoryQuery(storage);

            messageHandler =
                new HistoryMessageHandler(historyQuery);
        }

        // Xu ly yeu cau lich su tu Client
        public string HandleRequest(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return "";

            return messageHandler.HandleMessage(message);
        }

        // Tao mot tran moi
        public GameHistory StartGame(
            string gameId,
            string roomId,
            string playerX,
            string playerO)
        {
            GameHistory history =
                new GameHistory();

            history.GameId = gameId;
            history.RoomId = roomId;
            history.PlayerX = playerX;
            history.PlayerO = playerO;
            history.Status = "PLAYING";

            historyService.AddGame(history);

            return history;
        }

        // Them nuoc di vao tran
        public bool AddMove(
            string gameId,
            string player,
            int row,
            int column)
        {
            GameHistory history =
                historyService.GetByGameId(gameId);

            if (history == null)
                return false;

            history.AddMove(
                player,
                row,
                column);

            historyService.UpdateGame(history);

            return true;
        }

        // Ket thuc tran
        public bool EndGame(
            string gameId,
            string winner)
        {
            GameHistory history =
                historyService.GetByGameId(gameId);

            if (history == null)
                return false;

            history.FinishGame(winner);

            historyService.UpdateGame(history);

            return true;
        }

        // Lay lich su theo GameId
        public GameHistory GetGame(
            string gameId)
        {
            return historyService.GetByGameId(gameId);
        }

        // Lay tat ca lich su
        public System.Collections.Generic.List<GameHistory>
            GetAll()
        {
            return historyService.GetAll();
        }
    }
}