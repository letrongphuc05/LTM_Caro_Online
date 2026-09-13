using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CaroOnline.Network;

namespace CaroOnline
{
    public partial class FormLobby : Form
    {
        // Biến Singleton tĩnh để dễ dàng truy cập FormLobby từ các Form khác (VD: gọi hàm ShowReconnectPrompt từ FormMain)
        public static FormLobby Instance;

        // Các biến phục vụ vẽ hiệu ứng hoạt ảnh (animation) background ở màn hình sảnh
        private System.Windows.Forms.Timer animTimer;
        private List<SymbolParticle> particles;
        private Random rand = new Random();
        private float radarRadius = 0;

        // Các biến UI phục vụ chức năng hỏi "Kết nối lại" hoặc "Đầu hàng"
        private Panel pnlReconnect;
        private Label lblReconnectMsg;
        private Button btnReconnect;
        private Button btnSurrender;
        private FormMain pendingMatch; // Lưu tham chiếu đến trận đấu đang bị ẩn

        class SymbolParticle
        {
            public float X, Y, SpeedY;
            public string Text;
            public Font Font;
            public Color Color;
        }

        public FormLobby()
        {
            Instance = this;
            InitializeComponent();
            this.DoubleBuffered = true; // Chống giật lag (flicker) khi vẽ animation

            ApplyCyberpunkUI();
            InitAnimation();
            SetupReconnectUI(); // Khởi tạo trước Panel Kết nối lại (mặc định ẩn)

            lstOnlinePlayers.SelectionMode = SelectionMode.One;
            lstOnlinePlayers.Enabled = true;
            lstOnlinePlayers.Cursor = Cursors.Hand;

            btnSendChallenge.Click += btnSendChallenge_Click;
            btnWatchMatch.Click += btnWatchMatch_Click;

            lstOnlinePlayers.MouseEnter += (s, e) => lstOnlinePlayers.Cursor = Cursors.Hand;
            lstOnlinePlayers.MouseLeave += (s, e) => lstOnlinePlayers.Cursor = Cursors.Default;

            // Đăng ký các sự kiện nhận dữ liệu từ Server thông qua SocketManager
            SocketManager.Instance.OnUpdateOnlineList += UpdateOnlineList;
            SocketManager.Instance.OnReceiveChallenge += HandleIncomingChallenge;
            SocketManager.Instance.OnUpdateRoomList += UpdateMatchRooms;
            SocketManager.Instance.OnMatchStart += HandleMatchStart;

            UpdateOnlineList(SocketManager.Instance.LastOnlineList);
            UpdateMatchRooms(SocketManager.Instance.LastRoomList);
        }

        // Khởi tạo giao diện (Panel) xuất hiện khi người chơi bấm [X] tạm thoát khỏi trận đang đánh
        private void SetupReconnectUI()
        {
            pnlReconnect = new Panel();
            pnlReconnect.Size = new Size(400, 200);
            pnlReconnect.BackColor = Color.FromArgb(40, 40, 60);
            pnlReconnect.BorderStyle = BorderStyle.FixedSingle;
            pnlReconnect.Visible = false;
            pnlReconnect.Anchor = AnchorStyles.None;

            lblReconnectMsg = new Label();
            lblReconnectMsg.Text = "Bạn đang trong một trận đấu!\nBạn muốn kết nối lại hay đầu hàng?";
            lblReconnectMsg.ForeColor = Color.White;
            lblReconnectMsg.Font = new Font("Arial", 12, FontStyle.Bold);
            lblReconnectMsg.TextAlign = ContentAlignment.MiddleCenter;
            lblReconnectMsg.Dock = DockStyle.Top;
            lblReconnectMsg.Height = 80;

            btnReconnect = new Button();
            btnReconnect.Text = "Kết nối lại";
            btnReconnect.Size = new Size(120, 40);
            btnReconnect.Location = new Point(50, 100);
            btnReconnect.BackColor = Color.SeaGreen;
            btnReconnect.ForeColor = Color.White;
            btnReconnect.FlatStyle = FlatStyle.Flat;
            btnReconnect.Cursor = Cursors.Hand;
            btnReconnect.Click += (s, e) => {
                pnlReconnect.Visible = false;
                this.Hide();
                if (pendingMatch != null && !pendingMatch.IsDisposed)
                {
                    // Gửi tín hiệu đã kết nối lại và mở lại màn hình trận đấu
                    pendingMatch.ReconnectFromLobby();
                    pendingMatch.ShowDialog();
                }
                this.Show();
            };

            btnSurrender = new Button();
            btnSurrender.Text = "Đầu hàng (Không)";
            btnSurrender.Size = new Size(150, 40);
            btnSurrender.Location = new Point(200, 100);
            btnSurrender.BackColor = Color.IndianRed;
            btnSurrender.ForeColor = Color.White;
            btnSurrender.FlatStyle = FlatStyle.Flat;
            btnSurrender.Cursor = Cursors.Hand;
            btnSurrender.Click += (s, e) => {
                pnlReconnect.Visible = false;
                if (pendingMatch != null && !pendingMatch.IsDisposed)
                {
                    // Gửi tín hiệu xử thua và hủy trận đấu
                    pendingMatch.SurrenderFromLobby();
                }
                pendingMatch = null;
            };

            pnlReconnect.Controls.Add(lblReconnectMsg);
            pnlReconnect.Controls.Add(btnReconnect);
            pnlReconnect.Controls.Add(btnSurrender);
            this.Controls.Add(pnlReconnect);
            pnlReconnect.BringToFront();
        }

