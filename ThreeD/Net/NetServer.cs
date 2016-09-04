using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DCG.Framework.Net
{
    public class NetServer
    {
        private TcpListener server;
        private Thread listenerThread;
        private static ManualResetEvent allDone = new ManualResetEvent(false);
        private Dictionary<int, NetPlayer> clients;
        private static int nextClientId = 0;



        public NetServer()
        {
            server = new TcpListener(IPAddress.Any, 3366);
            clients = new Dictionary<int, NetPlayer>();
            listenerThread = new Thread(new ThreadStart(AwaitConnections));
            listenerThread.Start();
        }

        private void AwaitConnections()
        {
            this.server.Start();
            while (true)
            {
                TcpClient client = this.server.AcceptTcpClient();

                Thread clientThread = new Thread(new ParameterizedThreadStart(HandleInputs));
                TcpClient tcpClient = (TcpClient)client;
                var player = new NetPlayer(tcpClient);

                clients.Add(player.Id, player);

                clientThread.Start(player);
            }
        }

        private void HandleInputs(object p)
        {
            var player = (NetPlayer) p;
            var stream = player.Socket.GetStream();
            var streamWriter = new StreamWriter(stream);
            var streamReader = new StreamReader(stream);

            var buffer = new byte[1024];
            streamWriter.WriteLine("Welcome, player " + player.Id);
            streamWriter.Flush();
            while (!streamReader.EndOfStream)
            {
                Console.WriteLine(streamReader.ReadLine());
            }
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);
            Console.WriteLine("got connection from {0}", listener.RemoteEndPoint);
            allDone.Reset();
        }
    }
}
