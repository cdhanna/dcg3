using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCG.Framework.Net
{
    public abstract class NetState
    {
        public int LastAcknowledgedStep { get; private set; }

        protected NetState()
        {
            LastAcknowledgedStep = 0;
        }
    }
}
