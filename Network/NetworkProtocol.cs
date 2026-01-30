using System;
using System.Collections.Generic;
using System.IO;
using Lifeblood.Engine;

namespace Lifeblood.Network
{
    public enum PacketType : byte
    {
        PlayerInput,
        GameState,
        PlayerSpawned,
        PlayerDied,
        Ping,
        Pong
    }

    public struct PlayerInputPacket
    {
        public uint Sequence;
        public float DeltaTime;
        public float Forward;
        public float Right;
        public bool Jump;
        public bool Shoot;
        public float Yaw;
        public float Pitch;
    }

    public struct PlayerStatePacket
    {
        public int PlayerID;
        public float PosX, PosY, PosZ;
        public float VelX, VelY, VelZ;
        public float Yaw, Pitch;
        public int Health;
        public bool OnGround;
    }

    public struct GameStatePacket
    {
        public uint ServerTick;
        public uint LastProcessedInput;
        public List<PlayerStatePacket> Players;
    }

    public class PacketWriter : System.IO.BinaryWriter
    {
        public PacketWriter(System.IO.MemoryStream ms) : base(ms) { }

        public void Write(Engine.Vector3 v)
        {
            Write(v.X);
            Write(v.Y);
            Write(v.Z);
        }
    }

    public class PacketReader : System.IO.BinaryReader
    {
        public PacketReader(System.IO.MemoryStream ms) : base(ms) { }

        public Engine.Vector3 ReadVector3()
        {
            return new Engine.Vector3(ReadSingle(), ReadSingle(), ReadSingle());
        }
    }

    public static class NetworkProtocol
    {
        public const int DefaultPort = 7777;
        public const string ProtocolVersion = "LIFEBLOOD_V1";
        public const int MaxPacketSize = 1024;

        public static byte[] SerializeInput(PlayerInputPacket input)
        {
            using (var ms = new System.IO.MemoryStream())
            {
                var writer = new PacketWriter(ms);
                writer.Write((byte)PacketType.PlayerInput);
                writer.Write(input.Sequence);
                writer.Write(input.DeltaTime);
                writer.Write(input.Forward);
                writer.Write(input.Right);
                writer.Write(input.Jump);
                writer.Write(input.Shoot);
                writer.Write(input.Yaw);
                writer.Write(input.Pitch);
                return ms.ToArray();
            }
        }

        public static PlayerInputPacket DeserializeInput(byte[] data)
        {
            using (var ms = new System.IO.MemoryStream(data))
            {
                var reader = new PacketReader(ms);
                reader.ReadByte(); // Type
                return new PlayerInputPacket
                {
                    Sequence = reader.ReadUInt32(),
                    DeltaTime = reader.ReadSingle(),
                    Forward = reader.ReadSingle(),
                    Right = reader.ReadSingle(),
                    Jump = reader.ReadBoolean(),
                    Shoot = reader.ReadBoolean(),
                    Yaw = reader.ReadSingle(),
                    Pitch = reader.ReadSingle()
                };
            }
        }

        public static byte[] SerializeState(GameStatePacket state)
        {
            using (var ms = new System.IO.MemoryStream())
            {
                var writer = new PacketWriter(ms);
                writer.Write((byte)PacketType.GameState);
                writer.Write(state.ServerTick);
                writer.Write(state.LastProcessedInput);
                
                int count = state.Players != null ? state.Players.Count : 0;
                writer.Write(count);

                if (state.Players != null)
                {
                    foreach (var p in state.Players)
                    {
                        writer.Write(p.PlayerID);
                        writer.Write(p.PosX); writer.Write(p.PosY); writer.Write(p.PosZ);
                        writer.Write(p.VelX); writer.Write(p.VelY); writer.Write(p.VelZ);
                        writer.Write(p.Yaw); writer.Write(p.Pitch);
                        writer.Write(p.Health);
                        writer.Write(p.OnGround);
                    }
                }
                return ms.ToArray();
            }
        }

        public static GameStatePacket DeserializeState(byte[] data)
        {
            using (var ms = new System.IO.MemoryStream(data))
            {
                var reader = new PacketReader(ms);
                reader.ReadByte(); // Type
                var state = new GameStatePacket();
                state.ServerTick = reader.ReadUInt32();
                state.LastProcessedInput = reader.ReadUInt32();
                
                int count = reader.ReadInt32();
                state.Players = new List<PlayerStatePacket>();

                for (int i = 0; i < count; i++)
                {
                    state.Players.Add(new PlayerStatePacket
                    {
                        PlayerID = reader.ReadInt32(),
                        PosX = reader.ReadSingle(), PosY = reader.ReadSingle(), PosZ = reader.ReadSingle(),
                        VelX = reader.ReadSingle(), VelY = reader.ReadSingle(), VelZ = reader.ReadSingle(),
                        Yaw = reader.ReadSingle(), Pitch = reader.ReadSingle(),
                        Health = reader.ReadInt32(),
                        OnGround = reader.ReadBoolean()
                    });
                }
                return state;
            }
        }
    }
}
