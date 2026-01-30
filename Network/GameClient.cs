using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using Lifeblood.Engine;
using Lifeblood.Game;

namespace Lifeblood.Network
{
    public class GameClient
    {
        private UdpClient udpClient;
        private IPEndPoint serverEndPoint;
        private bool isConnected;
        private Thread receiveThread;
        
        // Local state
        public int MyPlayerID { get; private set; }
        public Dictionary<int, PlayerStatePacket> OtherPlayers { get; private set; }
        
        // Stats
        public float Ping { get; private set; }

        public GameClient()
        {
            OtherPlayers = new Dictionary<int, PlayerStatePacket>();
        }

        public void Connect(string ip, int port = NetworkProtocol.DefaultPort)
        {
            try
            {
                udpClient = new UdpClient();
                serverEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
                udpClient.Connect(serverEndPoint);
                
                isConnected = true;
                receiveThread = new Thread(ReceiveLoop);
                receiveThread.IsBackground = true;
                receiveThread.Start();
                
                // Send initial hello packet
                SendInput(new PlayerInputPacket()); // Seq 0, just to say hi
                
                Console.WriteLine("[CLIENT] Connecting to " + ip);
            }
            catch (Exception ex)
            {
                Console.WriteLine("[CLIENT] Connection failed: " + ex.Message);
            }
        }

        public void Disconnect()
        {
            isConnected = false;
            if (udpClient != null)
            {
                udpClient.Close();
                udpClient = null;
            }
        }

        public void SendInput(PlayerInputPacket input)
        {
            if (!isConnected) return;
            
            try
            {
                byte[] data = NetworkProtocol.SerializeInput(input);
                udpClient.Send(data, data.Length);
            }
            catch { }
        }

        private void ReceiveLoop()
        {
            while (isConnected && udpClient != null)
            {
                try
                {
                    IPEndPoint remote = new IPEndPoint(IPAddress.Any, 0);
                    byte[] data = udpClient.Receive(ref remote);
                    ProcessPacket(data);
                }
                catch (SocketException)
                {
                    // Disconnected or error
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[CLIENT] Receive error: " + ex.Message);
                }
            }
        }

        private void ProcessPacket(byte[] data)
        {
            if (data.Length < 1) return;

            PacketType type = (PacketType)data[0];
            switch (type)
            {
                case PacketType.GameState:
                    var state = NetworkProtocol.DeserializeState(data);
                    UpdateWorldState(state);
                    break;
            }
        }

        private void UpdateWorldState(GameStatePacket state)
        {
            lock (OtherPlayers)
            {
                OtherPlayers.Clear();
                if (state.Players != null)
                {
                    foreach (var p in state.Players)
                    {
                        // Identify myself?
                        // Ideally we need a handshake to know our ID. 
                        // For now we store everyone.
                        OtherPlayers[p.PlayerID] = p;
                        
                        // Heuristic to find self if unknown (not safe but works for test)
                        // In real code, Server sends "Your ID is X" on connect.
                    }
                }
            }
        }
        
        public PlayerStatePacket? GetPlayerState(int id)
        {
            lock(OtherPlayers)
            {
                if (OtherPlayers.ContainsKey(id)) return OtherPlayers[id];
                return null;
            }
        }
    }
}
