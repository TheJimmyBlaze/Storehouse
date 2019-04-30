using StorehouseLib.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorehouseLib.Factories
{
    internal class Provider
    {
        public readonly Resource Resource;
        public double ProvisionPerSecond { get; private set; }

        public Provider(Resource resource, double provisionPerSecond)
        {
            Resource = resource;
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
