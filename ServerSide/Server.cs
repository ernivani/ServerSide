using Lidgren.Network;
using System;
using System.Collections.Generic;

namespace ServerSide
{
    class Server
    {
        private NetServer _server;
        private Dictionary<NetConnection, int> playerPositions = new Dictionary<NetConnection, int>();

        static void Main(string[] args)
        {
            var server = new Server();
            server.Run();
        }

        private void Run()
        {
            var config = new NetPeerConfiguration("game");
            config.Port = 9999;
            config.MaximumConnections = 100;
            config.EnableMessageType(NetIncomingMessageType.ConnectionApproval);

            _server = new NetServer(config);
            _server.Start();

            Console.WriteLine("Server started");

            while (true)
            {
                var message = _server.ReadMessage();

                if (message != null)
                {
                    switch (message.MessageType)
                    {
                        case NetIncomingMessageType.ConnectionApproval:
                            playerPositions[message.SenderConnection] = 0;
                            message.SenderConnection.Approve();
                            break;
                        case NetIncomingMessageType.Data:
                            string command = message.ReadString();
                            if (command == "MOVE_LEFT")
                                playerPositions[message.SenderConnection]--;
                            else if (command == "MOVE_RIGHT")
                                playerPositions[message.SenderConnection]++;
                            break;
                        case NetIncomingMessageType.StatusChanged:
                            if ((NetConnectionStatus)message.ReadByte() == NetConnectionStatus.Disconnected)
                                playerPositions.Remove(message.SenderConnection);
                            break;
                        default:
                            Console.WriteLine("Unhandled message type");
                            break;
                    }

                    foreach (var player in playerPositions)
                    {
                        var msg = _server.CreateMessage();
                        msg.Write(player.Key.RemoteUniqueIdentifier);
                        msg.Write(player.Value);
                        _server.SendMessage(msg, message.SenderConnection, NetDeliveryMethod.ReliableOrdered);
                        Console.WriteLine("Sending position " + player.Value + " to " + player.Key.RemoteUniqueIdentifier);
                    }
                }
            }
        }
    }
}
