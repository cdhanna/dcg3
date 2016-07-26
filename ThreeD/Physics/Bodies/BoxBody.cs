using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DCG.Framework.Physics.Colliders;
using Microsoft.Xna.Framework;

namespace DCG.Framework.Physics.Bodies
{
    public class BoxBody : PhysicsBody
    {
        private readonly BoxCollider _collider;

        
        public Vector3 Size { get { return _collider.Size; } set { _collider.Size = value; } }

        public BoxBody(Vector3 position, Vector3 size) : base(position)
        {
            _collider = new BoxCollider(this)
            {
                Position = position,
                Size = size,
                Rotation = Quaternion.Identity
            };
        }

        public override AbsCollider Collider
        {
            get { return _collider; }
        }

        public override void AfterUpdate(GameTime time)
        {
            _collider.Position = Position;
            _collider.Rotation = Rotation;
        }
    }
}
