using StorehouseLib.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorehouseLib
{
    public class Checkpoint
    {
        public DateTime CheckpointTimeUTC { get; set; }
        private List<ResourceAmount> resourceTotals;
        public List<ResourceAmount> ResourceTotals
        {
            get
            {
                if (resourceTotals == null)
                    return null;

                ResourceAmount[] copy = new ResourceAmount[resourceTotals.Count];
                resourceTotals.CopyTo(copy);
                return copy.ToList();
            }
            set
            {
                resourceTotals = value;
            }
        }

        public Checkpoint() { }

        public Checkpoint(List<ResourceAmount> resourceTotals)
        {
            CheckpointTimeUTC = DateTime.UtcNow;
            this.resourceTotals = resourceTotals;
        }

        public Checkpoint(DateTime checkpointTimeUTC, List<ResourceAmount> resourceTotals, ResourceRegistry resourceRegistry)
        {
            CheckpointTimeUTC = checkpointTimeUTC;
            this.resourceTotals = new List<ResourceAmount>();

            foreach(ResourceAmount resourceCollection in resourceTotals)
            {
                try
                {
                    Resource resource = resourceRegistry.GetResource(resourceCollection.Resource.Name);
                    this.resourceTotals.Add(new ResourceAmount(resource, resourceCollection.Count));
                } catch (ArgumentException) { }
            }            
        }
    }
}
