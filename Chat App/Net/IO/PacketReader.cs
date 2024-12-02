using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Chat_App.Net.IO
{
    class PacketReader : BinaryReader
    {
        private NetworkStream _ms;
        public PacketReader(NetworkStream ms) : base(ms)
        {
            _ms = ms;
        }

        public string ReadMessage()
        {
            byte[] msgBuffer;
            var length = ReadInt32();
            msgBuffer = new byte[length];
            _ms.Read(msgBuffer, 0, length);

            var msg = Encoding.ASCII.GetString(msgBuffer);
            return msg;
        }
    }
}
