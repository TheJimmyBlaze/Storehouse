using StorehouseLib.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorehouseLib.Factories
{
    public class Factory
    {
        public readonly Guid ID;
        public readonly string Name;

        private readonly List<Provider> providers = new List<Provider>();
        private readonly List<Consumer> consumers = new List<Consumer>();

        public Factory(string name)
        {
            ID = Guid.NewGuid();

            Name = name;
        }

        public int MaxConsumedResourceParentNum
        {
            get
            {
                int maxResourceParentNum = consumers.Count;
                foreach (Consumer consumer in consumers)
                    maxResourceParentNum = Math.Max(maxResourceParentNum, consumer.Resource.NumParents);

                return maxResourceParentNum;
            }
        }

        public bool DoesProduceResource(Guid resourceID)
        {
            return providers.SingleOrDefault(x => x.Resource.ID == resourceID) != null;
        }

        public bool DoesConsumeResource(Guid resourceID)
        {
            return consumers.SingleOrDefault(x => x.Resource.ID == resourceID) != null;
        }

        public void AddProvider(Resource resource, double provisionPerSecond)
        {
            if (DoesProduceResource(resource.ID))
                throw new ArgumentException(string.Format("Factory already produces resource: {0} ({1})", resource.Name, resource.ID));

            providers.Add(new Provider(resource, provisionPerSecond));
        }

        public void AddConsumer(Resource resource, double consumptionPerSecond)
        {
            if (DoesConsumeResource(resource.ID))
                throw new ArgumentException(string.Format("Factory already consumes resource: {0} ({1})", resource.Name, resource.ID));

            consumers.Add(new Consumer(resource, consumptionPerSecond));
        }

        public Dictionary<Guid, double> Produce(Checkpoint lastCheckpoint, Dictionary<Guid, double> resourceTotals)
        {
            DateTime checkpointTimeUTC = lastCheckpoint.CheckpointTimeUTC;

            double operationalSeconds = double.MaxValue;

            foreach(Consumer consumer in consumers){
                Resource resource = consumer.Resource;
                resourceTotals.TryGetValue(resource.ID, out double resourceCount);

                double potentialConsumption = consumer.GetConsumption(checkpointTimeUTC);
                double consumption = Math.Min(potentialConsumption, resourceCount);

                double consumerOperationalSeconds = consumption / consumer.ConsumptionPerSecond;
                operationalSeconds = Math.Min(operationalSeconds, consumerOperationalSeconds);
            }

            foreach(Consumer consumer in consumers)
            {
                double consumption = operationalSeconds * consumer.ConsumptionPerSecond;

                Resource resource = consumer.Resource;
                resourceTotals.TryGetValue(resource.ID, out double resourceCount);

                if (resourceTotals.ContainsKey(resource.ID))
                    resourceTotals[resource.ID] = resourceCount - consumption;
            }

            foreach(Provider provider in providers)
            {
                double provision;
                if (consumers.Count == 0)
                    provision = provider.GetProvision(checkpointTimeUTC);
                else
                    provision = operationalSeconds * provider.ProvisionPerSecond;

                Resource resource = provider.Resource;
                resourceTotals.TryGetValue(resource.ID, out double resourceCount);

                if (resourceTotals.ContainsKey(resource.ID))
                    resourceTotals[resource.ID] = resourceCount + provision;
                else
                    resourceTotals.Add(resource.ID, resourceCount + provision);
            }

            return resourceTotals;
        }
    }
}
