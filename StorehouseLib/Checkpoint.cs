using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorehouseLib
{
    public class Checkpoint
    {
        public readonly DateTime CheckpointTimeUTC;
        private readonly Dictionary<Guid, double> resourceTotals;
        public Dictionary<Guid, double> ResourceTotals
        {
            get
            {
                Dictionary<Guid, double> returnValue = new Dictionary<Guid, double>();
                foreach(KeyValuePair<Guid, double> pair in resourceTotals)
                {
                    returnValue.Add(pair.Key, pair.Value);
                }
                return returnValue;
            }
        }

        public Checkpoint(Dictionary<Guid, double> resourceTotals)
        {
            CheckpointTimeUTC = DateTime.UtcNow;
            this.resourceTotals = resourceTotals;
        }
    }
}
