using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using DCG.Framework.Util;
using Microsoft.Xna.Framework;

namespace DCG.Framework.Physics.Colliders
{
    public abstract class AbsCollider
    {
        public CollisionResult CheckCollision(AbsCollider other)
        {
            return ColliderChecks.Check(this, other);
        }
    }

   
    public class SphereCollider : AbsCollider
    {
        public Vector3 Position { get; set; }
        public float Radius { get; set; }
    }

    public class BoxCollider : AbsCollider
    {
        public Vector3 Position { get; set; }
        public Vector3 Size { get; set; }
    }

    public class PlaneCollider : AbsCollider
    {
        public Vector3 Position { get; set; }
        public Vector3 Normal { get; set; }
    }

    public class ColliderChecks
    {
        static ColliderChecks() // on the static constructor, create all the possible type combos, and what methods they go to.
        {
            _registeredColliderTypes = new List<Type>();
            AddColliderType<SphereCollider>();
            AddColliderType<PlaneCollider>();

            AddMethod<SphereOnBox(SphereOnSphere);
        }


        #region book keeping
        private static List<Type> _registeredColliderTypes;
        private delegate bool ColliderMethod<A, B>(A a, B b, CollisionResult result) where A : AbsCollider where B : AbsCollider;

        private static void AddColliderType<T>() where T : AbsCollider
        {
            _registeredColliderTypes.Add(typeof(T));
        }

        private static void AddMethod<A, B>(ColliderMethod<A, B> mthd) where A : AbsCollider where B : AbsCollider
        {
            
        }

    
        private static string GetName(Type a, Type b)
        {
            return a.Name + "__" + b.Name;
        }

        #endregion


        public static CollisionResult Check(AbsCollider a, AbsCollider b)
        {
            var aType = a.GetType();
            var bType = b.GetType();

            var collisionData = new CollisionResult();

            if (aType == typeof (SphereCollider) && bType == typeof (SphereCollider))
            {
                SphereOnSphere(a as SphereCollider, b as SphereCollider, collisionData);
                return collisionData;
            }

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

        public static bool SphereOnSphere(SphereCollider a, SphereCollider b, CollisionResult result)
        {
            
            var diff = (b.Position - a.Position); // towards b.
            var dist = diff.Length();

            if (dist < 0 || dist > a.Radius + b.Radius)
            {
                return false; // there is no collision happening. This is an early out, and nothing has been put into _result_
            }

            // the sphere is colliding. We need to add a contact, and return true.
            result.AddContact(new Contact(
                a.Position + diff/2,
                diff.Normal(),
                (a.Radius + b.Radius) - dist));

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
