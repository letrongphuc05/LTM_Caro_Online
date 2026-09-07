using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.IO;

namespace CaroOnline.Network
{
    public class SocketManager
    {
        public static SocketManager Instance { get; set; } = new SocketManager();
        private TcpClient? client;
        private NetworkStream? stream;
        private bool isConnected = false;
        private string logFilePath = "client_log.txt";

        public Action<string>? OnReceiveChallenge;
        public Action<string>? OnReceiveHistory; // Tính năng mới từ nhánh Gia-Huy
        public Action<string[]>? OnUpdateOnlineList;
        public Action<string[]>? OnUpdateRoomList;
        public Action<string, string, int>? OnMatchStart;

        // [NOTE QUAN TRỌNG - KIẾN TRÚC EVENT-DRIVEN]:
        // Thêm 3 Action thay thế hoàn toàn cho biến mainForm. 
        // Triệt tiêu hoàn toàn lỗi luồng ẩn (Background thread) không gọi được UI.
        public Action<int, int>? OnReceiveMove;
        public Action? OnOpponentDisconnected;
        public Action<bool, string>? OnConnectionChanged;
        public Action? OnRematchRequest;       // Khi nhận REMATCH_REQUEST từ đối thủ
        public Action? OnRematchAccept;        // Khi đối thủ đồng ý tái đấu
        public Action? OnRematchDecline;       // Khi đối thủ từ chối tái đấu

        public string[] LastOnlineList { get; private set; } = new string[0];
        public string[] LastRoomList { get; private set; } = new string[0];

        // [NOTE QUAN TRỌNG - CHỐNG RACE CONDITION]:
        // Dùng để hứng tạm nước cờ trong thời gian giao diện Bàn Cờ đang Load
        public int PendingMoveX { get; set; } = -1;
        public int PendingMoveY { get; set; } = -1;

        public SocketManager() { }

        // GHI LOG
        private void WriteLog(string message)
        {
            try
            {
                string logEntry = $"[{DateTime.Now:dd/MM/yyyy HH:mm:ss}] {message}{Environment.NewLine}";
                File.AppendAllText(logFilePath, logEntry);
            }
            catch { }
        }

        // kết nối
        public bool Connect(string ip, int port) { return Connect(ip, port, ""); }

        public bool Connect(string ip, int port, string extraParam)
        {
            try
            {
                client = new TcpClient();
                client.Connect(ip, port);
                stream = client.GetStream();
                isConnected = true;

                WriteLog($"Kết nối thành công tới Server tại {ip}:{port}.");
                OnConnectionChanged?.Invoke(true, "Kết nối Server thành công!");

                if (!string.IsNullOrEmpty(extraParam)) Send($"LOGIN|{extraParam}");

                Thread listenThread = new Thread(ReceiveData);
                listenThread.IsBackground = true;
                listenThread.Start();
                return true;
            }
            catch (Exception ex)
            {
                WriteLog($"Lỗi kết nối: {ex.Message}");
                OnConnectionChanged?.Invoke(false, "Không thể kết nối đến Server!");
                return false;
            }
        }

        // gửi
        public void Send(string data)
        {
            if (!isConnected || stream == null) return;
            try
            {
                byte[] buffer = Encoding.UTF8.GetBytes(data + "\n");
                stream.Write(buffer, 0, buffer.Length);
                stream.Flush();
            }
            catch { Disconnect(); }
        }

        // đánh cờ
        public void SendChallenge(string targetUser) { Send($"CHALLENGE|{targetUser}"); }
        public void AcceptChallenge(string challenger) { Send($"ACCEPT|{challenger}"); }
        public void DeclineChallenge(string challenger) { Send($"DECLINE|{challenger}"); }

        // khán giả
        public void JoinRoomAsSpectator(string roomID) { Send($"SPECTATE|{roomID}"); }

