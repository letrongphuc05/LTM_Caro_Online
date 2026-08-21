using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace CaroOnline
{
    public partial class FormConnect : Form
    {
        public FormConnect()
        {
            InitializeComponent();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                // 1. Lấy thông tin người dùng nhập
                string ip = txtIP.Text.Trim();
                int port = int.Parse(txtPort.Text.Trim());

                // 2. Khởi tạo Sảnh chờ (Phòng ghép trận) và truyền IP, Port sang cho nó
                FormLobby lobby = new FormLobby(ip, port);

                // 3. Ẩn form này đi và bật sảnh chờ lên
                this.Hide();
                lobby.ShowDialog();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Vui lòng kiểm tra lại IP và Port!\n" + ex.Message, "Lỗi");
            }
        }

        private void txtIP_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void FormConnect_Load(object sender, EventArgs e)
        {

        }

        private void picGuide_Click(object sender, EventArgs e)
        {
           
            // 1. Tạo một cửa sổ (Form) mới làm Bảng Hướng Dẫn
            Form guideForm = new Form();
            guideForm.Text = "Bí kíp Tân thủ | Lê Doãn Đạt - 038206000230";
            guideForm.Size = new Size(550, 450);
            guideForm.StartPosition = FormStartPosition.CenterScreen;
            guideForm.FormBorderStyle = FormBorderStyle.FixedDialog;
            guideForm.MaximizeBox = false;

            // Đồng bộ màu nền tối giống với Form Khởi động
            guideForm.BackColor = Color.FromArgb(30, 30, 47);
            guideForm.ForeColor = Color.White;

            // 2. Tạo nội dung văn bản cho bảng
            Label lblContent = new Label();
            lblContent.Dock = DockStyle.Fill;
            lblContent.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            lblContent.Padding = new Padding(20);

            // Soạn thảo nội dung luật chơi và hướng dẫn kết nối
            lblContent.Text = "BÍ KÍP TÂN THỦ - CARO ONLINE\n" +
                              "                                                \n\n" +
                              "🎮 1. HƯỚNG DẪN GHÉP TRẬN (MẠNG P2P):\n" +
                              "  • Hai người chơi cần thống nhất chung 1 số Port (VD: 8080).\n" +
                              "  • Chủ phòng: Nhập IP của máy mình (VD: 127.0.0.1) -> Bấm Kết nối trước.\n" +
                              "  • Khách: Nhập IP của Chủ phòng và Port tương ứng -> Bấm Kết nối sau.\n\n" +
                              "🏆 2. LUẬT CHƠI CARO (CÓ CHẶN 2 ĐẦU):\n" +
                              "  • Xếp đủ 5 quân cờ liên tiếp (Ngang, Dọc, Chéo) để giành chiến thắng.\n" +
                              "  • LƯU Ý QUAN TRỌNG: Nếu chuỗi 5 quân của bạn bị đối thủ chặn \n" +
                              "    kín ở cả 2 đầu, đường cờ đó KHÔNG được tính là hợp lệ!\n\n" +
                              "💡 3. TÍNH NĂNG ĐẶC BIỆT:\n" +
                              "  • Hệ thống có cơ chế chống văng (Crash). Nếu rớt mạng hoặc đối thủ\n" +
                              "    thoát game ngang, bạn sẽ nhận được thông báo lập tức.\n\n" +
                              "                                                 \n" +
                              "Chúc bạn có những trận đấu đỉnh cao!";

            // 3. Gắn chữ vào bảng và hiển thị lên màn hình
            guideForm.Controls.Add(lblContent);
            guideForm.ShowDialog();
        }
    }
    }

