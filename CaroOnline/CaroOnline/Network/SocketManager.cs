using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace CaroOnline.Network
{
    public class SocketManager
    {
        private TcpClient client;
        private NetworkStream stream;
        private FormMain mainForm;
        private bool isConnected = false;

        public SocketManager(FormMain form)
        {
            mainForm = form;
        }


        // TỰ ĐỘNG CHỌN CLIENT HOẶC SERVER
        public void Connect(string ip, int port)
        {
            try
            {
                client = new TcpClient();
                client.Connect(ip, port);
                stream = client.GetStream();
                isConnected = true;

                mainForm.Network_OnConnectionChanged(true, "Kết nối thành công (Khách)!");

                Thread listenThread = new Thread(ReceiveData);
                listenThread.IsBackground = true;
                listenThread.Start();
            }
            catch
            {
                try
                {
                    TcpListener listener = new TcpListener(IPAddress.Any, port);
                    listener.Start();
                    mainForm.Network_OnConnectionChanged(false, "Đang chờ đối thủ vào...");
                    client = listener.AcceptTcpClient();
                    stream = client.GetStream();
                    isConnected = true;
                    listener.Stop(); 
                    mainForm.Network_OnConnectionChanged(true, "Kết nối thành công (Chủ Phòng)!");
                    Thread listenThread = new Thread(ReceiveData);
                    listenThread.IsBackground = true;
                    listenThread.Start();
                }
                catch
                {
                    mainForm.Network_OnConnectionChanged(false, "Lỗi mạng!");
                }
            }
        }


        // CHỐNG VĂNG 
        public void Send(string data)
        {
            if (!isConnected || stream == null) return;

            try
            {
                byte[] buffer = Encoding.UTF8.GetBytes(data);
                stream.Write(buffer, 0, buffer.Length);
            }
            catch
            {
                Disconnect();
            }
        }

        // NHẬN DỮ LIỆU & BẮT LỖI RỚT MẠNG
        private void ReceiveData()
        {
            byte[] buffer = new byte[1024];
            try
            {
                while (isConnected && stream != null)
                {
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                    {
                        break;
                    }
                    string data = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    if (data.StartsWith("MOVE"))
                    {
                        string[] parts = data.Split('|');
                        if (parts.Length == 3)
                        {
                            if (int.TryParse(parts[1], out int x) && int.TryParse(parts[2], out int y))
                            {
                                mainForm.Network_OnReceiveMove(x, y);
                            }
                        }
                    }
                }
            }
            catch
            {

            }
            finally
            {
                Disconnect();
            }
        }

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
            mainForm.Network_OnOpponentDisconnected();
        }
    }
}