using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CaroOnline.Server.Rooms;

namespace CaroOnline.Server.Network
{
    public class ServerController
    {
        private TcpListener? _listener;
        private CancellationTokenSource? _cancellationTokenSource;

        public bool IsRunning { get; private set; }
        public int ClientCount { get; private set; }
        public int TotalConnections { get; private set; }

        public event Action<string>? ClientConnected;
        public event Action<string>? ClientDisconnected;
        public event Action<string>? LogMessage;

        public ServerController()
        {
            IsRunning = false;
            ClientCount = 0;
            TotalConnections = 0;
        }

        public void Start(string ip, int port)
        {
            if (IsRunning) return;

            IPAddress ipAddress = IPAddress.Parse(ip);
            _listener = new TcpListener(ipAddress, port);
            _listener.Start();

            IsRunning = true;
            _cancellationTokenSource = new CancellationTokenSource();

            Log($"Server started at {ip}:{port}");
            _ = AcceptClientsAsync(_cancellationTokenSource.Token);
        }

        private void Log(string message)
        {
            LogMessage?.Invoke($"[{DateTime.Now:HH:mm:ss}] {message}");
        }

        private async Task AcceptClientsAsync(CancellationToken token)
        {
            while (IsRunning && !token.IsCancellationRequested)
            {
                try
                {
                    if (_listener == null) break;

                    TcpClient client = await _listener.AcceptTcpClientAsync(token);

                    ClientCount++;
                    TotalConnections++;

                    string clientAddress = client.Client.RemoteEndPoint?.ToString() ?? "Unknown";
                    ClientConnected?.Invoke(clientAddress);

                    _ = MonitorClientAsync(client, token);
                }
                catch (OperationCanceledException) { break; }
                catch (ObjectDisposedException) { break; }
            }
        }

        private async Task MonitorClientAsync(TcpClient client, CancellationToken token)
        {
            string clientAddress = client.Client.RemoteEndPoint?.ToString() ?? "Unknown";
            ClientConnection connection = new ClientConnection(client);

            // [NOTE QUAN TRỌNG - BỘ ĐỆM TCP]: 
            // Tạo một biến messageBuffer riêng cho từng client để lưu trữ tạm thời các mảnh TCP bị đứt gãy.
            string messageBuffer = "";

            try
            {
                NetworkStream stream = client.GetStream();
                byte[] buffer = new byte[1024];

                while (IsRunning && !token.IsCancellationRequested)
                {
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, token);
                    if (bytesRead == 0) break;

                    // [NOTE QUAN TRỌNG - CHỐNG DÍNH GÓI TIN]:
                    // TCP có thể gộp 2 lệnh (VD: "MOVE|1|1MOVE|2|2"). 
                    // Ta nối dữ liệu thô vào Buffer, sau đó cắt bằng ký tự xuống dòng '\n' để lấy từng lệnh một.
                    string rawData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    messageBuffer += rawData;

                    int newlineIndex;
                    while ((newlineIndex = messageBuffer.IndexOf('\n')) >= 0)
                    {
                        string cleanMsg = messageBuffer.Substring(0, newlineIndex).Trim();
                        messageBuffer = messageBuffer.Substring(newlineIndex + 1);

                        if (!string.IsNullOrEmpty(cleanMsg))
                        {
                            ProcessMessage(connection, cleanMsg, stream);
                        }
                    }
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                Log($"Lỗi client {clientAddress}: {ex.Message}");
            }
            finally
            {
                if (!string.IsNullOrEmpty(connection.Username))
                {
                    PlayerManager.Instance.RemovePlayer(connection.Username);
                    BroadcastOnlineList();
                    Log($"Player {connection.Username} disconnected");
                }

                if (connection.RoomId >= 0)
                {
                    GameRoom? room = RoomManager.Instance.GetRoom(connection.RoomId);
                    if (room != null)
                    {
                        RoomManager.Instance.RemoveRoom(connection.RoomId);
                        BroadcastRoomList();
                    }
                }

                client.Close();

                if (IsRunning && ClientCount > 0)
                {
                    ClientCount--;
                    ClientDisconnected?.Invoke(clientAddress);
                }
            }
        }

        public void Stop()
        {
            if (!IsRunning) return;
            _cancellationTokenSource?.Cancel();
            if (_listener != null)
            {
                _listener.Stop();
                _listener = null;
            }
            IsRunning = false;
            ClientCount = 0;
            Log("Server stopped.");
        }

        private void ProcessMessage(ClientConnection connection, string message, NetworkStream stream)
        {
            Log($"Received from {connection.Address}: {message}");
            string[] parts = message.Split('|');
            if (parts.Length == 0) return;

            string command = parts[0].Trim();
            switch (command)
            {
                case "LOGIN": HandleLogin(connection, parts, stream); break;
                case "CHALLENGE": HandleChallenge(connection, parts, stream); break;
                case "ACCEPT": HandleAccept(connection, parts, stream); break;
                case "DECLINE": HandleDecline(connection, parts, stream); break;
                case "MOVE": HandleMove(connection, parts, stream); break;
                default: Log($"Unknown command: {command}"); break;
            }
        }

        private void HandleLogin(ClientConnection connection, string[] parts, NetworkStream stream)
        {
            if (parts.Length < 2) return;
            string username = parts[1].Trim();
            connection.Username = username;

            PlayerManager.Instance.AddPlayer(connection);
            Log($"Player {username} logged in from {connection.Address}");

            BroadcastOnlineList();
            BroadcastRoomList();
        }

