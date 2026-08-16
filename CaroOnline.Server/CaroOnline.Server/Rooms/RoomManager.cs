using System.Collections.Generic;
using System.Linq;
using CaroOnline.Server.Network;

namespace CaroOnline.Server.Rooms
{
    internal class RoomManager
    {
        private readonly Dictionary<string, GameRoom> _rooms;
        private int _nextRoomId;

        public RoomManager()
        {
            _rooms = new Dictionary<string, GameRoom>();
            _nextRoomId = 1;
        }

        public GameRoom CreateRoom()
        {
            string roomId = $"Room{_nextRoomId++}";
            GameRoom room = new GameRoom(roomId);

            _rooms.Add(room.RoomId, room);

            return room;
        }

        public GameRoom? GetRoom(string roomId)
        {
            _rooms.TryGetValue(roomId, out GameRoom? room);
            return room;
        }

        public GameRoom JoinOrCreateRoom(ClientConnection player)
        {
            foreach (GameRoom room in _rooms.Values)
            {
                if (!room.IsFull && room.AddPlayer(player))
                {
                    return room;
                }
            }

            GameRoom newRoom = CreateRoom();
            newRoom.AddPlayer(player);

            return newRoom;
        }

        public bool RemovePlayer(ClientConnection player)
        {
            foreach (GameRoom room in _rooms.Values.ToList())
            {
                if (room.RemovePlayer(player))
                {
                    if (room.IsEmpty)
                    {
                        _rooms.Remove(room.RoomId);
                    }

                    return true;
                }
            }

            return false;
        }

        public List<GameRoom> GetAllRooms()
        {
            return _rooms.Values.ToList();
        }

        public int RoomCount => _rooms.Count;
    }
}