using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCG.Framework.Net
{
    class CircleBuffer<T>
    {

        public int Size { get; private set; }

        private T[] _buffer;

        public CircleBuffer(int size)
        {
            Size = size;
            _buffer = new T[size];
        }

        public void AddElement(T elem)
        {

        }

    }
}
