using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storehouse.Resources
{
    public class Resource
    {
        public readonly Guid id;
        public readonly string name;
        public readonly Resource parent;

        public int Max { get; private set; }

        public int NumParents
        {
            get
            {
                int numParents = 0;
                if (parent != null)
                {
                    numParents++;
                    numParents += parent.NumParents;
                }

                return numParents;
            }
        }

        public Resource(string name, Resource parent)
        {
            id = Guid.NewGuid();

            this.name = name;
            this.parent = parent;
        }
    }
}
