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
            SocketManager.Instance.OnUpdateOnlineList += UpdateOnlineList;
            SocketManager.Instance.OnReceiveChallenge += HandleIncomingChallenge;
            SocketManager.Instance.OnUpdateRoomList += UpdateMatchRooms;
        }

        private void UpdateOnlineList(string[] players)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => UpdateOnlineList(players)));
                return;
            }

            lstOnlinePlayers.Items.Clear();
<<<<<<< Updated upstream
            lstOnlinePlayers.Items.AddRange(players);
=======

            foreach (string player in players)
            {
                if (!string.IsNullOrWhiteSpace(player))
                {
                    lstOnlinePlayers.Items.Add(player);
                }
            }
>>>>>>> Stashed changes
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
<<<<<<< Updated upstream
                this.Invoke(new Action(() => HandleIncomingChallenge(challengerName)));
=======
                this.BeginInvoke(new Action(() => HandleIncomingChallenge(challengerName)));
>>>>>>> Stashed changes
                return;
            }

            DialogResult response = MessageBox.Show(
                $"{challengerName} muốn thách đấu với bạn. Bạn đồng ý không?",
                "Lời mời", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (response == DialogResult.Yes)
            {
                NetworkManager.Instance.AcceptChallenge(challengerName);
                FormMain board = new FormMain();
                this.Hide();
                board.ShowDialog();
                this.Show();
            }
            else
            {
                NetworkManager.Instance.DeclineChallenge(challengerName);
            }
        }

<<<<<<< Updated upstream
=======
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

>>>>>>> Stashed changes
        private void UpdateMatchRooms(string[] ongoingMatches)
        {
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


                FormMain watchBoard = new FormMain();


                watchBoard.Tag = "Spectator";

                this.Hide();
                watchBoard.ShowDialog();
                this.Show();
                this.Hide();
                watchBoard.ShowDialog();
                this.Show();
            }
            else
            {
                MessageBox.Show("Chọn một trận đấu để xem!", "Nhắc nhở");
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void lstOnlinePlayers_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void btnWatchMatch_Click_1(object sender, EventArgs e)
        {

        }
    }

}
