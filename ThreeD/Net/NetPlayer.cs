using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DCG.Framework.Net
{
    class NetPlayer
    {
        private static int nextId = 0;
        private Thread reader;
        private Thread writer;
        public NetPlayer(TcpClient tcpClient)
        {
            this.Socket = tcpClient;
            var stream = Socket.GetStream();
            var streamReader = new StreamReader(stream);
            var streamWriter = new StreamWriter(stream);

            reader = new Thread(new ParameterizedThreadStart(MessageRecvWorker));
            writer = new Thread(new ParameterizedThreadStart(MessageSendWorker));

            reader.Start(streamReader);
            writer.Start(streamWriter);

            this.Id = nextId++;
        }

        private void MessageSendWorker(object objWriter)
        {
            var writer = (StreamWriter)objWriter;
        }

        private void MessageRecvWorker(object objReader)
        {
            var reader = (StreamReader)objReader;
            while(true)
            {
                var line = reader.ReadLine();
                Console.WriteLine(String.Format("{0}:  {1}", Id, line));
            }
        }

        public int Id { get; private set; }
        public TcpClient Socket { get; private set; }
    }
}