        // NHẬN DỮ LIỆU
        private void ReceiveData()
        {
            byte[] buffer = new byte[1024];
            string messageBuffer = "";

            try
            {
                while (isConnected && stream != null)
                {
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break;

                    string rawData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    messageBuffer += rawData;

                    int newlineIndex;
                    while ((newlineIndex = messageBuffer.IndexOf('\n')) >= 0)
                    {
                        string data = messageBuffer.Substring(0, newlineIndex).Trim();
                        messageBuffer = messageBuffer.Substring(newlineIndex + 1);

                        if (string.IsNullOrEmpty(data)) continue;

                        // GHI LẠI MỌI THÔNG ĐIỆP NHẬN TỪ SERVER VÀO FILE LOG (Từ nhánh Gia-Huy)
                        WriteLog($"Nhận từ Server: {data}");

                        // Tính năng History gộp từ nhánh Gia-Huy
                        if (data.StartsWith("HISTORY"))
                        {
                            OnReceiveHistory?.Invoke(data);
                        }
                        else if (data.StartsWith("MOVE"))
                        {
                            string[] parts = data.Split('|');
                            if (parts.Length >= 3 &&
                                int.TryParse(parts[1].Trim(), out int x) &&
                                int.TryParse(parts[2].Trim(), out int y))
                            {
                                // Phát sự kiện thay vì gọi trực tiếp
                                if (OnReceiveMove != null)
                                {
                                    OnReceiveMove.Invoke(x, y);
                                }
                                else
                                {
                                    // Bỏ túi dữ liệu nếu giao diện cờ chưa load kịp để bắt sự kiện
                                    PendingMoveX = x;
                                    PendingMoveY = y;
                                }
                            }
                        }
                        // danh sách trực tuyến
                        else if (data.StartsWith("ONLINE"))
                        {
                            string content = data.Substring("ONLINE".Length).TrimStart('|');
                            string[] players = content.Split('|', StringSplitOptions.RemoveEmptyEntries);
                            LastOnlineList = players;
                            OnUpdateOnlineList?.Invoke(players);
                        }
                        // phòng đấu
                        else if (data.StartsWith("ROOMS"))
                        {
                            string content = data.Substring("ROOMS".Length).TrimStart('|');
                            string[] rooms = content.Split('|', StringSplitOptions.RemoveEmptyEntries);
                            LastRoomList = rooms;
                            OnUpdateRoomList?.Invoke(rooms);
                        }
                        else if (data.StartsWith("ROOM|"))
                        {
                            string[] parts = data.Split('|');
                            if (parts.Length >= 4)
                            {
                                string roomId = parts[1];
                                string opponent = parts[2];
                                int.TryParse(parts[3], out int role);
                                OnMatchStart?.Invoke(roomId, opponent, role);
                            }
                        }
                        else if (data.StartsWith("CHALLENGE"))
                        {
                            string challenger = data.Substring("CHALLENGE".Length).TrimStart('|');
                            OnReceiveChallenge?.Invoke(challenger);
                        }
                        else if (data.StartsWith("REMATCH_REQUEST"))
                        {
                            OnRematchRequest?.Invoke();
                        }
                        else if (data.StartsWith("REMATCH_ACCEPT"))
                        {
                            OnRematchAccept?.Invoke();
                        }
                        else if (data.StartsWith("REMATCH_DECLINE"))
                        {
                            OnRematchDecline?.Invoke();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog($"Lỗi nhận dữ liệu ngầm: {ex.Message}");
            }
            finally
            {
                Disconnect();
            }
        }

        // mất kết nối
        public void Disconnect()
        {
            if (!isConnected) return;
            isConnected = false;
            try
            {
                stream?.Close();
                client?.Close();
            }
            catch { }

            OnOpponentDisconnected?.Invoke();
        }
    }

    // độ TƯƠNG THÍCH mạng 
    public static class NetworkManager
    {
        public static SocketManager Instance
        {
            get { return SocketManager.Instance; }
        }
    }
}