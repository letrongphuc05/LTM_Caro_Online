
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using CaroOnline.Server.Network;

namespace CaroOnline.Server.Spectator
{
    internal class SpectatorManager
    {
        private readonly Dictionary<string, List<ClientConnection>> spectators
            = new Dictionary<string, List<ClientConnection>>();

        private readonly object locker = new object();

        // Them spectator vao phong
        public bool AddSpectator(string roomId, ClientConnection client)
        {
            if (string.IsNullOrWhiteSpace(roomId) || client == null)
                return false;

            lock (locker)
            {
                if (!spectators.ContainsKey(roomId))
                {
                    spectators[roomId] = new List<ClientConnection>();
                }

                if (spectators[roomId].Contains(client))
                    return false;

                spectators[roomId].Add(client);
                return true;
            }
        }

        // Xoa spectator khoi phong
        public bool RemoveSpectator(string roomId, ClientConnection client)
        {
            if (string.IsNullOrWhiteSpace(roomId) || client == null)
                return false;

            lock (locker)
            {
                if (!spectators.ContainsKey(roomId))
                    return false;

                bool removed = spectators[roomId].Remove(client);

                if (spectators[roomId].Count == 0)
                {
                    spectators.Remove(roomId);
                }

                return removed;
            }
        }

        // Kiem tra client có dang xem phong khong
        public bool IsSpectating(string roomId, ClientConnection client)
        {
            lock (locker)
            {
                return spectators.ContainsKey(roomId)
                    && spectators[roomId].Contains(client);
            }
        }

        // Lay so luong spectator
        public int GetSpectatorCount(string roomId)
        {
            lock (locker)
            {
                if (!spectators.ContainsKey(roomId))
                    return 0;

                return spectators[roomId].Count;
            }
        }

        // Lay danh sach spectator
        public List<ClientConnection> GetSpectators(string roomId)
        {
            lock (locker)
            {
                if (!spectators.ContainsKey(roomId))
                    return new List<ClientConnection>();

                return new List<ClientConnection>(
                    spectators[roomId]
                );
            }
        }

        // Gui message toi toan bo spectator
        public async Task BroadcastAsync(
            string roomId,
            string message)
        {
            List<ClientConnection> clients;

            lock (locker)
            {
                if (!spectators.ContainsKey(roomId))
                    return;

                clients = new List<ClientConnection>(
                    spectators[roomId]
                );
            }

            byte[] data = Encoding.UTF8.GetBytes(message);

            foreach (ClientConnection spectator in clients)
            {
                try
                {
                    TcpClient tcpClient = spectator.Client;

                    if (!tcpClient.Connected)
                    {
                        RemoveSpectator(roomId, spectator);
                        continue;
                    }

                    NetworkStream stream = tcpClient.GetStream();

                    await stream.WriteAsync(
                        data,
                        0,
                        data.Length
                    );
                }
                catch
                {
                    RemoveSpectator(roomId, spectator);
                }
            }
        }

        // Gui so luong spectator
        public async Task BroadcastSpectatorCountAsync(
            string roomId)
        {
            int count = GetSpectatorCount(roomId);

            string message =
                $"SPECTATOR_COUNT|{count}";

            await BroadcastAsync(
                roomId,
                message
            );
        }

        // Xoa toan bo spectator cua phong
        public void RemoveRoom(string roomId)
        {
            lock (locker)
            {
                if (spectators.ContainsKey(roomId))
                {
                    spectators.Remove(roomId);
                }
            }
        }

        // Xoa client khoi tat ca phong
        public void RemoveClient(ClientConnection client)
        {
            if (client == null)
                return;

            lock (locker)
            {
                foreach (string roomId in spectators.Keys.ToList())
                {
                    spectators[roomId].Remove(client);

                    if (spectators[roomId].Count == 0)
                    {
                        spectators.Remove(roomId);
                    }
                }
            }
        }
    }
}