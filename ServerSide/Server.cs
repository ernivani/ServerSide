using Lidgren.Network;
using System.Numerics;

namespace ServerSide
{
    class Server
    {
        private NetServer _server;
        private Dictionary<NetConnection, Vector2> playerPositions = new Dictionary<NetConnection, Vector2>();
        private object positionsLock = new object(); // Lock for playerPositions

        static void Main(string[] args)
        {
            Console.WriteLine("Starting server...");
            Console.WriteLine(args.Length);
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

            // Start a new thread to handle incoming messages
            Thread incomingMessageThread = new Thread(new ThreadStart(HandleIncomingMessages));
            incomingMessageThread.Start();

            while (true)
            {
                lock (positionsLock)  // Lock the dictionary while iterating
                {
                    foreach (var player in playerPositions)
                    {
                        if (player.Key.Status == NetConnectionStatus.Connected)
                        {
                            foreach (var otherPlayer in playerPositions)
                            {
                                var msg = _server.CreateMessage();
                                msg.Write(otherPlayer.Key.RemoteUniqueIdentifier);
                                msg.Write(otherPlayer.Value.X);
                                msg.Write(otherPlayer.Value.Y);
                                _server.SendMessage(msg, player.Key, NetDeliveryMethod.ReliableOrdered);
                            }
                        }
                    }
                }
                Thread.Sleep(10);  // Give other threads a chance to run
            }
        }

        private void HandleIncomingMessages()
        {
            while (true)
            {
                var message = _server.ReadMessage();

                if (message != null)
                {
                    switch (message.MessageType)
                    {
                        case NetIncomingMessageType.ConnectionApproval:
                            lock (positionsLock)
                            {
                                playerPositions[message.SenderConnection] = Vector2.Zero;
                            }
                            message.SenderConnection.Approve();
                            break;
                        case NetIncomingMessageType.Data:
                            string command = message.ReadString();
                            lock (positionsLock)
                            {
                                Console.WriteLine("Received command: " + command);
                                if (command == "MOVE_LEFT")
                                    playerPositions[message.SenderConnection] = new Vector2(playerPositions[message.SenderConnection].X - 1, playerPositions[message.SenderConnection].Y);
                                else if (command == "MOVE_RIGHT")
                                    playerPositions[message.SenderConnection] = new Vector2(playerPositions[message.SenderConnection].X + 1, playerPositions[message.SenderConnection].Y);
                                else if (command == "MOVE_UP") 
                                    playerPositions[message.SenderConnection] = new Vector2(playerPositions[message.SenderConnection].X, playerPositions[message.SenderConnection].Y - 1);
                                else if (command == "MOVE_DOWN")
                                    playerPositions[message.SenderConnection] = new Vector2(playerPositions[message.SenderConnection].X, playerPositions[message.SenderConnection].Y + 1);
                                else if (command == "MOVE_TO")
                                {
                                    float x = message.ReadFloat();
                                    float y = message.ReadFloat();
                                    Console.WriteLine("Moving to: " + x + ", " + y);
                                    Vector2 position = new Vector2(x, y);
                                    playerPositions[message.SenderConnection] = position;
                                }
                            }
                            break;
                        case NetIncomingMessageType.StatusChanged:
                            if ((NetConnectionStatus)message.ReadByte() == NetConnectionStatus.Disconnected)
                            {
                                lock (positionsLock)
                                {
                                    playerPositions.Remove(message.SenderConnection);
                                }
                            }
                            break;
                    }
                }
            }
        }
    }
}
