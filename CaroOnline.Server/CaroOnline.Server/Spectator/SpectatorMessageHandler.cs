using System;
using CaroOnline.Server.Network;

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

            string roomId = parts[1];

            bool added =
                spectatorManager.AddSpectator(
                    roomId,
                    client
                );

            if (!added)
                return "SPECTATE_ERROR|ALREADY_SPECTATING";

            int count =
                spectatorManager.GetSpectatorCount(roomId);

            return $"SPECTATE_OK|{roomId}|{count}";
        }

        private string HandleLeaveSpectate(
            string[] parts,
            ClientConnection client)
        {
            if (parts.Length < 2)
                return "LEAVE_SPECTATE_ERROR|INVALID_ROOM";

            string roomId = parts[1];

            bool removed =
                spectatorManager.RemoveSpectator(
                    roomId,
                    client
                );

            if (!removed)
                return "LEAVE_SPECTATE_ERROR|NOT_SPECTATING";

            return $"LEAVE_SPECTATE_OK|{roomId}";
        }
    }
}