using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace DCG.Framework.Net
{
    class NetPlayer
    {
        private static int nextId = 0;
        public NetPlayer(TcpClient tcpClient)
        {
            this.Socket = tcpClient;
            this.Id = nextId++;
        }

        public int Id { get; private set; }
        public TcpClient Socket { get; private set; }
    }
}
