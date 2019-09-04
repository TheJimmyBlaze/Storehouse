using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storehouse.Resources
{
    public class Resource
    {
        [JsonIgnore] public readonly Guid id;
        [JsonIgnore] public readonly Resource parent;
        public readonly string name;

        [JsonIgnore]
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
