using Chat_App.Net.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Chat_App.Net
{
     class Server
    {
        TcpClient _client;
        public PacketReader PacketReader;

        public event Action connectedEvent;
        public event Action msgReceivedEvent;
        public event Action userDisconnectEvent;

        //public string serverUrl;
        //public int port;

        public Server()
        {
           _client = new TcpClient();
        }

        public void ConnectToServer(string username, string serverUrl, int serverport)
        {
            Console.WriteLine($"the url is {serverUrl}");
            Console.WriteLine($"the url is {serverport}");
            try
            {
                if (!_client.Connected)
                {
                    _client.Connect(serverUrl, serverport);
                    PacketReader = new PacketReader(_client.GetStream());

                    if (!string.IsNullOrEmpty(username))
                    {
                        var connectPacket = new PacketBuilder();
                        connectPacket.WriteOpCode(0);
                        connectPacket.WriteMessage(username);
                        _client.Client.Send(connectPacket.GetPacketBytes());
                    }

                    ReadPackets();

                }
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine(e);
                Console.WriteLine($"the url isn't {serverUrl}");
                Console.WriteLine($"the url isn't {serverport}");
                throw;
            }
            
        }

        private void ReadPackets()
        {
            Task.Run(() =>
            {
                while(true) 
                { 
                    var opcode = PacketReader.ReadByte();
                    switch (opcode) 
                    {
                        case 1:
                            connectedEvent?.Invoke();
                            break;

                        case 5:
                            msgReceivedEvent?.Invoke();
                            break;

                        case 10:
                            userDisconnectEvent?.Invoke();
                            break;

                        default:
                            Console.WriteLine("ah yes...");
                            break;
                    }
                }   
            });
        }

        public void SendMessageToServer(string message) 
        {
            var messagePacket = new PacketBuilder();
            messagePacket.WriteOpCode(5);
            messagePacket.WriteMessage(message);
            _client.Client.Send(messagePacket.GetPacketBytes());

        }
    }
}
