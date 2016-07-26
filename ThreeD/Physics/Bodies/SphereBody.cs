using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DCG.Framework.Physics.Colliders;
using Microsoft.Xna.Framework;

namespace DCG.Framework.Physics.Bodies
{
    public class SphereBody : PhysicsBody
    {
        public float Radius { get { return _collider.Radius; } set { _collider.Radius = value; } }
        
        public override AbsCollider Collider { get { return _collider; } }

        private readonly SphereCollider _collider;

        public SphereBody() : this(Vector3.Zero, 1f) { }
        public SphereBody(Vector3 position, float radius) : base(position)
        {
            _collider = new SphereCollider(this)
            {
                Position = Position,
                Radius = radius
            };
        }

        public override void AfterUpdate(GameTime time)
        {
            _collider.Position = Position;
        }
    }
}
