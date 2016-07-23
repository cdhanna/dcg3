using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCG.Framework.Physics.Colliders
{
    public class CollisionResult
    {
        public Contact[] Contacts;
        public int ContactCount;

        public CollisionResult(int contactCount = 4)
        {
            Contacts = new Contact[contactCount];
            ContactCount = 0;
        }

        public bool AnyContact { get { return ContactCount != 0; } }

        public void AddContact(Contact c)
        {
            Contacts[ContactCount++] = c;
        }
    
    }
}
