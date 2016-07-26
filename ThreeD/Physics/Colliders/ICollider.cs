using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using DCG.Framework.Physics.Bodies;
using DCG.Framework.Util;
using Microsoft.Xna.Framework;

namespace DCG.Framework.Physics.Colliders
{
    public abstract class AbsCollider
    {
        public PhysicsBody Body;

        protected AbsCollider(PhysicsBody body)
        {
            Body = body;
        }

        public CollisionResult CheckCollision(AbsCollider other)
        {
            return ColliderChecks.Check(this, other);
        }
    }

   
    public class SphereCollider : AbsCollider
    {
        public Vector3 Position { get; set; }
        public float Radius { get; set; }
        public SphereCollider(PhysicsBody body) : base(body) { }
    }

    public class BoxCollider : AbsCollider
    {
        public Vector3 Position { get; set; }
        public Vector3 Size { get; set; }
        public Quaternion Rotation { get; set; }

        public BoxCollider(PhysicsBody body) : base(body) { }

    }

    public class PlaneCollider : AbsCollider
    {
        public float Offset { get; set; }
        public Vector3 Normal { get; set; }
        public PlaneCollider(PhysicsBody body) : base(body) { }

    }

    public class ColliderChecks
    {
 
        public static CollisionResult Check(AbsCollider a, AbsCollider b)
        {
            var aType = a.GetType();
            var bType = b.GetType();

            var res = new CollisionResult();

            var sphere = typeof (SphereCollider);
            var box = typeof (BoxCollider);
            var plane = typeof (PlaneCollider);

            var toSphere = new Func<AbsCollider, SphereCollider>(c => c as SphereCollider);
            var toBox = new Func<AbsCollider, BoxCollider>(c => c as BoxCollider);
            var toPlane = new Func<AbsCollider, PlaneCollider>(c => c as PlaneCollider);

            if (aType == sphere && bType == plane)
                SphereOnPlane(toSphere(a), toPlane(b), res);

            if (aType == plane && bType == sphere)
                SphereOnPlane(toSphere(b), toPlane(a), res);

            if (aType == box && bType == plane)
                BoxOnPlane(toBox(a), toPlane(b), res);

            if (aType == plane && bType == box)
                BoxOnPlane(toBox(b), toPlane(a), res);

            return res;
            //if (aType == typeof (SphereCollider) && bType == typeof (SphereCollider))
            //{
            //    SphereOnSphere(a as SphereCollider, b as SphereCollider, collisionData);
            //    return collisionData;
            //}

            throw new NotImplementedException();

            //if (aType == typeof (BoxCollider) && bType == typeof (BoxCollider))
            //{
            //    return BoxOnBox(a as BoxCollider, b as BoxCollider);
            //}
            //if (aType == typeof(BoxCollider) && bType == typeof(SphereCollider))
            //{
            //    return SphereOnBox(b as SphereCollider, a as BoxCollider);
            //}
            //if (aType == typeof(SphereCollider) && bType == typeof(BoxCollider))
            //{
            //    return SphereOnBox(a as SphereCollider, b as BoxCollider);
            //}
        }

        public static bool SphereOnPlane(SphereCollider a, PlaneCollider b, CollisionResult result)
        {
            var dist = b.Normal.Dot(a.Position) - a.Radius - b.Offset;
            if (dist >= 0) return false; // early out, no collision

            // TODO: remember, the plane is thought of as an infinately huge wall behind the normal.
            // TODO: to make it a thin page, we need to adjust the normal. Check the book, yooooooo.
            var c = new Contact(
                a.Position - b.Normal * (dist + a.Radius),
                b.Normal,
                -dist);

            c.bodyA = a.Body;
            c.bodyB = b.Body;

            result.AddContact(c);
            return true;
        }

        public static bool BoxOnPlane(BoxCollider a, PlaneCollider b, CollisionResult result)
        {
            // TODO add a pre check for early out options. 

            // TODO add rotation helper in. 
            // need to check all points. 
            var points = new Vector3[]
            {
                 new Vector3(1, 1, 1),
                 new Vector3(-1, 1, 1),
                 new Vector3(1, -1, 1),
                 new Vector3(-1, -1, 1),
                 new Vector3(1, 1, -1),
                 new Vector3(-1, 1, -1),
                 new Vector3(1, -1, -1),
                 new Vector3(-1, -1, -1),
            };

            var transformMatrix = Matrix.CreateFromQuaternion(a.Rotation);
            //transformMatrix = Matrix.CreateFromQuaternion(Quaternion.Identity);
            for (var i = 0; i < points.Length; i++)
            {
                var vert = Vector3.Transform(points[i] * a.Size/2, transformMatrix) + a.Position;
                var dist = b.Normal.Dot(vert);

                if (dist <= b.Offset)
                {
                    var c = new Contact(
                        vert + b.Normal*(dist - b.Offset)/2,
                        b.Normal,
                        b.Offset - dist);
                    c.bodyA = a.Body;
                    c.bodyB = b.Body;

                    result.AddContact(c);
                }
            }

            return result.AnyContact;
        }

        public static bool SphereOnSphere(SphereCollider a, SphereCollider b, CollisionResult result)
        {
            
            var diff = (b.Position - a.Position); // towards b.
            var dist = diff.Length();

            if (dist < 0 || dist > a.Radius + b.Radius)
            {
                return false; // there is no collision happening. This is an early out, and nothing has been put into _result_
            }

            // the sphere is colliding. We need to add a contact, and return true.
            var c= new Contact(
                a.Position + diff/2,
                diff.Normal(),
                (a.Radius + b.Radius) - dist);
            c.bodyA = a.Body;
            c.bodyB = b.Body;
            result.AddContact(c);
            return true;

        }

        public static CollisionResult SphereOnBox(SphereCollider a, BoxCollider b)
        {
            throw new NotImplementedException();            
        }

        public static CollisionResult BoxOnBox(BoxCollider a, BoxCollider b)
        {
            throw new NotImplementedException();
        }
    }
}
