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
    class NetClient
    {
        TcpClient serverConnection;
        Thread reader;
        Thread writer;

        ClientNetManager gameClient;

        public NetClient(ClientNetManaer gameClient, string host, int port)
        {
            this.gameClient = gameClient
            serverConnection.Connect(host, port);
            var stream = serverConnection.GetStream();
            var streamReader = new StreamReader(stream);
            var streamWriter = new StreamWriter(stream);

            reader = new Thread(new ParameterizedThreadStart(HandleMessages));
            reader.Start(streamReader);

            writer = new Thread(new ParameterizedThreadStart(SendMessages));
            writer.Start(streamWriter);
        }

        private void SendMessages(object obj)
        {

        }

        private void HandleMessages(object obj)
        {
            var reader = (StreamReader)obj;
            while(true)
            {
                var line = reader.ReadLine();
                Console.WriteLine(line);
            }
        }
    }
}
