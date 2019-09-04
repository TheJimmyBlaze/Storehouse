using Newtonsoft.Json;
using Storehouse.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storehouse.Factories
{
    public class Factory
    {
        [JsonIgnore] public readonly Guid id;
        public readonly string name;

        [JsonIgnore] public readonly List<ResourceAmount> cost;

        [JsonIgnore] public readonly List<Consumer> consumers;
        [JsonIgnore] public readonly List<Provider> providers;

        [JsonIgnore] public int ConsumerCount { get { return consumers.Count; } }
        [JsonIgnore] public int ProviderCount { get { return providers.Count; } }

        public Factory(string name, List<ResourceAmount> cost, List<Consumer> consumers, List<Provider> providers)
        {
            id = Guid.NewGuid();
            this.name = name;

            this.cost = cost;
            this.consumers = consumers;
            this.providers = providers;
        }

        [JsonIgnore]
        public int MaxConsumedResourceParentNum
        {
            get
            {
                int maxResourceParentNum = 0;
                foreach (Consumer consumer in consumers)
                    maxResourceParentNum = Math.Max(maxResourceParentNum, consumer.resource.NumParents);

                return maxResourceParentNum;
            }
        }

        public bool DoesConsumeResource(Guid resourceID)
        {
            return consumers.SingleOrDefault(x => x.resource.id == resourceID) != null;
        }

        public bool DoesProduceResource(Guid resourceID)
        {
            return providers.SingleOrDefault(x => x.resource.id == resourceID) != null;
        }

        public void AddConsumer(Resource resource, double consumptionPerSecond)
        {
            if (DoesConsumeResource(resource.id))
                throw new ArgumentException(string.Format("Factory already consumes resource: {0} ({1})", resource.name, resource.id));

            consumers.Add(new Consumer(resource, consumptionPerSecond));
        }

        public void AddProvider(Resource resource, double provisionPerSecond)
        {
            if (DoesProduceResource(resource.id))
                throw new ArgumentException(string.Format("Factory already produces resource: {0} ({1})", resource.name, resource.id));

            providers.Add(new Provider(resource, provisionPerSecond));
        }

        internal Dictionary<Guid, double> Produce(ResourceCheckpoint lastCheckpoint, Dictionary<Guid, double> resourceTotals)
        {
            DateTime checkpointTimeUTC = lastCheckpoint.CheckpointTimeUTC;

            double operationalSeconds = double.MaxValue;

            foreach(Consumer consumer in consumers){
                Resource resource = consumer.resource;
                resourceTotals.TryGetValue(resource.id, out double resourceCount);

                double potentialConsumption = consumer.GetConsumption(checkpointTimeUTC);
                double consumption = Math.Min(potentialConsumption, resourceCount);

                double consumerOperationalSeconds = consumption / consumer.ConsumptionPerSecond;
                operationalSeconds = Math.Min(operationalSeconds, consumerOperationalSeconds);
            }

            //Shortcircuit if not operational for any ammount of time.
            if (operationalSeconds == 0)
                return resourceTotals;

            foreach(Consumer consumer in consumers)
            {
                double consumption = operationalSeconds * consumer.ConsumptionPerSecond;

                Resource resource = consumer.resource;
                resourceTotals.TryGetValue(resource.id, out double resourceCount);

                if (resourceTotals.ContainsKey(resource.id))
                    resourceTotals[resource.id] = resourceCount - consumption;
            }

            foreach(Provider provider in providers)
            {
                double provision;
                if (consumers.Count == 0)
                    provision = provider.GetProvision(checkpointTimeUTC);
                else
                    provision = operationalSeconds * provider.ProvisionPerSecond;

                Resource resource = provider.resource;
                resourceTotals.TryGetValue(resource.id, out double resourceCount);

                if (resourceTotals.ContainsKey(resource.id))
                    resourceTotals[resource.id] = resourceCount + provision;
                else
                    resourceTotals.Add(resource.id, resourceCount + provision);
            }

            return resourceTotals;
        }
    }
}
