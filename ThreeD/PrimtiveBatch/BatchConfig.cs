using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace DCG.Framework.PrimtiveBatch
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
                var other = (BatchConfig)obj;

                return other.PrimtiveType.Equals(PrimtiveType)
                       && other.BlendState.Equals(BlendState)
                       && other.Texture.Equals(Texture)
                       && other.SamplerState.Equals(SamplerState);

            }
            else return false;
        }

        public override int GetHashCode()
        {
            return Texture.GetHashCode() * 3
                   + PrimtiveType.GetHashCode() * 7
                   + SamplerState.GetHashCode() * 11;
        }


    }
}
