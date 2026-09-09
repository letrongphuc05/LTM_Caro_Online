using System;
using System.Linq;
using CaroOnline.Server.Network;
using CaroOnline.Server.Rooms;

namespace CaroOnline.Server.Spectator
{
    internal class SpectatorMessageHandler
    {
        private readonly SpectatorManager spectatorManager;

        public SpectatorMessageHandler(
            SpectatorManager spectatorManager)
        {
            this.spectatorManager = spectatorManager;
        }

        public string HandleMessage(
            string message,
            ClientConnection client)
        {
            if (string.IsNullOrWhiteSpace(message))
                return "";

            string[] parts = message.Split('|');

            if (parts.Length == 0)
                return "";

            string command = parts[0];

            switch (command)
            {
                case "SPECTATE":
                    return HandleSpectate(parts, client);

                case "LEAVE_SPECTATE":
                    return HandleLeaveSpectate(parts, client);

                default:
                    return "UNKNOWN_COMMAND";
            }
        }

        private string HandleSpectate(
            string[] parts,
            ClientConnection client)
        {
            if (parts.Length < 2)
                return "SPECTATE_ERROR|INVALID_ROOM";

            string roomInfo = parts[1].Trim();

            GameRoom? room = null;

            // Nếu client gửi RoomId
            if (int.TryParse(roomInfo, out int roomId))
            {
                room = RoomManager.Instance.GetRoom(roomId);
            }
            else
            {
                // Nếu client gửi tên trận, ví dụ: "A vs B"
                room = RoomManager.Instance
                    .GetActiveRooms()
                    .FirstOrDefault(
                        r => r.GetRoomName() == roomInfo
                    );
            }

            if (room == null)
                return "SPECTATE_ERROR|ROOM_NOT_FOUND";

            string actualRoomId = room.RoomId.ToString();

            bool added = spectatorManager.AddSpectator(actualRoomId, client);

            if (!added)
                return "SPECTATE_ERROR|ALREADY_SPECTATING";

            int count = spectatorManager.GetSpectatorCount(actualRoomId);

            Console.WriteLine(
                $"[SPECTATOR] {client.Address} joined room {actualRoomId}. Total spectators: {count}"
            );

            return $"SPECTATE_OK|{actualRoomId}|{count}";

   
        }

        private string HandleLeaveSpectate(
            string[] parts,
            ClientConnection client)
        {
            if (parts.Length < 2)
                return "LEAVE_SPECTATE_ERROR|INVALID_ROOM";

            string roomInfo = parts[1].Trim();

            GameRoom? room = null;

            // Nếu client gửi RoomId
            if (int.TryParse(roomInfo, out int roomId))
            {
                room = RoomManager.Instance.GetRoom(roomId);
            }
            else
            {
                // Nếu client gửi tên trận
                room = RoomManager.Instance
                    .GetActiveRooms()
                    .FirstOrDefault(
                        r => r.GetRoomName() == roomInfo
                    );
            }

            if (room == null)
                return "LEAVE_SPECTATE_ERROR|ROOM_NOT_FOUND";

            string actualRoomId = room.RoomId.ToString();

            bool removed = spectatorManager.RemoveSpectator(
                actualRoomId,
                client
            );

            if (!removed)
                return "LEAVE_SPECTATE_ERROR|NOT_SPECTATING";

            return $"LEAVE_SPECTATE_OK|{actualRoomId}";
        }
    }
}