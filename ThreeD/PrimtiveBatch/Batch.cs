using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace ThreeD.PrimtiveBatch
{

    /// <summary>
    /// A BatchConfig is a struct of GraphicsDevice options. 
    /// A BC's uniqueness is determined by the values of its 4 fields
    /// </summary>
    internal struct BatchConfig
    {
        public Texture2D Texture;
        public PrimitiveType PrimtiveType;
        public SamplerState SamplerState;
        public BlendState BlendState;


        public BatchConfig(Texture2D texture, PrimitiveType primType, SamplerState sampler, BlendState blend)
        {
            Texture = texture;
            PrimtiveType = primType;
            BlendState = blend;
            SamplerState = sampler;
        }

        public override bool Equals(object obj)
        {
            if (obj is BatchConfig)
            {
                var other = (BatchConfig) obj;

                return other.PrimtiveType.Equals(PrimtiveType)
                       && other.BlendState.Equals(BlendState)
                       && other.Texture.Equals(Texture)
                       && other.SamplerState.Equals(SamplerState);

            }
            else return false;
        }

        public override int GetHashCode()
        {
            return Texture.GetHashCode()*3
                   + PrimtiveType.GetHashCode()*7
                   + SamplerState.GetHashCode()*11;
        }


    }

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

    internal class Batch
    {
        public BatchConfig Config { get; set; }

        public List<CustomVertexDeclaration> Verticies { get; set; }
        public List<short> Indicies { get; set; }

        public Batch(BatchConfig config)
        {
            Config = config;
            Verticies = new List<CustomVertexDeclaration>();
            Indicies = new List<short>();
        }

        public void Add(VerticiesAndIndicies vai)
        {
            var start = Verticies.Count;
            Verticies.AddRange(vai.Verticies);
            vai.Indices.ForEach(i => Indicies.Add( (short)(i + start) ));
        }

        

    }



}
