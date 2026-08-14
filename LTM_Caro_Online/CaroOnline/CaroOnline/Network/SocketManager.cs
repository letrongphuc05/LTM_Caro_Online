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
        private FormMain? mainForm;
        private bool isConnected = false;
        private string logFilePath = "client_log.txt";
        public Action<string[]>? UpdateOnlineList;
        public Action<string[]>? UpdateMatchRooms;
        public Action<string>? OnReceiveChallenge;
        public Action<string[]>? OnUpdateOnlineList;
        public Action<string[]>? OnUpdateRoomList;
        public SocketManager()
        {
        }

        public SocketManager(FormMain form)
        {
            mainForm = form;
        }

        public void SetMainForm(FormMain form)
        {
            mainForm = form;
        }

        // GHI LOG
       
        private void WriteLog(string message)
        {
            try
            {
                string logEntry =
                    $"[{DateTime.Now:dd/MM/yyyy HH:mm:ss}] " +
                    ($"{message}{Environment.NewLine}");

                File.AppendAllText(logFilePath, logEntry);
            }
            catch
            {
                
            }
        }


        // kết nối

        public bool Connect(string ip, int port)
        {
            return Connect(ip, port, "");
        }

        public bool Connect(string ip, int port, string extraParam)
        {
            try
            {
                client = new TcpClient();

                client.Connect(ip, port);

                stream = client.GetStream();

                isConnected = true;

                WriteLog(
                    $"Kết nối thành công tới Server tại {ip}:{port}."
                );

                mainForm?.Network_OnConnectionChanged(
                    true,
                    "Kết nối Server thành công!"
                );

                Thread listenThread = new Thread(ReceiveData);

                listenThread.IsBackground = true;

                listenThread.Start();

                return true;
            }
            catch (Exception ex)
            {
                WriteLog(
                    $"Lỗi kết nối tới Server: {ex.Message}"
                );

                mainForm?.Network_OnConnectionChanged(
                    false,
                    "Không thể kết nối đến Server!"
                );

                return false;
            }
        }



        // gửi

        public void Send(string data)
        {
            if (!isConnected || stream == null)
                return;

            try
            {
                byte[] buffer =
                    Encoding.UTF8.GetBytes(data);

                stream.Write(
                    buffer,
                    0,
                    buffer.Length
                );

                if (data.StartsWith("MOVE"))
                {
                    WriteLog($"Mình đánh: {data}");
                }
            }
            catch
            {
                WriteLog(
                    "Lỗi gửi dữ liệu, mất kết nối với Server."
                );

                Disconnect();
            }
        }



        // đánh cờ

        public void SendChallenge(string targetUser)
        {
            Send($"CHALLENGE|{targetUser}");
        }

        public void AcceptChallenge(string challenger)
        {
            Send($"ACCEPT|{challenger}");
        }

        public void DeclineChallenge(string challenger)
        {
            Send($"DECLINE|{challenger}");
        }


        // khán giả

        public void JoinRoomAsSpectator(string roomID)
        {
            Send($"SPECTATE|{roomID}");
        }


        // NHẬN DỮ LIỆU

        private void ReceiveData()
        {
            byte[] buffer = new byte[1024];

            try
            {
                while (isConnected && stream != null)
                {
                    int bytesRead =
                        stream.Read(
                            buffer,
                            0,
                            buffer.Length
                        );

                    if (bytesRead == 0)
                    {
                        WriteLog(
                            "Server đã đóng kết nối hoặc phòng chơi bị hủy."
                        );

                        break;
                    }

                    string data =
                        Encoding.UTF8.GetString(
                            buffer,
                            0,
                            bytesRead
                        );

                    // GHI LẠI MỌI THÔNG ĐIỆP NHẬN TỪ SERVER VÀO FILE LOG
                    WriteLog($"Nhận từ Server: {data}");

                    if (data.StartsWith("MOVE"))
                    {
                        string[] parts =
                            data.Split('|');

                        if (parts.Length == 3)
                        {
                            if (
                                int.TryParse(
                                    parts[1],
                                    out int x
                                )
                                &&
                                int.TryParse(
                                    parts[2],
                                    out int y
                                )
                            )
                            {
                                mainForm?.Network_OnReceiveMove(
                                    x,
                                    y
                                );
                            }
                        }
                    }

                    // danh sách trực tuyến

                    else if (data.StartsWith("ONLINE"))
                    {
                        string content =
                            data.Substring("ONLINE".Length)
                                .TrimStart('|');

                        string[] players =
                            content.Split(
                                '|',
                                StringSplitOptions.RemoveEmptyEntries
                            );

                        UpdateOnlineList?.Invoke(players);

                        OnUpdateOnlineList?.Invoke(players);
                    }

                    // phòng đấu
                    

                    else if (data.StartsWith("ROOMS"))
                    {
                        string content =
                            data.Substring("ROOMS".Length)
                                .TrimStart('|');

                        string[] rooms =
                            content.Split(
                                '|',
                                StringSplitOptions.RemoveEmptyEntries
                            );

                        UpdateMatchRooms?.Invoke(rooms);

                        OnUpdateRoomList?.Invoke(rooms);
                    }


                    else if (data.StartsWith("CHALLENGE"))
                    {
                        string challenger =
                            data.Substring("CHALLENGE".Length)
                                .TrimStart('|');

                        OnReceiveChallenge?.Invoke(challenger);
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog(
                    $"Đứt kết nối mạng ngầm: {ex.Message}"
                );
            }
            finally
            {
                Disconnect();
            }
        }


        // mất kết nối

        public void Disconnect()
        {
            if (!isConnected)
                return;

            isConnected = false;

            try
            {
                stream?.Close();
                client?.Close();
            }
            catch
            {
               
            }

            WriteLog(
                "Đã ngắt hoàn toàn kết nối với hệ thống Server."
            );

            mainForm?.Network_OnOpponentDisconnected();
        }
    }


    // độ TƯƠNG THÍCH mạng 
    public static class NetworkManager
    {
        public static SocketManager Instance
        {
            get
            {
                return SocketManager.Instance;
            }
        }
    }
}