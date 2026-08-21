using System;

namespace CaroOnline.Server.History
{
    internal class HistoryGameTracker
    {
        private readonly HistoryController controller;

        public HistoryGameTracker(
            HistoryController controller)
        {
            this.controller = controller;
        }

        // Bat dau mot tran
        public GameHistory StartGame(
            string gameId,
            string roomId,
            string playerX,
            string playerO)
        {
            if (string.IsNullOrWhiteSpace(gameId))
                return null;

            return controller.StartGame(
                gameId,
                roomId,
                playerX,
                playerO
            );
        }

        // Luu mot nuoc di
        public bool TrackMove(
            string gameId,
            string player,
            int row,
            int column)
        {
            if (string.IsNullOrWhiteSpace(gameId))
                return false;

            if (string.IsNullOrWhiteSpace(player))
                return false;

            return controller.AddMove(
                gameId,
                player,
                row,
                column
            );
        }

        // Ket thuc tran
        public bool EndGame(
            string gameId,
            string winner)
        {
            if (string.IsNullOrWhiteSpace(gameId))
                return false;

            return controller.EndGame(
                gameId,
                winner
            );
        }

        // Huy theo doi tran
        public bool CancelGame(string gameId)
        {
            if (string.IsNullOrWhiteSpace(gameId))
                return false;

            GameHistory history =
                controller.GetGame(gameId);

            return history != null;
        }
    }
}