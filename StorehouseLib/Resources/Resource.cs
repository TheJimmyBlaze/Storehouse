using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorehouseLib.Resources
{
    public class Resource
    {
        public readonly Guid ID;
        public readonly string Name;
        public readonly Resource Parent;

        public int Max { get; private set; }

        public int NumParents
        {
            get
            {
                int numParents = 0;
                if (Parent != null)
                {
                    numParents++;
                    numParents += Parent.NumParents;
                }

                return numParents;
            }
        }

        public Resource(string name, Resource parent)
        {
            ID = Guid.NewGuid();

            Name = name;
            Parent = parent;
        }
    }
}
