using MessengerShared.Communication;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace MessengerShared.Network
{
    public class MessageTransport
    {
        public void Send(NetworkStream stream, Message msg)
        {
            String json = JsonSerializer.Serialize(msg);
            Byte[] message = System.Text.Encoding.UTF8.GetBytes(json);
            Byte[] size = BitConverter.GetBytes(message.Length);

            stream.Write(size, 0, 4);
            stream.Write(message, 0, message.Length);
        }

        public Message? Receive(NetworkStream stream)
        {
            
            Byte[] buffer = new Byte[4];
            ReadExactly(stream, buffer);

            int size = BitConverter.ToInt32(buffer);
            buffer = new byte[size];
            ReadExactly(stream, buffer);

            Message? msg = JsonSerializer.Deserialize<Message>(buffer);
            return msg;
        }

        private void ReadExactly(NetworkStream stream, byte[] buffer)
        {
            int totalRead = 0;
            while (totalRead < buffer.Length)
            {
                int bytesRead = stream.Read(buffer, totalRead, buffer.Length - totalRead);

                if (bytesRead == 0) throw new EndOfStreamException();

                totalRead += bytesRead;
            }
        }
    }
}
