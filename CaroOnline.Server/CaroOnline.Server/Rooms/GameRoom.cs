using CaroOnline.Server.Network;

namespace CaroOnline.Server.Rooms
{
    public class GameRoom
    {
        public string RoomId { get; }

        public ClientConnection? Player1 { get; private set; }

        public ClientConnection? Player2 { get; private set; }

        public bool IsFull => Player1 != null && Player2 != null;

        public bool IsEmpty => Player1 == null && Player2 == null;

        public GameRoom(string roomId)
        {
            RoomId = roomId;
        }

        public bool AddPlayer(ClientConnection player)
        {
            if (IsFull)
                return false;

            if (Player1 == player || Player2 == player)
                return false;

            if (Player1 == null)
            {
                Player1 = player;
                return true;
            }

            Player2 = player;
            return true;
        }

        public bool RemovePlayer(ClientConnection player)
        {
            if (Player1 == player)
            {
                Player1 = null;
                return true;
            }

            if (Player2 == player)
            {
                Player2 = null;
                return true;
            }

            return false;
        }

        public bool HasPlayer(ClientConnection player)
        {
            return Player1 == player || Player2 == player;
        }

        public ClientConnection? GetOpponent(ClientConnection player)
        {
            if (Player1 == player)
                return Player2;

            if (Player2 == player)
                return Player1;

            return null;
        }
    }
}