        // Hàm được gọi từ FormMain để kích hoạt Panel hỏi Reconnect ra giữa màn hình sảnh
        public void ShowReconnectPrompt(FormMain matchForm)
        {
            this.Show();
            this.pendingMatch = matchForm;

            pnlReconnect.Location = new Point((this.ClientSize.Width - pnlReconnect.Width) / 2, (this.ClientSize.Height - pnlReconnect.Height) / 2);
            pnlReconnect.Visible = true;
            pnlReconnect.BringToFront();
        }

        // Tự động đóng Panel Reconnect nếu đối phương đã xử thua mình do không chờ đợi nữa
        public void HideReconnectPrompt()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => HideReconnectPrompt()));
                return;
            }
            pnlReconnect.Visible = false;
            pendingMatch = null;
        }

        private void UpdateOnlineList(string[] players)
        {
            if (players == null) return;
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => UpdateOnlineList(players)));
                return;
            }

            lstOnlinePlayers.Items.Clear();
            foreach (string player in players)
            {
                if (!string.IsNullOrWhiteSpace(player))
                {
                    lstOnlinePlayers.Items.Add(player);
                }
            }
        }

        private void btnSendChallenge_Click(object sender, EventArgs e)
        {
            if (lstOnlinePlayers.SelectedItem != null)
            {
                string targetPlayer = lstOnlinePlayers.SelectedItem.ToString();
                SocketManager.Instance.SendChallenge(targetPlayer);
                MessageBox.Show($"Đã gửi lời mời thách đấu tới {targetPlayer}.", "Thông báo");
            }
            else
            {
                MessageBox.Show("Hãy chọn một người chơi!", "Nhắc nhở");
            }
        }

        private void HandleIncomingChallenge(string challengerName)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => HandleIncomingChallenge(challengerName)));
                return;
            }

            DialogResult response = MessageBox.Show(
                $"{challengerName} muốn thách đấu với bạn. Bạn đồng ý không?",
                "Lời mời", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (response == DialogResult.Yes)
            {
                NetworkManager.Instance.AcceptChallenge(challengerName);
            }
            else
            {
                NetworkManager.Instance.DeclineChallenge(challengerName);
            }
        }

        // Xử lý khi 2 người ghép trận thành công (hoặc vào trận với tư cách khán giả)
        private void HandleMatchStart(string roomId, string opponent, int role)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => HandleMatchStart(roomId, opponent, role)));
                return;
            }

            try
            {
                bool isPlayer1 = (role == 1);
                FormMain board = new FormMain(isPlayer1, opponent);
                board.Tag = roomId; // Lưu roomId vào Tag để có thể LeaveRoom sau này

                this.Hide();

                // ShowDialog sẽ chặn luồng ở đây cho đến khi FormMain đóng lại
                board.ShowDialog();

                // Trận đấu đóng hẳn -> Mở lại Sảnh chờ
                this.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể khởi tạo bàn cờ!\nChi tiết lỗi: " + ex.Message, "Lỗi Form", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Show();
            }
        }

        private void UpdateMatchRooms(string[] ongoingMatches)
        {
            if (ongoingMatches == null) return;
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => UpdateMatchRooms(ongoingMatches)));
                return;
            }
            lstMatchRooms.Items.Clear();

            // Lọc bỏ các trận đã kết thúc (ẩn khỏi Lobby) bằng cách loại các chuỗi chứa từ khóa kết quả
            var uniqueMatches = ongoingMatches
                .Where(m => !string.IsNullOrWhiteSpace(m) &&
                            !m.ToLower().Contains("end") &&
                            !m.ToLower().Contains("kết thúc") &&
                            !m.ToLower().Contains("win") &&
                            !m.ToLower().Contains("lose") &&
                            !m.ToLower().Contains("thắng") &&
                            !m.ToLower().Contains("thua"))
                .Distinct()
                .ToList();

            foreach (string match in uniqueMatches)
            {
                lstMatchRooms.Items.Add(match);
            }
        }

        private void btnWatchMatch_Click(object sender, EventArgs e)
        {
            if (lstMatchRooms.SelectedItem != null)
            {
                try
                {
                    string matchId = lstMatchRooms.SelectedItem.ToString();
                    FormMain watchBoard = new FormMain(false, "Khán giả");
                    watchBoard.Tag = matchId;
                    SocketManager.Instance.JoinRoomAsSpectator(matchId);

                    this.Hide();
                    watchBoard.ShowDialog();
                    this.Show();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi mở phòng xem:\n" + ex.Message, "Lỗi Form", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.Show();
                }
            }
            else
            {
                MessageBox.Show("Chọn một trận đấu để xem!", "Nhắc nhở");
            }
        }

        private void button1_Click(object sender, EventArgs e) { }
        private void lstOnlinePlayers_SelectedIndexChanged(object sender, EventArgs e) { }
        private void label2_Click(object sender, EventArgs e) { }

        // Tùy chỉnh màu sắc, giao diện UI mang phong cách tối (Cyberpunk style)
        private void ApplyCyberpunkUI()
        {
            this.BackColor = Color.FromArgb(30, 30, 47);
            this.Text = "Sảnh chờ";

            foreach (Control ctrl in this.Controls)
            {
                if (ctrl is ListBox lst)
                {
                    lst.BackColor = Color.FromArgb(40, 40, 60);
                    lst.ForeColor = Color.White;
                    lst.BorderStyle = BorderStyle.None;
                    lst.Font = new Font("Segoe UI", 10, FontStyle.Regular);
                }
                else if (ctrl is Button btn)
                {
                    btn.FlatStyle = FlatStyle.Flat;
                    btn.FlatAppearance.BorderSize = 0;
                    btn.BackColor = Color.FromArgb(70, 130, 180);
                    btn.ForeColor = Color.White;
                    btn.Cursor = Cursors.Hand;
                    btn.MouseEnter += (s, e) => btn.BackColor = Color.FromArgb(100, 149, 237);
                    btn.MouseLeave += (s, e) => btn.BackColor = Color.FromArgb(70, 130, 180);
                }
                else if (ctrl is Label lbl)
                {
                    lbl.ForeColor = Color.Cyan;
                    lbl.BackColor = Color.Transparent;
                }
            }
        }

        // Tạo logic sinh hạt O/X bay bay trên nền cho màn hình sinh động
        private void InitAnimation()
        {
            particles = new List<SymbolParticle>();
            for (int i = 0; i < 20; i++)
            {
                bool isX = rand.Next(2) == 0;
                particles.Add(new SymbolParticle
                {
                    X = rand.Next(0, 800),
                    Y = rand.Next(-100, 600),
                    SpeedY = (float)(rand.NextDouble() * 1.5 + 0.5),
                    Text = isX ? "X" : "O",
                    Font = new Font("Comic Sans MS", rand.Next(15, 25), FontStyle.Bold),
                    Color = isX ? Color.FromArgb(60, 255, 50, 50) : Color.FromArgb(60, 50, 200, 255)
                });
            }

            animTimer = new System.Windows.Forms.Timer { Interval = 40 };
            animTimer.Tick += (s, e) =>
            {
                foreach (var p in particles)
                {
                    p.Y -= p.SpeedY;
                    if (p.Y + 50 < 0)
                    {
                        p.Y = this.ClientSize.Height;
                        p.X = rand.Next(0, this.ClientSize.Width);
                    }
                }
                radarRadius += 3;
                if (radarRadius > this.ClientSize.Width / 1.5f) radarRadius = 0;
                this.Invalidate(); // Yêu cầu vẽ lại Frame
            };
            animTimer.Start();
        }

        // Hàm vẽ (Render) đè lên Form các hạt và radar mờ
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            int centerX = this.ClientSize.Width / 2;
            int centerY = this.ClientSize.Height / 2;
            int alpha = (int)(255 * (1 - radarRadius / (this.ClientSize.Width / 1.5f)));
            if (alpha > 0 && alpha <= 255)
            {
                using (Pen radarPen = new Pen(Color.FromArgb(alpha, 0, 255, 255), 2))
                {
                    e.Graphics.DrawEllipse(radarPen, centerX - radarRadius, centerY - radarRadius, radarRadius * 2, radarRadius * 2);
                }
            }

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