using System;
using CaroOnline.Server.Network;

namespace CaroOnline.Server.Rooms
{
    internal class GameRoom
    {
        public int RoomId { get; private set; }
        public ClientConnection Player1 { get; private set; }
        public ClientConnection Player2 { get; private set; }
        public DateTime CreatedTime { get; private set; }

        public GameRoom(int roomId, ClientConnection player1, ClientConnection player2)
        {
            RoomId = roomId;
            Player1 = player1;
            Player2 = player2;
            CreatedTime = DateTime.Now;
        }

        /// <summary>
        /// Lấy tên của room để display
        /// </summary>
        public string GetRoomName()
        {
            return $"{Player1.Username} vs {Player2.Username}";
        }

        /// <summary>
        /// Kiểm tra xem player có trong room không
        /// </summary>
        public bool ContainsPlayer(ClientConnection player)
        {
            return Player1 == player || Player2 == player;
        }

        /// <summary>
        /// Lấy opponent của một player
        /// </summary>
        public ClientConnection? GetOpponent(ClientConnection player)
        {
            // [NOTE QUAN TRỌNG - FIX ĐỒNG BỘ MẠNG]:
            // So sánh Username thay vì so sánh đối tượng (Player1 == player).
            // Tránh lỗi không tìm thấy đối thủ do rớt mạng kết nối lại bị đổi vùng nhớ.
            if (Player1.Username == player.Username) return Player2;
            if (Player2.Username == player.Username) return Player1;
            return null;
        }
    }
}