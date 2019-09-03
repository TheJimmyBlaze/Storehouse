using Storehouse.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storehouse.Factories
{
    public class Consumer
    {
        public readonly Resource resource;
        public double ConsumptionPerSecond { get; private set; }

        public Consumer(Resource resource, double consumptionPerSecond)
        {
            this.resource = resource;
            ConsumptionPerSecond = consumptionPerSecond;
        }

        public double GetConsumption(DateTime lastCheckpointUTC)
        {
            TimeSpan durationSinceCheckpoint = DateTime.UtcNow - lastCheckpointUTC;
            double consumption = durationSinceCheckpoint.TotalSeconds * ConsumptionPerSecond;
            return consumption;
        }
    }
}
