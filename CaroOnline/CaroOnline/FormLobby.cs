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

            // 1. Hiệu ứng Fade-in (Từ trong suốt hiện dần lên)
            this.Opacity = 0;
            System.Windows.Forms.Timer fadeTimer = new System.Windows.Forms.Timer { Interval = 20 };
            fadeTimer.Tick += (s, args) => {
                if (this.Opacity < 1) this.Opacity += 0.05;
                else fadeTimer.Stop();
            };
            fadeTimer.Start();

            // 2. Bắt đầu đếm giờ
            searchStartTime = DateTime.Now;

            // 3. Hiệu ứng chữ nhấp nháy + Đồng hồ thời gian thực
            textTimer = new System.Windows.Forms.Timer { Interval = 500 };
            textTimer.Tick += (s, args) =>
            {
                dotCount = (dotCount + 1) % 4;
                lblStatus.Text = "Đang tìm kiếm đối thủ" + new string('.', dotCount);
                lblStatus.ForeColor = dotCount % 2 == 0 ? Color.Cyan : Color.DeepSkyBlue;
            };

            // ... (ĐOẠN CODE KHỞI TẠO MẠNG VÀ TASK.RUN BÊN DƯỚI BẠN GIỮ NGUYÊN) ...

            // Khởi tạo mạng
            FormMain gameBoard = new FormMain();
            CaroOnline.Network.SocketManager socket = new CaroOnline.Network.SocketManager();

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
        // ... (các biến cũ)
        private DateTime searchStartTime;
        private float radarRadius = 0;
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
            // Code bay của X O (giữ nguyên)
            foreach (var p in particles)
            {
                p.Y -= p.SpeedY;
                if (p.Y + 50 < 0)
                {
                    p.Y = this.ClientSize.Height;
                    p.X = rand.Next(0, this.ClientSize.Width);
                }
            }

            // Thêm code Sóng Radar mở rộng
            radarRadius += 4;
            if (radarRadius > 400) radarRadius = 0;

            this.Invalidate();
        }


        // Bắt sự kiện vẽ của Form để in các chữ X O ra
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // 1. Vẽ vòng sóng Radar tỏa ra từ trung tâm
            int centerX = this.ClientSize.Width / 2;
            int centerY = this.ClientSize.Height / 2;
            int alpha = (int)(255 * (1 - radarRadius / 400f)); // Mờ dần khi to ra

            if (alpha > 0)
            {
                using (Pen radarPen = new Pen(Color.FromArgb(alpha, 0, 255, 255), 2))
                {
                    e.Graphics.DrawEllipse(radarPen, centerX - radarRadius, centerY - radarRadius, radarRadius * 2, radarRadius * 2);
                }
            }

            // 2. Vẽ các hạt X, O bay (Giữ nguyên)
            foreach (var p in particles)
            {
                using (SolidBrush brush = new SolidBrush(p.Color))
                {
                    e.Graphics.DrawString(p.Text, p.Font, brush, p.X, p.Y);
                }
            }
            // ... (code vẽ hạt X O giữ nguyên)

            // Vẽ đồng hồ đếm giờ với Font nhỏ (Căn giữa tuyệt đối)
            TimeSpan elapsed = DateTime.Now - searchStartTime;
            string timerText = $"⏳ Thời gian tìm: {elapsed.Minutes:D2}:{elapsed.Seconds:D2}";

            using (Font timerFont = new Font("Segoe UI", 12, FontStyle.Regular)) // Kích thước chữ 12
            using (SolidBrush timerBrush = new SolidBrush(Color.White)) // Màu trắng nổi bật trên nền tối
            {
                // Đo kích thước chuỗi để tự động căn giữa màn hình
                SizeF textSize = e.Graphics.MeasureString(timerText, timerFont);
                float xPos = (this.ClientSize.Width - textSize.Width) / 2;

                // Đặt vị trí Y nằm ngay bên dưới dòng chữ "Đang tìm kiếm..."
                float yPos = (this.ClientSize.Height / 2) + 40;

                e.Graphics.DrawString(timerText, timerFont, timerBrush, xPos, yPos);
            }

            // 3. Watermark bản quyền (giữ nguyên code cũ của bạn)
            // ...
            
           
        }
    }
}