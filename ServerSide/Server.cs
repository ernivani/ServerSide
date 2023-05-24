using Lidgren.Network;
using System;

namespace ServerSide
{
    class Server
    {
        private NetServer _server;
        
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
                            Console.WriteLine("Connection approval");
                            message.SenderConnection.Approve();
                            break;
                        case NetIncomingMessageType.Data:
                            Console.WriteLine("Data received");
                            break;
                        case NetIncomingMessageType.StatusChanged:
                            Console.WriteLine("Status changed");
                            break;
                        default:
                            Console.WriteLine("Unhandled message type");
                            break;
                    }
                }
            }
        }
       
    }
}