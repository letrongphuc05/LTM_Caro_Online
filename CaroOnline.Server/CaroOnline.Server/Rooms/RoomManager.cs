using System;
using System.Collections.Generic;
using System.Linq;
using CaroOnline.Server.Network;

namespace CaroOnline.Server.Rooms
{
    internal class RoomManager
    {
        // Instance duy nhất của RoomManager
        private static RoomManager? _instance;

        // Danh sách các phòng đang hoạt động
        // Key = RoomId
        // Value = GameRoom
        private Dictionary<int, GameRoom> _rooms =
            new Dictionary<int, GameRoom>();

        // ID của phòng tiếp theo
        private int _nextRoomId = 1;

        // Đảm bảo nhiều Client không cùng lúc thay đổi danh sách phòng
        private readonly object _lockObject = new object();


        // ==============================
        // SINGLETON INSTANCE
        // ==============================

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


        // Constructor
        public RoomManager()
        {
        }


        // ==============================
        // TẠO PHÒNG
        // ==============================

        public GameRoom? CreateRoom(
            ClientConnection player1,
            ClientConnection player2)
        {
            lock (_lockObject)
            {
                // Tạo ID phòng mới
                int roomId = _nextRoomId++;

                // Tạo GameRoom chứa 2 người chơi
                GameRoom room =
                    new GameRoom(roomId, player1, player2);

                // Thêm phòng vào danh sách phòng đang hoạt động
                _rooms[roomId] = room;

                // Gán RoomId cho Player 1
                player1.RoomId = roomId;

                // Gán RoomId cho Player 2
                player2.RoomId = roomId;

                // Trả về phòng vừa tạo
                return room;
            }
        }


        // ==============================
        // LẤY PHÒNG THEO ID
        // ==============================

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


        // ==============================
        // XÓA PHÒNG
        // ==============================

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


        // ==============================
        // LẤY DANH SÁCH PHÒNG ĐANG HOẠT ĐỘNG
        // ==============================

        public List<GameRoom> GetActiveRooms()
        {
            lock (_lockObject)
            {
                return _rooms.Values.ToList();
            }
        }


        // ==============================
        // ĐẾM SỐ PHÒNG ĐANG HOẠT ĐỘNG
        // ==============================

        public int GetActiveRoomCount()
        {
            lock (_lockObject)
            {
                return _rooms.Count;
            }
        }
    }
}