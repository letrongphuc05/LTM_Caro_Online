using System;
using System.Collections.Generic;
using System.Linq;
using CaroOnline.Server.Network;

namespace CaroOnline.Server.Rooms
{
    internal class RoomManager
    {
        private static RoomManager? _instance;
        private Dictionary<int, GameRoom> _rooms = new Dictionary<int, GameRoom>();
        private int _nextRoomId = 1;
        private readonly object _lockObject = new object();

        public static RoomManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new RoomManager();
                }
                return _instance;
            }
        }

        public RoomManager()
        {
        }

        /// <summary>
        /// Tạo một room mới cho hai player
        /// </summary>
        public GameRoom? CreateRoom(ClientConnection player1, ClientConnection player2)
        {
            lock (_lockObject)
            {
                int roomId = _nextRoomId++;
                GameRoom room = new GameRoom(roomId, player1, player2);
                _rooms[roomId] = room;
                player1.RoomId = roomId;
                player2.RoomId = roomId;
                return room;
            }
        }

        /// <summary>
        /// Lấy room theo ID
        /// </summary>
        public GameRoom? GetRoom(int roomId)
        {
            lock (_lockObject)
            {
                if (_rooms.ContainsKey(roomId))
                {
                    return _rooms[roomId];
                }
                return null;
            }
        }

        /// <summary>
        /// Xóa room
        /// </summary>
        public void RemoveRoom(int roomId)
        {
            lock (_lockObject)
            {
                if (_rooms.ContainsKey(roomId))
                {
                    _rooms.Remove(roomId);
                }
            }
        }

        /// <summary>
        /// Lấy danh sách các room đang hoạt động
        /// </summary>
        public List<GameRoom> GetActiveRooms()
        {
            lock (_lockObject)
            {
                return _rooms.Values.ToList();
            }
        }

        /// <summary>
        /// Lấy số lượng room đang hoạt động
        /// </summary>
        public int GetActiveRoomCount()
        {
            lock (_lockObject)
            {
                return _rooms.Count;
            }
        }
    }
}
