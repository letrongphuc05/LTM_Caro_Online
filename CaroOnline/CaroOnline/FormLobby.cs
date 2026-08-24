using System;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using CaroOnline.Network;

namespace CaroOnline
{
    public partial class FormLobby : Form
    {
        public FormLobby()
        {
            InitializeComponent();

            lstOnlinePlayers.SelectionMode = SelectionMode.One;
            lstOnlinePlayers.Enabled = true;
            btnSendChallenge.Click += btnSendChallenge_Click;

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
                // Giữ lại bộ lọc chuỗi rỗng của bạn để tránh lỗi vệt sáng ở sảnh chờ
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
                // [NOTE QUAN TRỌNG - CHỐNG DEADLOCK TỪ MESSAGEBOX]:
                // Bắt buộc dùng BeginInvoke. Nếu dùng Invoke, luồng mạng sẽ bị đóng băng
                // mãi mãi cho tới khi người dùng bấm Yes/No.
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
                // [NOTE QUAN TRỌNG - CHỐNG DEADLOCK TỪ SHOWDIALOG]:
                // Bắt buộc dùng BeginInvoke. Nếu dùng Invoke, lệnh ShowDialog() sẽ chặn
                // luồng UI, luồng mạng cũng bị block theo khiến mạng bị sập ngang.
                this.BeginInvoke(new Action(() => HandleMatchStart(roomId, opponent, role)));
                return;
            }

            bool isPlayer1 = (role == 1);
            FormMain board = new FormMain(isPlayer1, opponent);
            this.Hide();
            board.ShowDialog();

            // [NOTE QUAN TRỌNG - TẨY RỬA CODE]:
            // Lệnh SetMainForm(null) đã bị gỡ bỏ do SocketManager giờ đã hoàn toàn độc lập,
            // không còn bị phụ thuộc cứng vào bất kỳ Giao diện nào nữa.
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
                SocketManager.Instance.JoinRoomAsSpectator(matchId);

                FormMain watchBoard = new FormMain(false, "Khán giả");
                watchBoard.Tag = "Spectator";

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
        private void btnWatchMatch_Click_1(object sender, EventArgs e) { }
    }
}