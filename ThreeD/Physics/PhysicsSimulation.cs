using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DCG.Framework.Physics.Bodies;
using DCG.Framework.Physics.Colliders;
using Microsoft.Xna.Framework;

namespace DCG.Framework.Physics
{
    public class BodyPair
    {
        public PhysicsBody BodyA { get; set; }
        public PhysicsBody BodyB { get; set; }

        public BodyPair(PhysicsBody a, PhysicsBody b)
        {
            BodyA = a;
            BodyB = b;
        }

        public override bool Equals(object obj)
        {
            if (obj is BodyPair)
            {
                var other = obj as BodyPair;
                return (other.BodyA == BodyA && other.BodyB == BodyB)
                       || (other.BodyA == BodyB && other.BodyB == BodyA);
            }
            else return false;
        }

        public override int GetHashCode()
        {
            return BodyA.GetHashCode() + BodyB.GetHashCode();
        }
    }

    public class PhysicsSimulation
    {
        public List<PhysicsBody> Bodies { get; private set; }
        public Vector3 Gravity { get; set; }


        public PhysicsSimulation()
        {
            Bodies = new List<PhysicsBody>();
            Gravity = Vector3.Zero;
        }

        public void Update(GameTime time)
        {
            // apply gravity force. Fg = mass * g
            Bodies.ForEach(b => b.SigmaForce += b.Mass * Gravity);

            // move all the bodies. 
            Bodies.ForEach(b => b.Update(time));

            // produce object pairs to check. 
            var bodyPairs = new List<BodyPair>();
            for (var i = 0; i < Bodies.Count; i++)
            {
                for (var j = 0; j < Bodies.Count; j++)
                {
                    if (i != j)
                    {
                        var pair = new BodyPair(Bodies[i], Bodies[j]);
                        if (!bodyPairs.Contains(pair))
                        {
                            bodyPairs.Add(pair);
                        }
                    }
                }
            }

            // check for contacts in pairs
            var contacts = new List<Contact>();
            for (var i = 0; i < bodyPairs.Count; i++)
            {
                var pair = bodyPairs[i];
                var res = pair.BodyA.Collider.CheckCollision(pair.BodyB.Collider);

                //for (var j = 0; j < res.ContactCount; j++)
                //{
                //    contacts.Add(res.Contacts[j]);
                //}
                if (res.ContactCount > 0)
                {
                    contacts.Add(res.Contacts[0]);
                }
            }


            // resolve contacts
            contacts.ForEach(c =>
            {
                c.bodyA.Position += c.Normal*c.Penetration;
                c.bodyA.Velocity *= -1;

                //c.bodyB.Velocity *= -1;
            });
        }
    }
}
