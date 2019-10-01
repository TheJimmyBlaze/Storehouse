using Storehouse.Modifiers;
using Storehouse.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storehouse.Factories
{
    public class FactoryAmount
    {
        public Factory Factory { get; set; }
        public int Count { get; set; }

        public FactoryAmount() { }

        public FactoryAmount(Factory factory, int count)
        {
            Factory = factory;
            Count = count;
        }

        public Dictionary<Guid, double> Produce(ResourceCheckpoint lastCheckpoint, Dictionary<Guid, double> resourceTotals, ModifierManager modifierManager)
        {
            for(int i = 0; i < Count; i++)
                resourceTotals = Factory.Produce(lastCheckpoint, resourceTotals, modifierManager);
            return resourceTotals;
        }
    }
}
