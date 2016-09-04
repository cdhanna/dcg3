using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCG.Framework.Net
{
    public class ClientNetManager<TState> where TState : NetState
    {

        private int _lastAckedStepNumber;
        private List<InputCollection> _bufferedInputs;

        private List<INetStateHandler<TState>> _handlers;
        public ClientNetManager()
        {
            _handlers = new List<INetStateHandler<TState>>();
            _bufferedInputs = new List<InputCollection>();
        }

        public void AddHandler(INetStateHandler<TState> handler)
        {
            _handlers.Add(handler);
        }

        public void SendInput(InputCollection ic)
        {
            _bufferedInputs.Add(ic);
        }

        public void Update(TState state)
        {
            _handlers.ForEach(handler =>
            {
                handler.SetState(state);
                _bufferedInputs.ForEach(ic => handler.ApplyInput(ic));

            });
            
        }


    }
}
