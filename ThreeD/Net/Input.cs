using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCG.Framework.Net
{

    

    public interface Input
    {
        object Value { get; }
        
    }
    public abstract class InputTyped<T> : Input
    {
        public object Value { get; private set; }

        public T TypedValue { get { return (T) Value; } set { Value = value; } }

        protected InputTyped(T value)
        {
            Value = value;
        }
    }

    public class InputCollection
    {
        private Input[] _inputs;
        private int StepNumber { get; set; }

        public int Size { get { return _inputs.Length; } }

        public InputCollection(Input[] inputs)
        {
            _inputs = inputs;
        }

        

        public List<T> Get<T>() 
            where T : class , Input
        {
            
            return _inputs
                .Where(i => i.GetType().IsEquivalentTo(typeof(T)))
                .Cast<T>()
                .ToList();
        }


    }
}
