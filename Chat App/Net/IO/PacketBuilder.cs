﻿using System;
using System.IO;
using System.Text;


namespace Chat_App.Net.IO
{
    class PacketBuilder
    {
        MemoryStream _ms;
        public PacketBuilder()
        {
            _ms = new MemoryStream();

        }

        public void WriteOpCode(byte opcode) 
        {
           _ms.WriteByte(opcode); 
        }

        public void WriteMessage(string msg)
        {
            var msgLength = msg.Length;
            _ms.Write(BitConverter.GetBytes(msgLength), 0, 4); // Write the message length (4 bytes)
            _ms.Write(Encoding.ASCII.GetBytes(msg), 0, msgLength); // Write the message itself
        }
        public byte[] GetPacketBytes() 
        {
            return _ms.ToArray();
        }
    }
}
