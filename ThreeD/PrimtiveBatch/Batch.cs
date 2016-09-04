using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace DCG.Framework.PrimtiveBatch
{
    internal class Batch
    {
        public BatchConfig Config { get; set; }

        public VertexPositionColorNormalTexture[] VertexArray { get; set; }
        public uint[] IndexArray { get; set; }
        public int VertexArrayCapacity { get; set; }
        public int IndexArrayCapacity { get; set; }

        private int _indexArrayRunningIndex;
        private int _vertexArrayRunningIndex;
        private int _count;

        private static readonly uint[] CubeIndicies = new uint[]
        {
            0, 1, 2, 0, 2, 3,
            4, 5, 6, 4, 6, 7,
            8, 9, 10, 8, 10, 11,
            12, 13, 14, 12, 14, 15,
            16, 17, 18, 16, 18, 19,
            20, 21, 22, 20, 22, 23
        };

        public Batch(BatchConfig config)
        {
            
            Config = config;

            VertexArrayCapacity = 8000;
            IndexArrayCapacity = 8000;
            _vertexArrayRunningIndex = 0;
            _indexArrayRunningIndex = 0;
            VertexArray = new VertexPositionColorNormalTexture[VertexArrayCapacity];
            IndexArray = new uint[IndexArrayCapacity];
        }

        public void AddVertex(VertexPositionColorNormalTexture v)
        {
            // check and make sure that our size is big enough to hold the new vertex.
            if (_vertexArrayRunningIndex >= VertexArrayCapacity)
            {
                VertexArrayCapacity *= 2;
                var next = new VertexPositionColorNormalTexture[VertexArrayCapacity];
                Array.Copy(VertexArray, 0, next, 0, VertexArray.Length);
                VertexArray = next;
            }

            // plop the vertex in the array
            VertexArray[_vertexArrayRunningIndex] = v;
            _vertexArrayRunningIndex += 1;
        }

        public int GetVertexArrayLength()
        {
            return _vertexArrayRunningIndex;
        }

        public void AddSomeIndicies(uint[] indicies, uint offset)
        {
            // check and make sure that our size is big enough to hold the new indicies.
            while (_indexArrayRunningIndex + indicies.Length >= IndexArrayCapacity)
            {
                IndexArrayCapacity *= 2;
                var next = new uint[IndexArrayCapacity];
                Array.Copy(IndexArray, 0, next, 0, IndexArray.Length);
                IndexArray = next;
            }

            //_indexArrayRunningIndex = indicies.Length;
            // time to go in and add the 36 points for each cube.
            for (int i = 0; i < indicies.Length; i++)
            {
                IndexArray[i + _indexArrayRunningIndex] = (uint)(indicies[i] + (uint)offset);
            }
           
            _indexArrayRunningIndex += indicies.Length;

        }

        //public void AddCubeIndicies()
        //{

        //    // check and make sure that our size is big enough to hold the new indicies.
        //    if (_indexArrayRunningIndex + CubeIndicies.Length >= IndexArrayCapacity)
        //    {
        //        IndexArrayCapacity *= 2;
        //        var next = new uint[IndexArrayCapacity];
        //        Array.Copy(IndexArray, 0, next, 0, IndexArray.Length);
        //        IndexArray = next;
        //    }
            
        //    // time to go in and add the 36 points for each cube.
        //    for (int i = 0; i < CubeIndicies.Length; i++)
        //    {
        //        IndexArray[i + _indexArrayRunningIndex] = (uint) (CubeIndicies[i] + (uint)_count);
        //    }
        //    _count += 24;
        //    _indexArrayRunningIndex += CubeIndicies.Length;
        //}

        public int GetIndexArrayLength()
        {
            return _indexArrayRunningIndex;
        }

    }
}
