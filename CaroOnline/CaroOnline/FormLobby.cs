using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace CaroOnline
{
    public partial class FormLobby : Form
    {
        // ==============================
        // 1. THÔNG TIN KẾT NỐI SERVER
        // ==============================

        // IP của Server cần kết nối
        private string targetIP;

        // Port của Server cần kết nối
        private int targetPort;

        // Timer tạo hiệu ứng "Đang tìm kiếm đối thủ..."
        private System.Windows.Forms.Timer textTimer;

        // Dùng để tạo số lượng dấu chấm thay đổi liên tục
        private int dotCount = 0;


        // ==============================
        // 2. HIỆU ỨNG X O BAY TRÊN FORM
        // ==============================

        // Timer điều khiển chuyển động của các hạt X và O
        private System.Windows.Forms.Timer animTimer;

        // Danh sách các hạt X/O đang được vẽ trên màn hình
        private List<SymbolParticle> particles;

        // Dùng để tạo vị trí và tốc độ ngẫu nhiên
        private Random rand = new Random();


        // ==============================
        // 3. ĐỐI TƯỢNG KẾT NỐI MẠNG
        // ==============================

        // SocketManager chịu trách nhiệm kết nối Client tới Server
        private CaroOnline.Network.SocketManager socket;

        // FormMain là giao diện bàn cờ Caro
        private FormMain gameBoard;


        // ==============================
        // 4. CẤU TRÚC CỦA MỘT HẠT X/O
        // ==============================

        class SymbolParticle
        {
            // Vị trí X và Y của hạt trên màn hình
            public float X, Y;

            // Tốc độ di chuyển theo chiều dọc
            public float SpeedY;

            // Ký tự hiển thị: X hoặc O
            public string Text;

            // Font chữ của hạt
            public Font Font;

            // Màu của hạt
            public Color Color;
        }


        // ==============================
        // 5. HÀM KHỞI TẠO FORM
        // ==============================

        public FormLobby(string ip, int port)
        {
            InitializeComponent();

            // Lưu IP và Port được truyền từ Form trước
            targetIP = ip;
            targetPort = port;

            // Bật Double Buffer để giảm hiện tượng nhấp nháy
            // khi Form liên tục được vẽ lại
            this.DoubleBuffered = true;


            // ==============================
            // KHỞI TẠO SOCKET VÀ BÀN CỜ
            // ==============================

            // Tạo đối tượng SocketManager để kết nối Server
            socket = new CaroOnline.Network.SocketManager();

            // Tạo FormMain để chuẩn bị chuyển sang bàn cờ
            gameBoard = new FormMain();


            // ==============================
            // KHỞI TẠO HIỆU ỨNG X/O
            // ==============================

            InitBackgroundAnimation();

            // Đăng ký sự kiện Load của Form
            this.Load += FormLobby_Load;
        }


        // ==============================
        // 6. XỬ LÝ KHI FORM LOBBY MỞ
        // ==============================

        private async void FormLobby_Load(object sender, EventArgs e)
        {
            // Đặt tiêu đề cho cửa sổ Lobby
            this.Text = "Phòng Ghép Trận | Le Doan Dat - 038206000230";


            // ==============================
            // HIỆU ỨNG FADE-IN
            // ==============================

            // Hiệu ứng từ trong suốt hiện dần lên
            this.Opacity = 0;

            System.Windows.Forms.Timer fadeTimer =
                new System.Windows.Forms.Timer { Interval = 20 };

            fadeTimer.Tick += (s, args) =>
            {
                if (this.Opacity < 1)
                    this.Opacity += 0.05;
                else
                    fadeTimer.Stop();
            };

            fadeTimer.Start();


            // ==============================
            // BẮT ĐẦU ĐẾM GIỜ
            // ==============================

            searchStartTime = DateTime.Now;


            // ==============================
            // HIỆU ỨNG CHỮ "ĐANG TÌM KIẾM..."
            // ==============================

            textTimer = new System.Windows.Forms.Timer
            {
                Interval = 500
            };

            textTimer.Tick += (s, args) =>
            {
                dotCount = (dotCount + 1) % 4;

                lblStatus.Text =
                    "Đang tìm kiếm đối thủ" +
                    new string('.', dotCount);
            };

            textTimer.Start();


            // ==============================
            // KẾT NỐI TỚI SERVER
            // ==============================

            try
            {
                // Chạy kết nối trong Task riêng để giao diện
                // không bị đứng trong lúc đang kết nối Server
                await System.Threading.Tasks.Task.Run(() =>
                {
                    socket.Connect(targetIP, targetPort);
                });


                // Nếu chạy tới đây nghĩa là kết nối thành công

                // Dừng hiệu ứng tìm kiếm
                textTimer.Stop();

                // Ẩn Lobby
                this.Hide();

                // Mở bàn cờ Caro
                gameBoard.ShowDialog();

                // Đóng Lobby sau khi bàn cờ đóng
                this.Close();
            }
            catch (Exception ex)
            {
                // Nếu kết nối thất bại thì dừng Timer
                textTimer.Stop();

                // Hiển thị trạng thái lỗi
                lblStatus.Text = "Lỗi mạng!";


                // Thông báo lỗi cho người dùng
                MessageBox.Show(
                    "Không thể kết nối đến mạng: " + ex.Message,
                    "Lỗi kết nối",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );


                // Đóng Lobby để quay lại Form trước
                this.Close();
            }
        }


        // ==============================
        // 7. KHỞI TẠO HIỆU ỨNG X/O
        // ==============================

        private void InitBackgroundAnimation()
        {
            // Tạo danh sách chứa các hạt
            particles = new List<SymbolParticle>();


            // Tạo 15 hạt X/O ban đầu
            for (int i = 0; i < 15; i++)
            {
                particles.Add(CreateRandomParticle());
            }


            // Timer chạy khoảng 25 lần mỗi giây
            animTimer = new System.Windows.Forms.Timer
            {
                Interval = 40
            };


            // Đăng ký sự kiện Tick
            animTimer.Tick += AnimTimer_Tick;

            // Bắt đầu hiệu ứng
            animTimer.Start();
        }


        // ==============================
        // BIẾN PHỤC VỤ HIỆU ỨNG
        // ==============================

        private DateTime searchStartTime;

        private float radarRadius = 0;


        // ==============================
        // 8. TẠO MỘT HẠT X/O NGẪU NHIÊN
        // ==============================

        private SymbolParticle CreateRandomParticle()
        {
            // Random xem hạt này là X hay O
            bool isX = rand.Next(2) == 0;


            return new SymbolParticle
            {
                // Vị trí ngang ngẫu nhiên
                X = rand.Next(
                    0,
                    Math.Max(1, this.ClientSize.Width)
                ),

                // Vị trí dọc ngẫu nhiên
                Y = rand.Next(
                    -100,
                    Math.Max(1, this.ClientSize.Height)
                ),

                // Tốc độ bay ngẫu nhiên
                SpeedY = (float)(
                    rand.NextDouble() * 1.5 + 0.5
                ),

                // Chọn ký tự X hoặc O
                Text = isX ? "X" : "O",

                // Kích thước Font ngẫu nhiên
                Font = new Font(
                    "Comic Sans MS",
                    rand.Next(15, 30),
                    FontStyle.Bold
                ),

                // Màu X và O có độ trong suốt
                Color = isX
                    ? Color.FromArgb(60, 255, 50, 50)
                    : Color.FromArgb(60, 50, 50, 255)
            };
        }


        // ==============================
        // 9. XỬ LÝ CHUYỂN ĐỘNG CỦA X/O
        // ==============================

        private void AnimTimer_Tick(object sender, EventArgs e)
        {
            // Duyệt qua toàn bộ các hạt
            foreach (var p in particles)
            {
                // Cho hạt bay từ dưới lên trên
                p.Y -= p.SpeedY;


                // Nếu hạt bay ra khỏi màn hình
                // thì đưa nó trở lại phía dưới
                if (p.Y + 50 < 0)
                {
                    p.Y = this.ClientSize.Height;

                    p.X = rand.Next(
                        0,
                        Math.Max(1, this.ClientSize.Width)
                    );
                }
            }


            // ==============================
            // HIỆU ỨNG SÓNG RADAR
            // ==============================

            radarRadius += 4;

            if (radarRadius > 400)
                radarRadius = 0;


            // Yêu cầu Form vẽ lại
            this.Invalidate();
        }


        // ==============================
        // 10. VẼ CÁC HẠT X/O LÊN FORM
        // ==============================

        protected override void OnPaint(PaintEventArgs e)
        {
            // Gọi OnPaint của Form cha
            base.OnPaint(e);

            // Bật AntiAlias để chữ X/O mượt hơn
            e.Graphics.SmoothingMode =
                System.Drawing.Drawing2D.SmoothingMode.AntiAlias;


            // ==============================
            // VẼ VÒNG SÓNG RADAR
            // ==============================

            int centerX = this.ClientSize.Width / 2;
            int centerY = this.ClientSize.Height / 2;

            int alpha =
                (int)(255 * (1 - radarRadius / 400f));


            if (alpha > 0)
            {
                using (Pen radarPen = new Pen(
                    Color.FromArgb(alpha, 0, 255, 255),
                    2))
                {
                    e.Graphics.DrawEllipse(
                        radarPen,
                        centerX - radarRadius,
                        centerY - radarRadius,
                        radarRadius * 2,
                        radarRadius * 2
                    );
                }
            }


            // ==============================
            // VẼ CÁC HẠT X/O
            // ==============================

            foreach (var p in particles)
            {
                // Tạo Brush theo màu của hạt
                using (SolidBrush brush =
                    new SolidBrush(p.Color))
                {
                    // Vẽ ký tự X/O lên Form
                    e.Graphics.DrawString(
                        p.Text,
                        p.Font,
                        brush,
                        p.X,
                        p.Y
                    );
                }
            }


            // ==============================
            // VẼ ĐỒNG HỒ ĐẾM GIỜ
            // ==============================

            TimeSpan elapsed =
                DateTime.Now - searchStartTime;

            string timerText =
                $"⏳ Thời gian tìm: {elapsed.Minutes:D2}:{elapsed.Seconds:D2}";


            using (Font timerFont =
                new Font(
                    "Segoe UI",
                    12,
                    FontStyle.Regular))
            using (SolidBrush timerBrush =
                new SolidBrush(Color.White))
            {
                // Đo kích thước chuỗi để tự động căn giữa màn hình
                SizeF textSize =
                    e.Graphics.MeasureString(
                        timerText,
                        timerFont
                    );

                float xPos =
                    (this.ClientSize.Width -
                    textSize.Width) / 2;

                // Đặt vị trí Y nằm ngay bên dưới dòng chữ
                // "Đang tìm kiếm..."
                float yPos =
                    (this.ClientSize.Height / 2) + 40;


                e.Graphics.DrawString(
                    timerText,
                    timerFont,
                    timerBrush,
                    xPos,
                    yPos
                );
            }


            // ==============================
            // WATERMARK BẢN QUYỀN
            // ==============================

            // Vẽ watermark tại đây nếu cần
        }


        // ==============================
        // 11. DỌN DẸP KHI ĐÓNG FORM
        // ==============================

        protected override void OnFormClosed(
            FormClosedEventArgs e)
        {
            // Dừng Timer chữ
            if (textTimer != null)
            {
                textTimer.Stop();
                textTimer.Dispose();
            }


            // Dừng Timer animation
            if (animTimer != null)
            {
                animTimer.Stop();
                animTimer.Dispose();
            }


            // Gọi xử lý đóng Form của lớp cha
            base.OnFormClosed(e);
        }
    }
}