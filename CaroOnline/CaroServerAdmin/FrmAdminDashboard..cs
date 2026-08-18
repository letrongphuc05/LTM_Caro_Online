using System.Net;
using System.Net.Sockets;
using System.Drawing;
using CaroOnline.Server.Network;

namespace CaroServerAdmin
{

    public partial class FrmAdminDashboard : Form
    {
        private ServerController _serverController;
        private TcpClient? _testClient;

        public FrmAdminDashboard()
        {
            InitializeComponent();

            _serverController = new ServerController();

            _serverController.ClientConnected +=
                ServerController_ClientConnected;

            _serverController.ClientDisconnected +=
                ServerController_ClientDisconnected;
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

        private void ServerController_ClientConnected(
             string clientAddress)
        {
            if (InvokeRequired)
            {
                Invoke(() =>
                    ServerController_ClientConnected(
                        clientAddress));

                return;
            }

            AddLog($"Client connected: {clientAddress}");
        }

        private void ServerController_ClientDisconnected(
            string clientAddress)
        {
            if (InvokeRequired)
            {
                Invoke(() =>
                    ServerController_ClientDisconnected(
                        clientAddress));

                return;
            }

            AddLog($"Client disconnected: {clientAddress}");
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

                _serverController.Start(ipAddress.ToString(), port);

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
                if (_testClient != null)
                {
                    _testClient.Close();
                    _testClient = null;
                }
                _serverController.Stop();

                lblServerStatus.Text = "STOPPED";
                lblServerStatus.ForeColor = Color.Red;

                btnStartServer.Enabled = true;
                btnStopServer.Enabled = false;

                btnConnectTestClient.Enabled = true;
                btnDisconnectTestClient.Enabled = false;

                lblTestClientStatus.Text = "DISCONNECTED";
                lblTestClientStatus.ForeColor = Color.Red;

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

        private void timerStatistics_Tick(object sender, EventArgs e)
        {
            lblClientsOnline.Text = _serverController.ClientCount.ToString();
            lblTotalConnections.Text = _serverController.TotalConnections.ToString();
        }

        private async void btnConnectTestClient_Click(object sender, EventArgs e)
        {
            try
            {
                if (!_serverController.IsRunning)
                {
                    MessageBox.Show(
                        "Hay START Server truoc.",
                        "Thong bao",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    return;
                }

                string ip = txtIP.Text.Trim();
                int port = int.Parse(txtPort.Text.Trim());

                _testClient = new TcpClient();

                await _testClient.ConnectAsync(ip, port);

                btnConnectTestClient.Enabled = false;
                btnDisconnectTestClient.Enabled = true;
                //Nối trạng thái này với code CONNECT
                lblTestClientStatus.Text = "CONNECTED";
                lblTestClientStatus.ForeColor = Color.Green;

                AddLog("Test Client connected.");
            }
            catch (Exception ex)
            {
                AddLog("TEST CONNECT ERROR: " + ex.Message);

                MessageBox.Show(
                    "Khong the ket noi Test Client.\n" + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void btnDisconnectTestClient_Click(object sender, EventArgs e)
        {
            try
            {
                if (_testClient != null)
                {
                    _testClient.Close();
                    _testClient = null;
                }

                btnConnectTestClient.Enabled = true;
                btnDisconnectTestClient.Enabled = false;
                //Nối trạng thái với DISCONNECT
                lblTestClientStatus.Text = "DISCONNECTED";
                lblTestClientStatus.ForeColor = Color.Red;

                AddLog("Test Client disconnected.");
            }
            catch (Exception ex)
            {
                AddLog("TEST DISCONNECT ERROR: " + ex.Message);
            }
        }

        private void FrmAdminDashboard_Load(object sender, EventArgs e)
        {
            lblServerStatus.Text = "STOPPED";
            lblServerStatus.ForeColor = Color.Red;
        }

        private void btnClearLog_Click(object sender, EventArgs e)
        {
            lstServerLog.Items.Clear();
            AddLog("Log cleared.");
        }

    }
}
