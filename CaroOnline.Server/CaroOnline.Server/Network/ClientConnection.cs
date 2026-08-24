using CaroOnline.Server.Rooms;
using System;
using System.Net.Sockets;
using System.Text;

namespace CaroOnline.Server.Network
{
    internal class ClientConnection
    {
        // Lưu kết nối TCP của một Client
        private readonly TcpClient _client;
        private string _username = "";
        private int _roomId = -1;

        public ClientConnection(TcpClient client)
        {
            _client = client;
        }

        // Cho phép các thành phần khác truy cập kết nối TCP
        public TcpClient Client => _client;

        // Lấy địa chỉ của Client đang kết nối
        public string Address =>
            _client.Client.RemoteEndPoint?.ToString() ?? "Unknown";

        // Lấy/Đặt username của player 
        public string Username
        {
            get => _username;
            set => _username = value;
        }

        // Lấy/Đặt room ID của player 
        public int RoomId
        {
            get => _roomId;
            set => _roomId = value;
        }

        public void ReceiveData() // XỬ LÝ NHẬN DỮ LIỆU, CHỐNG CRASH & GHI LOG
        {
            NetworkStream stream = _client.GetStream();
            byte[] buffer = new byte[1024];

            try
            {
                // Ghi log: Bắt đầu theo dõi một kết nối mới
                ServerController.WriteLog($"[New Connection] Client {Address} is ready to transmit data.");

                while (true)
                {
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);

                    if (bytesRead == 0)
                    {
                        // Ghi log: Khách chủ động bấm nút thoát game đàng hoàng
                        ServerController.WriteLog($"[Disconnected] Client {Address} has left the network safely.");
                        break;
                    }

                    // Dịch dữ liệu và Ghi Log để giám sát toàn bộ hoạt động
                    string data = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    ServerController.WriteLog($"[Server Received from {Address}]: {data}");

                    // Người phụ trách logic sẽ viết các lệnh if-else xử lý nước đi (MOVE), vào phòng (ROOM) ở đây
                }
            }
            catch (Exception ex)
            {
                // CHỐNG CRASH: Nếu Client đột ngột rút dây mạng, cúp điện, lỗi văng ra 
                // sẽ bị "nhốt" ở đây. Server CHỈ ghi log lỗi thay vì bị sập toàn hệ thống.
                ServerController.WriteLog($"[Network Error] Client {Address} disconnected unexpectedly: {ex.Message}");
            }
            finally
            {
                // DỌN DẸP BỘ NHỚ: Xử lý khi Client bị ngắt kết nối đột ngột hoặc an toàn
                if (!string.IsNullOrEmpty(_username))
                {
                    PlayerManager.Instance.RemovePlayer(_username);
                    ServerController.WriteLog($"[Disconnected] Player {_username} has left the network safely.");
                }

                if (_roomId >= 0)
                {
                    GameRoom room = RoomManager.Instance.GetRoom(_roomId);
                    if (room != null)
                    {
                        RoomManager.Instance.RemoveRoom(_roomId);
                    }
                }

                ServerController.WriteLog($"[Cleanup] Closing network stream for Client {Address}.");
                Close();
            }
        }

        // Đóng kết nối với Client
        public void Close()
        {
            try
            {
                ServerController.WriteLog($"[System] Completely closed TcpClient connection of {Address}.");
                _client.Close();
            }
            catch
            {
            }
        }
    }
}