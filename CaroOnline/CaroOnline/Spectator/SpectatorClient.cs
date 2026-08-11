using System;

namespace CaroOnline.Spectator
{
    public class SpectatorClient
    {
        private bool isSpectating;
        private string currentRoomId = "";

        public bool IsSpectating
        {
            get { return isSpectating; }
        }

        public string CurrentRoomId
        {
            get { return currentRoomId; }
        }

        // Bat dau xem phong
        public void StartSpectating(string roomId)
        {
            if (string.IsNullOrWhiteSpace(roomId))
                return;

            currentRoomId = roomId;
            isSpectating = true;
        }

        // Roi phong dang xem
        public void StopSpectating()
        {
            currentRoomId = "";
            isSpectating = false;
        }

        // Kiem tra co dang xem dung phong hay khong
        public bool IsWatchingRoom(string roomId)
        {
            if (!isSpectating)
                return false;

            return string.Equals(
                currentRoomId,
                roomId,
                StringComparison.OrdinalIgnoreCase
            );
        }

        // Xu ly thong bao tu server
        public void HandleServerMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;

            string[] parts = message.Split('|');

            if (parts.Length == 0)
                return;

            string command = parts[0];

            switch (command)
            {
                case "SPECTATE_OK":
                    if (parts.Length >= 2)
                    {
                        StartSpectating(parts[1]);
                    }
                    break;

                case "LEAVE_SPECTATE_OK":
                    StopSpectating();
                    break;

                case "GAME_END":
                    StopSpectating();
                    break;
            }
        }
    }
}