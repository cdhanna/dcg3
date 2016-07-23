using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCG.Framework.PrimtiveBatch
{

    internal class BatchCollection
    {
        private Dictionary<BatchConfig, Batch> _batches;

        public BatchCollection()
        {
            _batches = new Dictionary<BatchConfig, Batch>();
        }

        public void Clear()
        {
            _batches.Clear();
        }

        public List<Batch> GetAll()
        {
            return _batches.Values.ToList();
        }

        public Batch Get(BatchConfig conf)
        {
            if (!_batches.ContainsKey(conf))
                _batches.Add(conf, new Batch(conf));
            return _batches[conf];
        }

    }
}
