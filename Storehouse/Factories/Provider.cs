using Storehouse.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storehouse.Factories
{
    public class Provider
    {
        public readonly Resource resource;
        public double ProvisionPerSecond { get; private set; }

        public Provider(Resource resource, double provisionPerSecond)
        {
            this.resource = resource;
            ProvisionPerSecond = provisionPerSecond;
        }

        public double GetProvision(DateTime lastCheckpointUTC)
        {
            TimeSpan durationSinceCheckpoint = DateTime.UtcNow - lastCheckpointUTC;
            double provision = durationSinceCheckpoint.TotalSeconds * ProvisionPerSecond;
            return provision;
        }
    }
}
