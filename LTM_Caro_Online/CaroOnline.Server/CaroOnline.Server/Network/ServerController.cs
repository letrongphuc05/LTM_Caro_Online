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
    }
}