using StorehouseLib.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorehouseLib.Factories
{
    internal class Consumer
    {
        public readonly Resource Resource;
        public double ConsumptionPerSecond { get; private set; }

        public Consumer(Resource resource, double consumptionPerSecond)
        {
            Resource = resource;
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
