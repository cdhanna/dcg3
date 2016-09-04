using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCG.Framework.Net
{
    public class InputManager
    {

        private List<InputGenerator> _inputGenerators;

        public InputManager(params InputGenerator[] generators)
        {
            _inputGenerators = new List<InputGenerator>();
            generators.ToList().ForEach(g => AddInputGenerator(g));
        }

        public void AddInputGenerator(InputGenerator generator)
        {
            _inputGenerators.Add(generator);
        }

        public InputCollection Update()
        {
            var allInputs = new List<Input>();
            _inputGenerators.ForEach(g => allInputs.AddRange(g.CheckForInput()));

            var ic = new InputCollection(allInputs.ToArray());
            return ic;
        }

    }
}
