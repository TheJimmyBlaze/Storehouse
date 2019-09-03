using Storehouse.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storehouse.Factories
{
    public class FactoryManager
    {
        public Dictionary<Guid, FactoryAmount> FactoryAmounts { get; set; } = new Dictionary<Guid, FactoryAmount>();

        public FactoryManager() { }

        public FactoryManager(List<FactoryAmount> factoryAmounts)
        {
            FactoryAmounts = factoryAmounts.ToDictionary(x => x.Factory.id, x => x);
            SortFactoryAmounts();
        } 

        public FactoryManager(List<FactoryAmount> factoryAmounts, FactoryRegistry factoryRegistry)
        {
            FactoryAmounts = new Dictionary<Guid, FactoryAmount>();

            foreach (FactoryAmount factoryAmount in factoryAmounts)
            {
                try
                {
                    Factory factory = factoryRegistry.GetFactory(factoryAmount.Factory.name);
                    FactoryAmounts.Add(factory.id, new FactoryAmount(factory, factoryAmount.Count));
                }
                catch (ArgumentException) { }
            }
            SortFactoryAmounts();
        }

        internal Factory AddFactory(Factory factory)
        {
            if (!FactoryAmounts.ContainsKey(factory.id))
            {
                FactoryAmounts.Add(factory.id, new FactoryAmount(factory, 1));
                SortFactoryAmounts();

                return factory;
            }

            if (FactoryAmounts.TryGetValue(factory.id, out FactoryAmount factoryAmount))
                factoryAmount.Count++;

            return factory;
        }

        public FactoryAmount GetFactoryAmount(Guid id)
        {
            FactoryAmounts.TryGetValue(id, out FactoryAmount factory);
            if (factory == null)
                throw new ArgumentException(string.Format("FactoryAmount could not be found with ID: {0}", id));

            return factory;
        }

        private void SortFactoryAmounts()
        {
            FactoryAmounts = FactoryAmounts.OrderBy(x => x.Value.Factory.MaxConsumedResourceParentNum)
                                        .ThenBy(x => x.Value.Factory.ConsumerCount)
                                        .ThenBy(x => x.Value.Factory.ProviderCount)
                                        .ToDictionary(x => x.Key, x => x.Value);
        }

        public Dictionary<Guid, double> Produce(ResourceCheckpoint lastCheckpoint, Dictionary<Guid, double> resourceTotals)
        {
            foreach(FactoryAmount factoryAmount in FactoryAmounts.Values)
                resourceTotals = factoryAmount.Produce(lastCheckpoint, resourceTotals);
            return resourceTotals;
        }
    }
}
