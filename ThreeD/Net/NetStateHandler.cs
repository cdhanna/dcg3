using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCG.Framework.Net
{
    public interface INetStateHandler<TState> where TState : NetState
    {
        void SetState(NetState state);

        void ApplyInput(InputCollection inputs);
    }
}
