using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace CaroOnline
{
    public partial class FormConnect : Form
    {
        public FormConnect()
        {
            InitializeComponent();


            // Gọi hàm đổi giao diện mà bạn đã viết ở dưới
            ApplyCyberpunkUI();

            // --- CHỈ THÊM ĐÚNG ĐOẠN TẠO NÚT CUỐN SÁCH NÀY ---
            Button btnHelp = new Button();
            btnHelp.Size = new Size(40, 40);
            btnHelp.Location = new Point(12, this.ClientSize.Height - 50);

            // Đảm bảo đường dẫn này khớp với máy của bạn
            btnHelp.BackgroundImage = Properties.Resources.book;
            btnHelp.BackgroundImageLayout = ImageLayout.Zoom;

            btnHelp.FlatStyle = FlatStyle.Flat;
            btnHelp.FlatAppearance.BorderSize = 0;
            btnHelp.FlatAppearance.MouseDownBackColor = Color.Transparent;
            btnHelp.FlatAppearance.MouseOverBackColor = Color.Transparent;
            btnHelp.BackColor = Color.Transparent;
            btnHelp.Cursor = Cursors.Hand;

            btnHelp.Click += (s, e) =>
            {
                string rules = "🎮 HƯỚNG DẪN CHƠI CARO ONLINE CHUẨN MỰC\n\n" +
                               "1. CÁCH CHƠI CƠ BẢN:\n" +
                               "   • Hai người chơi luân phiên đánh dấu X hoặc O.\n" +
                               "   • Trò chơi kết thúc khi có người chiến thắng hoặc hòa.\n\n" +
                               "2. ĐIỀU KIỆN CHIẾN THẮNG:\n" +
                               "   • Xếp được 5 quân liên tiếp theo ngang, dọc, hoặc chéo.\n\n" +
                               "3. MẸO CHIẾN THUẬT:\n" +
                               "   • Kiểm soát trung tâm và tạo thế gọng kìm.";

                MessageBox.Show(rules, "Cẩm nang Caro Online", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            this.Controls.Add(btnHelp);
            btnHelp.BringToFront();
        }
        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            string ip = txtIP.Text.Trim();
            string portText = txtPort.Text.Trim();
            string username = txtUsername.Text.Trim();

            if (string.IsNullOrEmpty(username))
            {
                MessageBox.Show("Vui lòng nhập tên người chơi!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                int port = int.Parse(portText);


                if (CaroOnline.Network.SocketManager.Instance.Connect(ip, port, username))
                {
                    FormLobby lobby = new FormLobby();
                    this.Hide();
                    lobby.ShowDialog();
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết nối: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void txtIP_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        private void ApplyCyberpunkUI()


        {
            this.Text = "Caro Online";


            this.BackgroundImageLayout = ImageLayout.Stretch;

            foreach (Control ctrl in this.Controls)
            {
                if (ctrl is Label lbl)
                {
                    lbl.BackColor = Color.Transparent;

                    if (lbl.Text.ToUpper().Contains("GAME CARO ONLINE"))
                    {
                        lbl.ForeColor = Color.DeepSkyBlue;
                        lbl.Font = new Font("Impact", 26, FontStyle.Regular);
                    }
                    else
                    {
                        lbl.ForeColor = Color.White;
                        lbl.Font = new Font("Segoe UI", 10, FontStyle.Bold);
                    }
                }
                else if (ctrl is Button btn)
                {

                    btn.FlatStyle = FlatStyle.Flat;


                    btn.BackColor = Color.Transparent;
                    btn.UseVisualStyleBackColor = false;


                    btn.FlatAppearance.BorderColor = Color.White;
                    btn.FlatAppearance.BorderSize = 1;


                    btn.ForeColor = Color.White;
                    btn.Font = new Font("Segoe UI", 11, FontStyle.Bold);
                    btn.Cursor = Cursors.Hand;


                    btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(40, 255, 255, 255);
                    btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(80, 255, 255, 255);
                }
                else if (ctrl is ComboBox || ctrl is TextBox)
                {

                    ctrl.BackColor = Color.White;
                    ctrl.ForeColor = Color.Black;
                    ctrl.Font = new Font("Segoe UI", 10, FontStyle.Regular);
                }
            }
        }

        private void txtUsername_TextChanged(object sender, EventArgs e)
        {

        }
    }
}

