using System;
using Lidgren.Network;
using System.Collections.Generic;

namespace ServerSide
{
    class Program
    {
        static void Main(string[] args)
        {
            NetPeerConfiguration config = new NetPeerConfiguration("game") { Port = 9999 };
            NetServer server = new NetServer(config);
            server.Start();
            Console.WriteLine("Server started");

            Dictionary<long, (float, float)> playerPositions = new Dictionary<long, (float, float)>();

            while (true)
            {
                NetIncomingMessage inc;
                while ((inc = server.ReadMessage()) != null)
                {
                    switch (inc.MessageType)
                    {
                        case NetIncomingMessageType.Data:
                            string message = inc.ReadString();
                            if (message == "MOVE_TO")
                            {
                                float x = inc.ReadFloat();
                                float y = inc.ReadFloat();
                                Console.WriteLine($"Player {inc.SenderConnection.RemoteUniqueIdentifier} moved to {x}, {y}");
                                playerPositions[inc.SenderConnection.RemoteUniqueIdentifier] = (x, y);

                                // Send updated player positions to all clients
                                foreach (var connection in server.Connections)
                                {
                                    var outMessage = server.CreateMessage();
                                    outMessage.Write(inc.SenderConnection.RemoteUniqueIdentifier);
                                    outMessage.Write(x);
                                    outMessage.Write(y);
                                    server.SendMessage(outMessage, connection, NetDeliveryMethod.ReliableOrdered);
                                }
                            }
                            break;

                        default:
                            break;
                    }
                    server.Recycle(inc);
                }
            }
        }
    }
}