        private void HandleChallenge(ClientConnection connection, string[] parts, NetworkStream stream)
        {
            if (parts.Length < 2 || string.IsNullOrEmpty(connection.Username)) return;
            string targetPlayer = parts[1].Trim();
            ClientConnection? opponent = PlayerManager.Instance.GetPlayer(targetPlayer);

            if (opponent == null)
            {
                SendMessage(stream, "ERROR|Player not found");
                return;
            }
            try
            {
                NetworkStream? opponentStream = opponent.Client.GetStream();
                if (opponentStream != null) SendMessage(opponentStream, $"CHALLENGE|{connection.Username}");
            }
            catch { SendMessage(stream, "ERROR|Failed to send challenge"); }
        }

        private void HandleAccept(ClientConnection connection, string[] parts, NetworkStream stream)
        {
            if (parts.Length < 2 || string.IsNullOrEmpty(connection.Username)) return;
            string challenger = parts[1].Trim();
            ClientConnection? player1 = PlayerManager.Instance.GetPlayer(challenger);

            if (player1 == null)
            {
                SendMessage(stream, "ERROR|Challenger not found");
                return;
            }

            GameRoom? room = RoomManager.Instance.CreateRoom(player1, connection);
            if (room != null)
            {
                Log($"Room created: {room.GetRoomName()}");
                try
                {
                    NetworkStream? player1Stream = player1.Client.GetStream();
                    if (player1Stream != null)
                    {
                        // [NOTE QUAN TRỌNG - PHÂN QUYỀN ĐI TRƯỚC]:
                        // Gắn thêm "|1" để báo hiệu player1 (Người mời) là phe X, được đi trước.
                        SendMessage(player1Stream, $"ROOM|{room.RoomId}|{connection.Username}|1");
                    }
                    // Gắn thêm "|2" để báo hiệu connection (Người đồng ý) là phe O, đi sau.
                    SendMessage(stream, $"ROOM|{room.RoomId}|{challenger}|2");
                }
                catch { RoomManager.Instance.RemoveRoom(room.RoomId); }

                BroadcastRoomList();
            }
        }

        private void HandleDecline(ClientConnection connection, string[] parts, NetworkStream stream)
        {
            if (parts.Length < 2) return;
            string challenger = parts[1].Trim();
            ClientConnection? player1 = PlayerManager.Instance.GetPlayer(challenger);

            if (player1 != null)
            {
                try
                {
                    NetworkStream? player1Stream = player1.Client.GetStream();
                    if (player1Stream != null) SendMessage(player1Stream, $"DECLINED|{connection.Username}");
                }
                catch { }
            }
        }

        private void HandleMove(ClientConnection connection, string[] parts, NetworkStream stream)
        {
            if (parts.Length < 3 || connection.RoomId < 0) return;
            GameRoom? room = RoomManager.Instance.GetRoom(connection.RoomId);
            if (room == null) return;

            ClientConnection? opponent = room.GetOpponent(connection);
            if (opponent == null) return;

            try
            {
                NetworkStream? opponentStream = opponent.Client.GetStream();
                if (opponentStream != null)
                    SendMessage(opponentStream, $"MOVE|{parts[1].Trim()}|{parts[2].Trim()}");
            }
            catch { }
        }

        private void BroadcastOnlineList()
        {
            List<string> onlinePlayers = PlayerManager.Instance.GetOnlinePlayersList();
            foreach (string username in onlinePlayers)
            {
                ClientConnection? player = PlayerManager.Instance.GetPlayer(username);
                if (player != null)
                {
                    List<string> otherPlayers = onlinePlayers.Where(p => p != username).ToList();
                    string onlineMessage = "ONLINE|" + string.Join("|", otherPlayers);
                    try
                    {
                        NetworkStream? stream = player.Client.GetStream();
                        if (stream != null) SendMessage(stream, onlineMessage);
                    }
                    catch { }
                }
            }
        }

        private void BroadcastRoomList()
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
                        if (stream != null) SendMessage(stream, roomMessage);
                    }
                    catch { }
                }
            }
        }

        private void SendMessage(NetworkStream stream, string message)
        {
            try
            {
                // [NOTE QUAN TRỌNG - CHUẨN HÓA MẠNG TCP]:
                // Bắt buộc đính kèm "\n" vào cuối chuỗi để làm ký tự kết thúc gói tin.
                byte[] buffer = Encoding.UTF8.GetBytes(message + "\n");
                stream.Write(buffer, 0, buffer.Length);

                // [NOTE QUAN TRỌNG - FIX ĐỒNG BỘ]: 
                // Bắt buộc phải có Flush để lập tức ép hệ điều hành đẩy gói tin MOVE sang Client 2.
                // Nếu thiếu lệnh này, gói tin MOVE (chỉ vài byte) sẽ bị kẹt lại trong OS Buffer, 
                // Client 2 sẽ mãi chờ mà không bao giờ vẽ được bàn cờ.
                stream.Flush();
            }
            catch { }
        }

        
        public static void WriteLog(string message) // GHI LOG CHO SERVER
        {
            try
            {
                string logEntry = $"[{DateTime.Now:dd/MM/yyyy HH:mm:ss}] {message}";

                // 1. In ra màn hình đen (Console) của Server để dễ quan sát
                Console.WriteLine(logEntry);

                // 2. Ghi lưu vào file server_log.txt
                File.AppendAllText("server_log.txt", logEntry + Environment.NewLine);
            }
            catch
            {
                // Bắt lỗi im lặng, tránh làm sập Server nếu file đang bị khóa
            }
        }
    }
}