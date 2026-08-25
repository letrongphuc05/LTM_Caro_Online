using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using CaroOnline.Server.Network;
using CaroOnline.Server.Rooms;

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
                byte[] buffer = new byte[1024];
                NetworkStream stream = connection.Client.GetStream();

                while (true)
                {
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);

                    if (bytesRead == 0)
                    {
                        // Client đã ngắt kết nối
                        break;
                    }

                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    ProcessMessage(connection, message, stream);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling client {connection.Address}: {ex.Message}");
            }
            finally
            {
                // Xử lý khi Client bị ngắt kết nối
                if (!string.IsNullOrEmpty(connection.Username))
                {
                    PlayerManager.Instance.RemovePlayer(connection.Username);
                    BroadcastOnlineList();
                    Console.WriteLine($"Player {connection.Username} disconnected");
                }

                if (connection.RoomId >= 0)
                {
                    GameRoom? room = RoomManager.Instance.GetRoom(connection.RoomId);
                    if (room != null)
                    {
                        RoomManager.Instance.RemoveRoom(connection.RoomId);
                    }
                }

                Console.WriteLine($"Client disconnected: {connection.Address}");
                connection.Close();
            }
        }

        static void ProcessMessage(ClientConnection connection, string message, NetworkStream stream)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;

            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Received from {connection.Address}: {message}");

            string[] parts = message.Split('|');

            if (parts.Length == 0)
                return;

            string command = parts[0].Trim();

            switch (command)
            {
                case "LOGIN":
                    HandleLogin(connection, parts, stream);
                    break;

                case "CHALLENGE":
                    HandleChallenge(connection, parts, stream);
                    break;

                case "ACCEPT":
                    HandleAccept(connection, parts, stream);
                    break;

                case "DECLINE":
                    HandleDecline(connection, parts, stream);
                    break;

                case "MOVE":
                    HandleMove(connection, parts, stream);
                    break;

                default:
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Unknown command: {command}");
                    break;
            }
        }

        static void HandleLogin(ClientConnection connection, string[] parts, NetworkStream stream)
        {
            if (parts.Length < 2)
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] LOGIN failed: insufficient parameters");
                return;
            }

            string username = parts[1].Trim();
            connection.Username = username;

            PlayerManager.Instance.AddPlayer(connection);
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Player {username} logged in from {connection.Address}");

            // Gửi danh sách người chơi online (không bao gồm chính người chơi này)
            BroadcastOnlineList();
            BroadcastRoomList();
        }

        static void HandleChallenge(ClientConnection connection, string[] parts, NetworkStream stream)
        {
            if (parts.Length < 2 || string.IsNullOrEmpty(connection.Username))
                return;

            string targetPlayer = parts[1].Trim();
            ClientConnection? opponent = PlayerManager.Instance.GetPlayer(targetPlayer);

            if (opponent == null)
            {
                SendMessage(stream, "ERROR|Player not found");
                return;
            }

            // Gửi thông báo thách đấu tới opponent
            try
            {
                NetworkStream? opponentStream = opponent.Client.GetStream();
                if (opponentStream != null)
                {
                    SendMessage(opponentStream, $"CHALLENGE|{connection.Username}");
                }
            }
            catch
            {
                SendMessage(stream, "ERROR|Failed to send challenge");
            }
        }

        static void HandleAccept(ClientConnection connection, string[] parts, NetworkStream stream)
        {
            if (parts.Length < 2 || string.IsNullOrEmpty(connection.Username))
                return;

            string challenger = parts[1].Trim();
            ClientConnection? player1 = PlayerManager.Instance.GetPlayer(challenger);

            if (player1 == null)
            {
                SendMessage(stream, "ERROR|Challenger not found");
                return;
            }

            // Tạo room cho hai player
            GameRoom room = RoomManager.Instance.CreateRoom(player1, connection);

            Console.WriteLine($"Room created: {room.GetRoomName()}");

            // Thông báo cho cả hai player room ID
            try
            {
                NetworkStream? player1Stream = player1.Client.GetStream();
                if (player1Stream != null)
                {
                    SendMessage(player1Stream, $"ROOM|{room.RoomId}|{connection.Username}");
                }
                SendMessage(stream, $"ROOM|{room.RoomId}|{challenger}");
            }
            catch
            {
                RoomManager.Instance.RemoveRoom(room.RoomId);
            }

            // Cập nhật danh sách room cho tất cả client
            BroadcastRoomList();
        }

        static void HandleDecline(ClientConnection connection, string[] parts, NetworkStream stream)
        {
            if (parts.Length < 2)
                return;

            string challenger = parts[1].Trim();
            ClientConnection? player1 = PlayerManager.Instance.GetPlayer(challenger);

            if (player1 != null)
            {
                try
                {
                    NetworkStream? player1Stream = player1.Client.GetStream();
                    if (player1Stream != null)
                    {
                        SendMessage(player1Stream, $"DECLINED|{connection.Username}");
                    }
                }
                catch
                {
                }
            }
        }

        static void HandleMove(ClientConnection connection, string[] parts, NetworkStream stream)
        {
            if (parts.Length < 3)
                return;

            if (connection.RoomId < 0)
                return;

            GameRoom? room = RoomManager.Instance.GetRoom(connection.RoomId);
            if (room == null)
                return;

            ClientConnection? opponent = room.GetOpponent(connection);
            if (opponent == null)
                return;

            // Chuyển tiếp nước đi tới opponent
            try
            {
                NetworkStream? opponentStream = opponent.Client.GetStream();
                if (opponentStream != null)
                {
                    SendMessage(opponentStream, $"MOVE|{parts[1]}|{parts[2]}");
                }
            }
            catch
            {
            }
        }

        static void BroadcastOnlineList()
        {
            List<string> onlinePlayers = PlayerManager.Instance.GetOnlinePlayersList();
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Broadcasting online list. Total players: {onlinePlayers.Count}");

            foreach (string username in onlinePlayers)
            {
                ClientConnection? player = PlayerManager.Instance.GetPlayer(username);
                if (player != null)
                {
                    // Gửi danh sách các player khác (không bao gồm chính mình)
                    List<string> otherPlayers = onlinePlayers.Where(p => p != username).ToList();
                    string onlineMessage = "ONLINE|" + string.Join("|", otherPlayers);

                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Sending to {username}: {onlineMessage}");

                    try
                    {
                        NetworkStream? stream = player.Client.GetStream();
                        if (stream != null)
                        {
                            SendMessage(stream, onlineMessage);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Error sending to {username}: {ex.Message}");
                    }
                }
            }
        }

        static void BroadcastRoomList()
        {
            List<GameRoom> activeRooms = RoomManager.Instance.GetActiveRooms();
            string roomMessage = "ROOMS|" + string.Join("|", activeRooms.Select(r => r.GetRoomName()));

            List<string> onlinePlayers = PlayerManager.Instance.GetOnlinePlayersList();
            foreach (string username in onlinePlayers)
            {
                ClientConnection? player = PlayerManager.Instance.GetPlayer(username);
                if (player != null)
                {
                    try
                    {
                        NetworkStream? stream = player.Client.GetStream();
                        if (stream != null)
                        {
                            SendMessage(stream, roomMessage);
                        }
                    }
                    catch
                    {
                    }
                }
            }
        }

        static void SendMessage(NetworkStream stream, string message)
        {
            try
            {
                byte[] buffer = Encoding.UTF8.GetBytes(message);
                stream.Write(buffer, 0, buffer.Length);
            }
            catch
            {
            }
        }
    }
}