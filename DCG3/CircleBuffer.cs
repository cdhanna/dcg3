using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCG.Framework.Net
{
    public class CicularBuffer<T>
    {

        public int Size { get; private set; }
        public int Head { get; private set; }
        public int Tail { get; private set; }

        private T[] _buffer;

        public CicularBuffer(int size)
        {
            Size = size + 1;
            _buffer = new T[Size];
        }

        public void AddElement(T elem)
        {
            /* length = 2
             * 0 1 
             * D 
             * T H
             */
            if ((Head + 1) % Size == Tail)
            {
                throw new Exception("aw poop, you overwrote data on the cbuffer before reading it");
            }

            var nextSlot = Head % Size;
            Head += 1;
            _buffer[nextSlot] = elem;
        }

        public T PullElement()
        {
            if (Tail == Head)
            {
                throw new Exception("You read from the cbuffer without new data available");
            }

            var pullSlot = Tail % Size;
            Tail += 1;
            return _buffer[pullSlot];
        }

        public List<T> PullToHead()
        {
            var stuff = new List<T>();
            while (Tail != Head)
            {
                stuff.Add(PullElement());
            }
            return stuff;
        }

    }
}
