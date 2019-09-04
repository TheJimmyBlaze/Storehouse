using Storehouse.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storehouse.Resources
{
    public class ResourceCheckpoint
    {
        public DateTime CheckpointTimeUTC { get; set; }

        private List<ResourceAmount> resourceAmounts;
        public List<ResourceAmount> ResourceAmounts
        {
            get
            {
                if (resourceAmounts == null)
                    return null;

                ResourceAmount[] copy = new ResourceAmount[resourceAmounts.Count];
                resourceAmounts.CopyTo(copy);
                return copy.ToList();
            }
            set
            {
                resourceAmounts = value;
            }
        }

        public ResourceCheckpoint() { }

        public ResourceCheckpoint(List<ResourceAmount> resourceAmounts)
        {
            CheckpointTimeUTC = DateTime.UtcNow;
            this.resourceAmounts = resourceAmounts;
        }

        public ResourceCheckpoint(DateTime checkpointTimeUTC, List<ResourceAmount> resourceAmounts, ResourceRegistry resourceRegistry)
        {
            CheckpointTimeUTC = checkpointTimeUTC;
            this.resourceAmounts = new List<ResourceAmount>();

            foreach(ResourceAmount resourceAmount in resourceAmounts)
            {
                try
                {
                    Resource resource = resourceRegistry.GetResource(resourceAmount.Resource.name);
                    this.resourceAmounts.Add(new ResourceAmount(resource, resourceAmount.Count));
                } catch (ArgumentException) { }
            }            
        }
    }
}
