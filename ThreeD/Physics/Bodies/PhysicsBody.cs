using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DCG.Framework.Physics.Colliders;
using Microsoft.Xna.Framework;

namespace DCG.Framework.Physics.Bodies
{
    public abstract class PhysicsBody
    {
        public Vector3 Position { get; set; }
        public Vector3 Velocity { get; set; }
        public Vector3 SigmaForce { get; set; }
        public float Mass { get; set; }

        public Quaternion Rotation { get; set; }

        public bool IsMovable { get; set; }
        public abstract AbsCollider Collider { get; }

        protected PhysicsBody() : this(Vector3.Zero) { }

        protected PhysicsBody(Vector3 position)
        {
            Position = position;
            Velocity = Vector3.Zero;
            SigmaForce = Vector3.Zero;
            Mass = 1;

            Rotation = Quaternion.Identity;

            IsMovable = true;
        }

        public void Update(GameTime time)
        {
            var inverseMass = 1f/Mass;
            if (IsMovable)
            {
                var acceleration = SigmaForce*inverseMass;
                Velocity += acceleration;
                Position += Velocity;

                SigmaForce = Vector3.Zero;
            }

            AfterUpdate(time);
        }

        public abstract void AfterUpdate(GameTime time);
    }
}
