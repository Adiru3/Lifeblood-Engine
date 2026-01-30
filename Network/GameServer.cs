using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Lifeblood.Engine;
using Lifeblood.Game;

namespace Lifeblood.Network
{
    public class ServerPlayer
    {
        public IPEndPoint EndPoint;
        public int PlayerID;
        public Physics3D Physics;
        public Player Data;
        public float Yaw;
        public float Pitch;
        public uint LastSequence;
    }

    public class GameServer
    {
        private UdpClient udpListener;
        private bool isRunning;
        private Thread networkThread;
        private Dictionary<string, ServerPlayer> players = new Dictionary<string, ServerPlayer>();
        private int nextPlayerId = 1;
        private uint serverTick = 0;

        public bool IsRunning { get { return isRunning; } }

        public void Start(int port = NetworkProtocol.DefaultPort)
        {
            try
            {
                udpListener = new UdpClient(port);
                isRunning = true;
                
                networkThread = new Thread(NetworkLoop);
                networkThread.IsBackground = true;
                networkThread.Start();
                
                Console.WriteLine(string.Format("[SERVER] Started on port {0}", port));
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("[SERVER] Failed to start: {0}", ex.Message));
            }
        }

        public void Stop()
        {
            isRunning = false;
            if (udpListener != null)
            {
                udpListener.Close();
                udpListener = null;
            }
            Console.WriteLine("[SERVER] Stopped");
        }

        public void Update(float deltaTime)
        {
            serverTick++;

            lock (players)
            {
                // Update physics for all players
                foreach (var sp in players.Values)
                {
                    // For now, simple gravity/friction if no input
                    // Real implementation would process input queue here
                    sp.Physics.ApplyGravity(deltaTime);
                    sp.Physics.UpdatePosition(deltaTime);
                    
                    if (sp.Physics.Position.Y < 0)
                    {
                        sp.Physics.Position.Y = 0;
                        sp.Physics.Velocity.Y = 0;
                        sp.Physics.OnGround = true;
                    }
                }
            }

            // Broadcast state every tick (or throttle to 20-30Hz)
            if (serverTick % 2 == 0) // Example: 30Hz if game is 60Hz
            {
                BroadcastState();
            }
        }

        private void NetworkLoop()
        {
            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            while (isRunning && udpListener != null)
            {
                try
                {
                    byte[] data = udpListener.Receive(ref sender);
                    ProcessPacket(sender, data);
                }
                catch (SocketException)
                {
                    // Ignore
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[SERVER] Error: " + ex.Message);
                }
            }
        }

        private void ProcessPacket(IPEndPoint sender, byte[] data)
        {
            if (data.Length < 1) return;

            string key = sender.ToString();
            PacketType type = (PacketType)data[0];

            lock (players)
            {
                if (!players.ContainsKey(key))
                {
                    // New player
                    Console.WriteLine("[SERVER] New connection from " + key);
                    var newPlayer = new ServerPlayer
                    {
                        EndPoint = sender,
                        PlayerID = nextPlayerId++,
                        Physics = new Physics3D(new Vector3(0, 10, 0)),
                        Data = new Player(nextPlayerId, "Player" + nextPlayerId)
                    };
                    players.Add(key, newPlayer);
                }

                var player = players[key];

                switch (type)
                {
                    case PacketType.PlayerInput:
                        var input = NetworkProtocol.DeserializeInput(data);
                        if (input.Sequence > player.LastSequence)
                        {
                            player.LastSequence = input.Sequence;
                            // Update player yaw/pitch for rendering/camera
                            player.Yaw = input.Yaw;
                            player.Pitch = input.Pitch;

                            lock(player.Physics)
                            {
                                var wishDir = new Engine.Vector3(
                                     (float)Math.Sin(input.Yaw) * input.Forward + (float)Math.Cos(input.Yaw) * input.Right,
                                     0,
                                     (float)Math.Cos(input.Yaw) * input.Forward - (float)Math.Sin(input.Yaw) * input.Right
                                ).Normalize();
                                
                                player.Physics.Update(wishDir, input.Jump, input.DeltaTime, null); // No map on server yet
                            }
                        }
                        break;
                }
            }
        }

        private Vector3 CalculateWishDir(float yaw, float forward, float right)
        {
             // Simple helper to convert yaw/input keys to direction
             // Similar to Game3DWindow logic
             if (Math.Abs(forward) < 0.01f && Math.Abs(right) < 0.01f) return Vector3.Zero;

             float x = (float)Math.Sin(yaw) * forward + (float)Math.Cos(yaw) * right;
             float z = (float)Math.Cos(yaw) * forward - (float)Math.Sin(yaw) * right;
             return new Vector3(x, 0, z).Normalize();
        }

        private void BroadcastState()
        {
            GameStatePacket state = new GameStatePacket
            {
                ServerTick = serverTick,
                LastProcessedInput = 0, // Simplified
                Players = new List<PlayerStatePacket>()
            };

            lock (players)
            {
                foreach (var sp in players.Values)
                {
                    state.Players.Add(new PlayerStatePacket
                    {
                        PlayerID = sp.PlayerID,
                        PosX = sp.Physics.Position.X,
                        PosY = sp.Physics.Position.Y,
                        PosZ = sp.Physics.Position.Z,
                        VelX = sp.Physics.Velocity.X,
                        VelY = sp.Physics.Velocity.Y,
                        VelZ = sp.Physics.Velocity.Z,
                        Yaw = sp.Yaw,
                        Pitch = sp.Pitch,
                        Health = sp.Data.Health,
                        OnGround = sp.Physics.OnGround
                    });
                }
            }

            byte[] data = NetworkProtocol.SerializeState(state);

            lock (players)
            {
                foreach (var sp in players.Values)
                {
                    try
                    {
                        udpListener.Send(data, data.Length, sp.EndPoint);
                    }
                    catch { }
                }
            }
        }
    }
}
