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



        public NetServer()
        {
            server = new TcpListener(IPAddress.Any, 3366);
            clients = new Dictionary<int, NetPlayer>();
            listenerThread = new Thread(new ThreadStart(AwaitConnections));
            listenerThread.Start();
        }

        private void AwaitConnections()
        {
            server.Start();
            while (true)
            {
                TcpClient client = this.server.AcceptTcpClient();
                var player = new NetPlayer(client);
                clients.Add(player.Id, player);
            }
        }
    }
}
