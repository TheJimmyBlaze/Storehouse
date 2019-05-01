using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorehouseLib.Resources
{
    public class ResourceAmount
    {
        public Resource Resource { get; set; }
        public double Count { get; set; }

        public ResourceAmount() { }

        public ResourceAmount(Resource resource, double count)
        {
            Resource = resource;
            Count = count;
        }
    }
}
