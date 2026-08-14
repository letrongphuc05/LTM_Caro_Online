using System.Net.Sockets;

namespace CaroOnline.Server.Network
{
    internal class ClientConnection
    {
        // Lưu kết nối TCP của một Client
        private readonly TcpClient _client;

        public ClientConnection(TcpClient client)
        {
            _client = client;
        }

        // Cho phép các thành phần khác truy cập kết nối TCP
        public TcpClient Client => _client;

        // Lấy địa chỉ của Client đang kết nối
        public string Address =>
            _client.Client.RemoteEndPoint?.ToString() ?? "Unknown";

        // Đóng kết nối với Client
        public void Close()
        {
            _client.Close();
        }
    }
}