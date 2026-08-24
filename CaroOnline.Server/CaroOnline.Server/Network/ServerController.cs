using System.Net;
using System.Net.Sockets;

namespace CaroOnline.Server.Network
{
    public class ServerController
    {
        private TcpListener? _listener;
        private CancellationTokenSource? _cancellationTokenSource;

        public bool IsRunning { get; private set; }

        public int ClientCount { get; private set; }

        public int TotalConnections { get; private set; }

        public ServerController()
        {
            IsRunning = false;
            ClientCount = 0;
            TotalConnections = 0;
        }

        public void Start(string ip, int port)
        {
            if (IsRunning)
                return;

            IPAddress ipAddress = IPAddress.Parse(ip);

            _listener = new TcpListener(ipAddress, port);

            _listener.Start();

            IsRunning = true;

            _cancellationTokenSource = new CancellationTokenSource();

            _ = AcceptClientsAsync(_cancellationTokenSource.Token);
        }
        private async Task AcceptClientsAsync(CancellationToken token)
        {
            while (IsRunning && !token.IsCancellationRequested)
            {
                try
                {
                    if (_listener == null)
                        break;

                    TcpClient client =
                        await _listener.AcceptTcpClientAsync(token);

                    ClientCount++;
                    TotalConnections++;
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
            }
        }

        public void Stop()
        {
            if (!IsRunning)
                return;

            _cancellationTokenSource?.Cancel();

            if (_listener != null)
            {
                _listener.Stop();
                _listener = null;
            }

            IsRunning = false;
            ClientCount = 0;
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