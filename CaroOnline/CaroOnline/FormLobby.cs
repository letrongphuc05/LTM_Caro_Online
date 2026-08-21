using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace CaroOnline
{
    public partial class FormLobby : Form
    {
        // 1. Biến Mạng và Text
        private string targetIP;
        private int targetPort;
        private System.Windows.Forms.Timer textTimer;
        private int dotCount = 0;

        // 2. Biến Hiệu ứng Hạt (X, O)
        private System.Windows.Forms.Timer animTimer;
        private List<SymbolParticle> particles;
        private Random rand = new Random();

        // Cấu trúc của 1 hạt X/O
        class SymbolParticle
        {
            public float X, Y, SpeedY;
            public string Text;
            public Font Font;
            public Color Color;
        }

        // HÀM KHỞI TẠO
        public FormLobby(string ip, int port)
        {
            InitializeComponent();
            targetIP = ip;
            targetPort = port;

            // Bật DoubleBuffer chống giật nháy khi vẽ hạt
            this.DoubleBuffered = true;

            // Gọi hàm tạo hạt X O
            InitBackgroundAnimation();

            // Đăng ký sự kiện khi Form vừa hiện lên
            this.Load += FormLobby_Load;
        }

        // SỰ KIỆN KHI FORM MỞ LÊN (Load)
        private async void FormLobby_Load(object sender, EventArgs e)
        {
            this.Text = "Phòng Ghép Trận | Le Doan Dat - 038206000230";

            // Hiệu ứng chữ nhấp nháy
            textTimer = new System.Windows.Forms.Timer { Interval = 500 };
            textTimer.Tick += (s, args) =>
            {
                dotCount = (dotCount + 1) % 4;
                lblStatus.Text = "Đang tìm kiếm đối thủ" + new string('.', dotCount);
            };
            textTimer.Start();

            // Khởi tạo mạng
            FormMain gameBoard = new FormMain();
            CaroOnline.Network.SocketManager socket = new CaroOnline.Network.SocketManager(gameBoard);

            try
            {
                // Chạy lệnh kết nối mạng ngầm (Task.Run) để không làm đơ hạt X O bay
                await System.Threading.Tasks.Task.Run(() =>
                {
                    socket.Connect(targetIP, targetPort);
                });

                // Tới đây là đã kết nối thành công, chuyển sang bàn cờ
                textTimer.Stop();
                this.Hide();
                gameBoard.ShowDialog();
                this.Close();
            }
            catch (Exception ex)
            {
                textTimer.Stop();
                lblStatus.Text = "Lỗi mạng!";
                MessageBox.Show("Không thể kết nối đến mạng: " + ex.Message);
                this.Close(); // Đóng sảnh để quay lại Form Khởi động
            }
        }

        // --- CÁC HÀM XỬ LÝ HIỆU ỨNG HẠT X, O BAY TRÊN NỀN ---
        private void InitBackgroundAnimation()
        {
            particles = new List<SymbolParticle>();
            // Tạo 15 ký tự bay ngẫu nhiên
            for (int i = 0; i < 15; i++)
            {
                particles.Add(CreateRandomParticle());
            }

            animTimer = new System.Windows.Forms.Timer { Interval = 40 }; // ~25 fps
            animTimer.Tick += AnimTimer_Tick;
            animTimer.Start();
        }

        private SymbolParticle CreateRandomParticle()
        {
            bool isX = rand.Next(2) == 0;
            return new SymbolParticle
            {
                X = rand.Next(0, this.ClientSize.Width),
                Y = rand.Next(-100, this.ClientSize.Height),
                SpeedY = (float)(rand.NextDouble() * 1.5 + 0.5), // Rơi chậm rãi
                Text = isX ? "X" : "O",
                Font = new Font("Comic Sans MS", rand.Next(15, 30), FontStyle.Bold),
                // X (Đỏ nhạt), O (Xanh nhạt) với độ mờ 60 để làm nền chìm
                Color = isX ? Color.FromArgb(60, 255, 50, 50) : Color.FromArgb(60, 50, 50, 255)
            };
        }

        private void AnimTimer_Tick(object sender, EventArgs e)
        {
            foreach (var p in particles)
            {
                p.Y -= p.SpeedY; // Bay từ dưới lên trên
                // Nếu bay khuất khỏi trên thì rớt lại từ dưới đáy
                if (p.Y + 50 < 0)
                {
                    p.Y = this.ClientSize.Height;
                    p.X = rand.Next(0, this.ClientSize.Width);
                }
            }
            this.Invalidate(); // Yêu cầu Form vẽ lại màn hình
        }

        // Bắt sự kiện vẽ của Form để in các chữ X O ra
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias; // Chống răng cưa

            foreach (var p in particles)
            {
                using (SolidBrush brush = new SolidBrush(p.Color))
                {
                    e.Graphics.DrawString(p.Text, p.Font, brush, p.X, p.Y);
                }
            }
        }
    }
}