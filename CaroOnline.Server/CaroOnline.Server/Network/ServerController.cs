using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CaroOnline.Server.Rooms;
using CaroOnline.Server.History;
using CaroOnline.Server.Spectator;

namespace CaroOnline.Server.Network
{
    public class ServerController
    {
        private TcpListener? _listener;
        private CancellationTokenSource? _cancellationTokenSource;

        // Quản lý khán giả và lịch sử nước đi theo từng phòng.
        private readonly SpectatorManager _spectatorManager;
        private readonly SpectatorMessageHandler _spectatorHandler;

        // Lịch sử trận đấu của đúng phiên Server hiện tại.
        private readonly HistoryController _historyController;
        private readonly Dictionary<int, string> _activeHistoryGameIds;
        private readonly object _historyLock = new object();

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

            _spectatorManager = new SpectatorManager();
            _spectatorHandler = new SpectatorMessageHandler(_spectatorManager);

            _historyController = new HistoryController();
            _activeHistoryGameIds = new Dictionary<int, string>();
        }

        public void Start(string ip, int port)
        {
            if (IsRunning) return;

            // Mỗi lần Start là một phiên Server mới => lịch sử bắt đầu từ rỗng.
            _historyController.Clear();
            lock (_historyLock)
            {
                _activeHistoryGameIds.Clear();
            }

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

                // Nếu client là khán giả thì gỡ khỏi tất cả phòng đang xem.
                _spectatorManager.RemoveClient(connection);

                if (connection.RoomId >= 0)
                {
                    GameRoom? room = RoomManager.Instance.GetRoom(connection.RoomId);
                    if (room != null)
                    {
                        int roomId = connection.RoomId;
                        RoomManager.Instance.RemoveRoom(roomId);
                        _spectatorManager.RemoveRoom(roomId.ToString());
                        lock (_historyLock)
                        {
                            _activeHistoryGameIds.Remove(roomId);
                        }
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

            // Khi tắt Server, lịch sử phiên hiện tại biến mất hoàn toàn.
            _historyController.Clear();
            lock (_historyLock)
            {
                _activeHistoryGameIds.Clear();
            }

            Log("Server stopped. History cleared.");
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
                case "GAME_END": HandleGameEnd(connection, parts); break;

                // Lịch sử trận đấu.
                case "HISTORY_ALL":
                case "HISTORY_GAME":
                case "HISTORY_PLAYER":
                case "HISTORY_ROOM":
                    HandleHistory(message, stream);
                    break;

                // Khán giả: đăng ký xem phòng và nhận lại toàn bộ nước đi cũ.
                case "SPECTATE": HandleSpectate(connection, message, stream); break;
                case "LEAVE_SPECTATE": HandleLeaveSpectate(connection, message, stream); break;

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

            string xText = parts[1].Trim();
            string yText = parts[2].Trim();
            string moveMessage = $"MOVE|{xText}|{yText}";

            if (!int.TryParse(xText, out int x) ||
                !int.TryParse(yText, out int y))
                return;

            // -5: thông báo ai được đi trước ở ván mới.
            // Đây là lúc tạo history đầu tiên của phòng vì lúc này đã biết X/O thực tế.
            if (x == -5)
            {
                bool player1GoesFirst = y == 1;

                lock (_historyLock)
                {
                    string playerX = player1GoesFirst
                        ? room.Player1.Username
                        : room.Player2.Username;

                    string playerO = player1GoesFirst
                        ? room.Player2.Username
                        : room.Player1.Username;

                    GameHistory history =
                        _historyController.StartGame(
                            Guid.NewGuid().ToString(),
                            connection.RoomId.ToString(),
                            playerX,
                            playerO);

                    _activeHistoryGameIds[connection.RoomId] = history.GameId;
                }
            }
            // -4: hai người đồng ý tái đấu => mở một GameHistory mới.
            else if (x == -4)
            {
                lock (_historyLock)
                {
                    GameHistory? previousGame = null;

                    if (_activeHistoryGameIds.TryGetValue(
                        connection.RoomId, out string? previousGameId))
                    {
                        previousGame =
                            _historyController.GetGame(previousGameId);
                    }

                    if (previousGame != null &&
                        !string.IsNullOrWhiteSpace(previousGame.Winner))
                    {
                        string playerX = previousGame.Winner;
                        string playerO = string.Equals(
                            previousGame.PlayerX,
                            playerX,
                            StringComparison.OrdinalIgnoreCase)
                            ? previousGame.PlayerO
                            : previousGame.PlayerX;

                        GameHistory newGame =
                            _historyController.StartGame(
                                Guid.NewGuid().ToString(),
                                connection.RoomId.ToString(),
                                playerX,
                                playerO);

                        _activeHistoryGameIds[connection.RoomId] =
                            newGame.GameId;
                    }
                }
            }
            // -1: người hiện tại hết giờ => đối thủ thắng.
            else if (x == -1)
            {
                lock (_historyLock)
                {
                    if (_activeHistoryGameIds.TryGetValue(
                        connection.RoomId, out string? gameId))
                    {
                        ClientConnection? opponent =
                            room.GetOpponent(connection);

                        if (opponent != null)
                        {
                            _historyController.EndGame(
                                gameId, opponent.Username);
                        }
                    }
                }
            }
            // Nước đi thật trên bàn cờ.
            else if (x >= 0 && y >= 0)
            {
                lock (_historyLock)
                {
                    if (_activeHistoryGameIds.TryGetValue(
                        connection.RoomId, out string? gameId))
                    {
                        _historyController.AddMove(
                            gameId,
                            connection.Username,
                            y,
                            x);
                    }
                }

                // Lưu cho spectator và phát theo thời gian thực.
                _spectatorManager.SaveMove(
                    connection.RoomId.ToString(),
                    moveMessage);

                _ = _spectatorManager.BroadcastAsync(
                    connection.RoomId.ToString(),
                    moveMessage);
            }

            // Luồng game 2 người hiện có: gửi lệnh MOVE cho đối thủ.
            ClientConnection? opponentClient = room.GetOpponent(connection);
            if (opponentClient == null) return;

            try
            {
                NetworkStream? opponentStream =
                    opponentClient.Client.GetStream();

                if (opponentStream != null)
                    SendMessage(opponentStream, moveMessage);
            }
            catch { }
        }

        private void HandleGameEnd(
            ClientConnection connection,
            string[] parts)
        {
            if (parts.Length < 2 ||
                connection.RoomId < 0)
                return;

            // Chỉ người vừa thắng mới gửi GAME_END|WIN.
            if (!string.Equals(
                parts[1].Trim(),
                "WIN",
                StringComparison.OrdinalIgnoreCase))
                return;

            lock (_historyLock)
            {
                if (_activeHistoryGameIds.TryGetValue(
                    connection.RoomId, out string? gameId))
                {
                    _historyController.EndGame(
                        gameId,
                        connection.Username);
                }
            }
        }

        private void HandleHistory(
            string message,
            NetworkStream stream)
        {
            string response =
                _historyController.HandleRequest(message);

            if (string.IsNullOrWhiteSpace(response))
                return;

            // BuildResponse có thể chứa nhiều dòng HISTORY.
            foreach (string line in response.Split('\n', StringSplitOptions.RemoveEmptyEntries))
            {
                SendMessage(stream, line.Trim());
            }
        }

        private void HandleSpectate(
            ClientConnection connection,
            string message,
            NetworkStream stream)
        {
            string response =
                _spectatorHandler.HandleMessage(message, connection);

            if (string.IsNullOrEmpty(response))
                return;

            SendMessage(stream, response);

            string[] responseParts = response.Split('|');
            if (responseParts.Length < 2 ||
                responseParts[0] != "SPECTATE_OK")
                return;

            string roomId = responseParts[1];
            List<string> history = _spectatorManager.GetMoveHistory(roomId);

            Log(
                $"Spectator {connection.Address} joined room {roomId}. " +
                $"Replaying {history.Count} moves.");

            // Gửi toàn bộ nước cờ cũ theo đúng thứ tự để dựng lại bàn cờ.
            foreach (string oldMove in history)
            {
                SendMessage(stream, oldMove);
            }

            _ = _spectatorManager.BroadcastSpectatorCountAsync(roomId);
        }

        private void HandleLeaveSpectate(
            ClientConnection connection,
            string message,
            NetworkStream stream)
        {
            string response =
                _spectatorHandler.HandleMessage(message, connection);

            if (string.IsNullOrEmpty(response))
                return;

            SendMessage(stream, response);

            string[] responseParts = response.Split('|');
            if (responseParts.Length >= 2 &&
                responseParts[0] == "LEAVE_SPECTATE_OK")
            {
                _ = _spectatorManager.BroadcastSpectatorCountAsync(
                    responseParts[1]);
            }
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