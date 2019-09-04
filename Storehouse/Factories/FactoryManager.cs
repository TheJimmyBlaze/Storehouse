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
        public List<FactoryAmount> FactoryAmounts { get; set; } = new List<FactoryAmount>();

        public FactoryManager() { }

        public FactoryManager(List<FactoryAmount> factoryAmounts)
        {
            FactoryAmounts = factoryAmounts;
            SortFactoryAmounts();
        } 

        public FactoryManager(List<FactoryAmount> factoryAmounts, FactoryRegistry factoryRegistry)
        {
            FactoryAmounts = new List<FactoryAmount>();

            foreach (FactoryAmount factoryAmount in factoryAmounts)
            {
                try
                {
                    Factory factory = factoryRegistry.GetFactory(factoryAmount.Factory.name);
                    FactoryAmounts.Add(new FactoryAmount(factory, factoryAmount.Count));
                }
                catch (ArgumentException) { }
            }
            SortFactoryAmounts();
        }

        internal Factory AddFactory(Factory factory)
        {
            if (FactoryAmounts.Where(x => x.Factory.id == factory.id).Count() == 0)
            {
                FactoryAmounts.Add(new FactoryAmount(factory, 1));
                SortFactoryAmounts();

                return factory;
            }

            FactoryAmount factoryAmount = FactoryAmounts.Single(x => x.Factory.id == factory.id);
            factoryAmount.Count++;

            return factory;
        }

        public FactoryAmount GetFactoryAmount(Guid id)
        {
            FactoryAmount factoryAmount = FactoryAmounts.SingleOrDefault(x => x.Factory.id == id);
            if (factoryAmount == null)
                throw new ArgumentException(string.Format("FactoryAmount could not be found with ID: {0}", id));

            return factoryAmount;
        }

        private void SortFactoryAmounts()
        {
            FactoryAmounts = FactoryAmounts.OrderBy(x => x.Factory.MaxConsumedResourceParentNum)
                                        .ThenBy(x => x.Factory.ConsumerCount)
                                        .ThenBy(x => x.Factory.ProviderCount).ToList();
        }

        public Dictionary<Guid, double> Produce(ResourceCheckpoint lastCheckpoint, Dictionary<Guid, double> resourceTotals)
        {
            foreach(FactoryAmount factoryAmount in FactoryAmounts)
                resourceTotals = factoryAmount.Produce(lastCheckpoint, resourceTotals);
            return resourceTotals;
        }
    }
}
