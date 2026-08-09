using System.Net;
using System.Net.Sockets;
using CaroOnline.Server.Network;

namespace CaroOnline.Server
{
    internal class Program
    {
        static void Main(string[] args)
        {
            const int port = 5000;

            // Tạo TCP Server và lắng nghe trên tất cả địa chỉ mạng của máy
            TcpListener server = new TcpListener(IPAddress.Any, port);

            // Bắt đầu mở port để chờ Client kết nối
            server.Start();

            Console.WriteLine($"Caro Server started on port {port}");
            Console.WriteLine("Waiting for clients...");

            while (true)
            {
                // Chờ một Client kết nối vào Server
                TcpClient client = server.AcceptTcpClient();

                // Tạo đối tượng quản lý kết nối của Client
                ClientConnection connection = new ClientConnection(client);

                Console.WriteLine($"Client connected: {connection.Address}");

                // Tạo một Thread riêng để xử lý Client này
                // Nhờ đó Server vẫn có thể tiếp tục nhận các Client khác
                Thread clientThread = new Thread(() =>
                {
                    HandleClient(connection);
                });

                clientThread.Start();
            }
        }

        static void HandleClient(ClientConnection connection)
        {
            try
            {
                // Giữ kết nối của Client hoạt động
                // Sau này sẽ thay phần này bằng code nhận và xử lý dữ liệu
                while (true)
                {
                    Thread.Sleep(1000);
                }
            }
            catch
            {
                // Xử lý khi Client bị ngắt kết nối
                Console.WriteLine($"Client disconnected: {connection.Address}");
                connection.Close();
            }
        }
    }
}