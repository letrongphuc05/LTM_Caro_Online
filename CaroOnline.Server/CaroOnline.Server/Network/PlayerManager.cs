using System;
using System.Collections.Generic;
using System.Linq;

namespace CaroOnline.Server.Network
{
    internal class PlayerManager
    {
        private static PlayerManager? _instance;
        private Dictionary<string, ClientConnection> _players = new Dictionary<string, ClientConnection>();
        private readonly object _lockObject = new object();

        public static PlayerManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new PlayerManager();
                }
                return _instance;
            }
        }

        public PlayerManager()
        {
        }

        /// <summary>
        /// Thêm một player vào danh sách online
        /// </summary>
        public void AddPlayer(ClientConnection player)
        {
            if (string.IsNullOrEmpty(player.Username))
                return;

            lock (_lockObject)
            {
                // [NOTE QUAN TRỌNG - FIX LỖI KẾT NỐI ZOMBIE]: 
                // Bỏ hàm kiểm tra !_players.ContainsKey(player.Username).
                // Luôn luôn ghi đè kết nối vào Dictionary. 
                // Mục đích: Nếu người chơi rớt mạng và kết nối lại ngay lập tức (cùng tên), 
                // Server sẽ cập nhật đúng luồng kết nối mới nhất thay vì lưu luồng mạng đã chết cũ.
                _players[player.Username] = player;
            }
        }

        /// <summary>
        /// Xóa một player khỏi danh sách online
        /// </summary>
        public void RemovePlayer(string username)
        {
            lock (_lockObject)
            {
                if (_players.ContainsKey(username))
                {
                    _players.Remove(username);
                }
            }
        }

        /// <summary>
        /// Lấy player theo username
        /// </summary>
        public ClientConnection? GetPlayer(string username)
        {
            lock (_lockObject)
            {
                if (_players.ContainsKey(username))
                {
                    return _players[username];
                }
                return null;
            }
        }

        /// <summary>
        /// Lấy danh sách tất cả player online (không bao gồm player đó)
        /// </summary>
        public List<string> GetOnlinePlayersList(string excludeUsername = "")
        {
            lock (_lockObject)
            {
                if (string.IsNullOrEmpty(excludeUsername))
                {
                    return _players.Keys.ToList();
                }
                else
                {
                    return _players.Keys.Where(p => p != excludeUsername).ToList();
                }
            }
        }

        /// <summary>
        /// Lấy số lượng player online
        /// </summary>
        public int GetOnlinePlayerCount()
        {
            lock (_lockObject)
            {
                return _players.Count;
            }
        }

        /// <summary>
        /// Kiểm tra xem player có online không
        /// </summary>
        public bool IsPlayerOnline(string username)
        {
            lock (_lockObject)
            {
                return _players.ContainsKey(username);
            }
        }
    }
}