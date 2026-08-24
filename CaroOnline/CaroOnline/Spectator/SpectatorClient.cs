using System;
using System.Collections.Generic;

namespace CaroOnline.Spectator
{
    public class SpectatorClient
    {
        private bool isSpectating;
        private string currentRoomId = "";

        // Luu cac nuoc di nhan duoc
        private readonly List<string> receivedMoves =
            new List<string>();

        // So luong spectator hien tai
        private int spectatorCount;

        public bool IsSpectating
        {
            get { return isSpectating; }
        }

        public string CurrentRoomId
        {
            get { return currentRoomId; }
        }

        public int SpectatorCount
        {
            get { return spectatorCount; }
        }

        // Bat dau xem phong
        public void StartSpectating(string roomId)
        {
            if (string.IsNullOrWhiteSpace(roomId))
                return;

            currentRoomId = roomId;
            isSpectating = true;

            receivedMoves.Clear();
            spectatorCount = 0;
        }

        // Tao message gui len server de xem phong
        public string CreateSpectateMessage(string roomId)
        {
            if (string.IsNullOrWhiteSpace(roomId))
                return "";

            return $"SPECTATE|{roomId}";
        }

        // Tao message roi phong dang xem
        public string CreateLeaveSpectateMessage()
        {
            if (!isSpectating ||
                string.IsNullOrWhiteSpace(currentRoomId))
                return "";

            return $"LEAVE_SPECTATE|{currentRoomId}";
        }

        // Roi phong dang xem
        public void StopSpectating()
        {
            currentRoomId = "";
            isSpectating = false;

            receivedMoves.Clear();
            spectatorCount = 0;
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
                // Server chap nhan xem tran
                case "SPECTATE_OK":
                    if (parts.Length >= 2)
                    {
                        StartSpectating(parts[1]);
                    }
                    break;

                // Nhan trang thai ban co hien tai
                case "GAME_STATE":
                    HandleGameState(parts);
                    break;

                // Nhan nuoc di moi
                case "MOVE":
                    HandleMove(parts);
                    break;

                // Nhan so luong spectator
                case "SPECTATOR_COUNT":
                    HandleSpectatorCount(parts);
                    break;

                // Tran dau ket thuc
                case "GAME_END":
                    HandleGameEnd(parts);
                    break;

                // Roi phong thanh cong
                case "LEAVE_SPECTATE_OK":
                    StopSpectating();
                    break;
            }
        }

        // Xu ly trang thai ban co hien tai
        private void HandleGameState(string[] parts)
        {
            if (!isSpectating)
                return;

            // Du lieu ban co se duoc Client Game su dung sau
            Console.WriteLine(
                "Da nhan trang thai ban co hien tai."
            );
        }

        // Xu ly nuoc di moi
        private void HandleMove(string[] parts)
        {
            if (!isSpectating)
                return;

            string move = string.Join("|", parts);

            receivedMoves.Add(move);

            Console.WriteLine(
                $"Spectator nhan nuoc di: {move}"
            );
        }

        // Xu ly so luong spectator
        private void HandleSpectatorCount(string[] parts)
        {
            if (!isSpectating)
                return;

            if (parts.Length < 2)
                return;

            int count;

            if (int.TryParse(parts[1], out count))
            {
                spectatorCount = count;
            }
        }

        // Xu ly khi tran ket thuc
        private void HandleGameEnd(string[] parts)
        {
            Console.WriteLine(
                $"Tran dau ket thuc: {string.Join("|", parts)}"
            );

            StopSpectating();
        }

        // Lay danh sach cac nuoc di da nhan
        public List<string> GetReceivedMoves()
        {
            return new List<string>(receivedMoves);
        }

        // Lay so luong nuoc di
        public int GetMoveCount()
        {
            return receivedMoves.Count;
        }

        // Xoa du lieu spectator
        public void Reset()
        {
            StopSpectating();
        }
    }
}