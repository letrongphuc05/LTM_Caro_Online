using System;
using System.Windows.Forms;

namespace CaroOnline.Network
{
    public class SocketManager
    {
        // [DÀNH CHO NGƯỜI LÀM MẠNG]: Khai báo Socket TCP/UDP ở đây

        public static void Connect(string ip, int port)
        {
            // Giả lập kết nối mạng để test UI
            MessageBox.Show($"[Khung Mạng] Backend đang xử lý kết nối tới {ip}:{port}...", "Thông báo cho Backend");

            // Chú ý cho Backend: Nhớ gọi hàm Network_OnConnectionChanged của FormMain khi kết nối xong!
        }

        public static void Send(string message)
        {
            // Giả lập gửi tọa độ mạng
            MessageBox.Show($"[Khung Mạng] Gói tin gửi đi: {message}", "Thông báo cho Backend");
        }

        public static void CloseConnection()
        {
            // Giả lập ngắt kết nối
            // MessageBox.Show("[Khung Mạng] Đã đóng kết nối Socket.");
        }
    }
}