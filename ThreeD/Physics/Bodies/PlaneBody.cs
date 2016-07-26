using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DCG.Framework.Physics.Colliders;
using Microsoft.Xna.Framework;

namespace DCG.Framework.Physics.Bodies
{
    public class PlaneBody : PhysicsBody
    {
        public Vector3 Normal { get { return _collider.Normal; } set { _collider.Normal = value; } }
        public float Offset { get { return _collider.Offset; } set { _collider.Offset = value; } }

        private readonly PlaneCollider _collider;

        public PlaneBody(Vector3 normal, float offset)
        {
            _collider = new PlaneCollider(this)
            {
                Normal = normal,
                Offset = offset
            };
            IsMovable = false;
        }

        public override AbsCollider Collider
        {
            get { return _collider; }
        }

        public override void AfterUpdate(GameTime time)
        {
            // do nothing.
        }
    }
}
