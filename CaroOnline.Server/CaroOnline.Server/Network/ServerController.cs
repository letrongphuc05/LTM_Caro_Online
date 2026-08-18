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

        public event Action<string>? ClientConnected;

        public event Action<string>? ClientDisconnected;

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

            _cancellationTokenSource =
                new CancellationTokenSource();

            _ = AcceptClientsAsync(
                _cancellationTokenSource.Token);
        }

        private async Task AcceptClientsAsync(
            CancellationToken token)
        {
            while (IsRunning &&
                   !token.IsCancellationRequested)
            {
                try
                {
                    if (_listener == null)
                        break;
                   //Client kết nối -> Server lấy địa chỉ IP:Port -> ClientConnected được phát -> Dashboard nhận event -> Dashboard ghi log
                    TcpClient client =
                        await _listener.AcceptTcpClientAsync(token);

                    ClientCount++;
                    TotalConnections++;

                    string clientAddress =
                        client.Client.RemoteEndPoint?.ToString()
                        ?? "Unknown";

                    ClientConnected?.Invoke(clientAddress);

                    _ = MonitorClientAsync(client, token);
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

        private async Task MonitorClientAsync(
            TcpClient client,
            CancellationToken token)
        {
            string clientAddress =
               client.Client.RemoteEndPoint?.ToString()
                 ?? "Unknown";
            try
            {
                while (IsRunning &&
                       !token.IsCancellationRequested)
                {
                    bool disconnected =
                        client.Client.Poll(
                            1000,
                            SelectMode.SelectRead)
                        &&
                        client.Client.Available == 0;

                    if (disconnected)
                    {
                        break;
                    }

                    await Task.Delay(500, token);
                }
            }
            catch (OperationCanceledException)
            {
                // Server dang Stop
            }
            catch (SocketException)
            {
                // Client bi mat ket noi
            }
            finally
            {
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
    }
}