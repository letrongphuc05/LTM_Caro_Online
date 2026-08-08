using System.Net;
using System.Net.Sockets;
using System.Drawing;
namespace CaroServerAdmin
{

    public partial class FrmAdminDashboard : Form
    {
        private TcpListener? _listener;
        private bool _serverRunning = false;

        public FrmAdminDashboard()
        {
            InitializeComponent();
            AddLog("Admin Dashboard initialized.");
        }
        private void AddLog(string message)
        {
            string time = DateTime.Now.ToString("HH:mm:ss");

            lstServerLog.Items.Add($"[{time}] {message}");

            if (lstServerLog.Items.Count > 0)
            {
                lstServerLog.TopIndex = lstServerLog.Items.Count - 1;
            }
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {
            //Code START 
        }

        private void btnStartServer_Click(object sender, EventArgs e)
        {
            try
            {
                if (!IPAddress.TryParse(txtIP.Text.Trim(), out IPAddress? ipAddress))
                {
                    MessageBox.Show(
                        "Dia chi IP khong hop le.",
                        "Loi",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);

                    return;
                }

                if (!int.TryParse(txtPort.Text.Trim(), out int port) ||
                    port < 1 || port > 65535)
                {
                    MessageBox.Show(
                        "Port phai nam trong khoang 1 - 65535.",
                        "Loi",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);

                    return;
                }

                _listener = new TcpListener(ipAddress, port);

                _listener.Start();

                _serverRunning = true;

                lblServerStatus.Text = "LISTENING";
                lblServerStatus.ForeColor = Color.Green;

                btnStartServer.Enabled = false;
                btnStopServer.Enabled = true;

                txtIP.Enabled = false;
                txtPort.Enabled = false;
                AddLog($"Server started at {ipAddress}:{port}");
            }
            catch (SocketException ex)
            {
                lblServerStatus.Text = "ERROR";
                lblServerStatus.ForeColor = Color.Red;

                AddLog("START ERROR: " + ex.Message);

                MessageBox.Show(
                    "Khong the khoi dong Server.\n" + ex.Message,
                    "Socket Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                lblServerStatus.Text = "ERROR";
                lblServerStatus.ForeColor = Color.Red;

                AddLog("ERROR: " + ex.Message);

                MessageBox.Show(
                    ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void btnStopServer_Click(object sender, EventArgs e)
        {
            try
            {
                if (_listener != null)
                {
                    _listener.Stop();
                    _listener = null;
                }

                _serverRunning = false;

                lblServerStatus.Text = "STOPPED";
                lblServerStatus.ForeColor = Color.Red;

                btnStartServer.Enabled = true;
                btnStopServer.Enabled = false;

                txtIP.Enabled = true;
                txtPort.Enabled = true;

                AddLog("Server stopped.");
            }
            catch (Exception ex)
            {
                lblServerStatus.Text = "ERROR";
                lblServerStatus.ForeColor = Color.Red;

                AddLog("STOP ERROR: " + ex.Message);

                MessageBox.Show(
                    "Khong the dung Server.\n" + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
