using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using CaroOnline.Network;

namespace CaroOnline
{
    public partial class FormLobby : Form
    {
        private System.Windows.Forms.Timer animTimer;
        private List<SymbolParticle> particles;
        private Random rand = new Random();
        private float radarRadius = 0;

        class SymbolParticle
        {
            public float X, Y, SpeedY;
            public string Text;
            public Font Font;
            public Color Color;
        }


        public FormLobby()
        {
            InitializeComponent();
            this.DoubleBuffered = true; 


            ApplyCyberpunkUI();
            InitAnimation();


            lstOnlinePlayers.SelectionMode = SelectionMode.One;
            lstOnlinePlayers.Enabled = true;
            lstOnlinePlayers.Cursor = Cursors.Hand;

            btnSendChallenge.Click += btnSendChallenge_Click;
            btnWatchMatch.Click += btnWatchMatch_Click;



            lstOnlinePlayers.MouseEnter += (s, e) => lstOnlinePlayers.Cursor = Cursors.Hand;
            lstOnlinePlayers.MouseLeave += (s, e) => lstOnlinePlayers.Cursor = Cursors.Default;

            SocketManager.Instance.OnUpdateOnlineList += UpdateOnlineList;
            SocketManager.Instance.OnReceiveChallenge += HandleIncomingChallenge;
            SocketManager.Instance.OnUpdateRoomList += UpdateMatchRooms;
            SocketManager.Instance.OnMatchStart += HandleMatchStart;

            UpdateOnlineList(SocketManager.Instance.LastOnlineList);
            UpdateMatchRooms(SocketManager.Instance.LastRoomList);
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

        private void HandleMatchStart(string roomId, string opponent, int role)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => HandleMatchStart(roomId, opponent, role)));
                return;
            }

            bool isPlayer1 = (role == 1);
            FormMain board = new FormMain(isPlayer1, opponent);
            this.Hide();
            board.ShowDialog();
            this.Show();
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

            foreach (string match in ongoingMatches)
            {
                if (!string.IsNullOrWhiteSpace(match))
                {
                    lstMatchRooms.Items.Add(match);
                }
            }
        }

        private void btnWatchMatch_Click(object sender, EventArgs e)
        {
            if (lstMatchRooms.SelectedItem != null)
            {
                string matchId = lstMatchRooms.SelectedItem.ToString();
                FormMain watchBoard = new FormMain(false, "Khán giả");
                watchBoard.Tag = matchId;
                SocketManager.Instance.JoinRoomAsSpectator(matchId);
                this.Hide();
                watchBoard.ShowDialog();
                this.Show();
            }
            else
            {
                MessageBox.Show("Chọn một trận đấu để xem!", "Nhắc nhở");
            }
        }

        private void button1_Click(object sender, EventArgs e) { }
        private void lstOnlinePlayers_SelectedIndexChanged(object sender, EventArgs e) { }
        private void label2_Click(object sender, EventArgs e) { }


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
                this.Invalidate();
            };
            animTimer.Start();
        }

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