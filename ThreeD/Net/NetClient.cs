using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DCG.Framework.Net
{
    public class NetClient<TState> where TState : NetState
    {
        TcpClient serverConnection;
        Thread reader;
        Thread writer;

        ClientNetManager<TState> gameClient;
        ConcurrentQueue<InputCollection> pendingInputs;

        public NetClient(ClientNetManager<TState> gameClient, string host, int port)
        {
            this.gameClient = gameClient;
            serverConnection = new TcpClient(host, port);

            var stream = serverConnection.GetStream();
            var streamReader = new StreamReader(stream);
            var streamWriter = new StreamWriter(stream);


            pendingInputs = new ConcurrentQueue<InputCollection>();

            reader = new Thread(new ParameterizedThreadStart(MessageRecvWorker));
            reader.Start(streamReader);

            writer = new Thread(new ParameterizedThreadStart(MessageSendWorker));
            writer.Start(streamWriter);
        }

        private void MessageSendWorker(object obj)
        {
            var writer = (StreamWriter)obj;
            InputCollection inputs;
            while (true)
            {
                var success = pendingInputs.TryDequeue(out inputs);
                if (success)
                {
                    writer.WriteLine(JsonConvert.SerializeObject(inputs));
                }
            }

        }

        public void QueueMessage(InputCollection inputs)
        {
            pendingInputs.Enqueue(inputs);
            gameClient.BufferInput(inputs);
        }

        private void MessageRecvWorker(object obj)
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
