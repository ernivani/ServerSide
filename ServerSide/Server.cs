using System;
using System.Collections.Generic;
using Lidgren.Network;
using System.Numerics;

namespace ServerSide
{
    public class Player
    {
        public long Id { get; private set; }
        public Vector2 Position { get; set; }

        public Player(long id, Vector2 position)
        {
            this.Id = id;
            this.Position = position;
        }
    }

    class Program
    {
        static void Main()
        {
            var config = new NetPeerConfiguration("game") { Port = 9999 };
            var server = new NetServer(config);
            server.Start();

            var players = new Dictionary<long, Player>();

            Console.WriteLine("Server started...");

            while (true)
            {
                NetIncomingMessage inc;
                while ((inc = server.ReadMessage()) != null)
                {
                    if (inc.MessageType == NetIncomingMessageType.Data)
                    {
                        var id = inc.ReadInt64();
                        var pos = new Vector2(inc.ReadFloat(), inc.ReadFloat());

                        if (!players.TryGetValue(id, out Player player))
                        {
                            player = new Player(id, pos);
                            players.Add(id, player);
                            Console.WriteLine($"New player {id} connected.");
                        }

                        player.Position = pos;

                        foreach (var p in players.Values)
                        {
                            Console.WriteLine($"Player {p.Id}: {p.Position.X}, {p.Position.Y}");
                        }
                    }

                    server.Recycle(inc);
                }
            }
        }
    }
}