using StorehouseLib.Factories;
using StorehouseLib.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorehouseLib
{
    public class Storehouse
    {
        public readonly ResourceRegistry ResourceRegistry = new ResourceRegistry();
        public readonly FactoryManager FactoryManager = new FactoryManager();

        private Checkpoint lastCheckpoint;

        public Storehouse()
        {
            lastCheckpoint = new Checkpoint(new Dictionary<Guid, double>());
        }

        public Resource RegisterResource(Resource resource)
        {
            lastCheckpoint = new Checkpoint(GetResourceTotals());
            return ResourceRegistry.RegisterResource(resource);
        }

        public Factory RegisterFactory(Factory factory)
        {
            lastCheckpoint = new Checkpoint(GetResourceTotals());
            return FactoryManager.AddFactory(factory);
        }
        
        public Dictionary<Guid, double> GetResourceTotals()
        {
            Dictionary<Guid, double> resourceTotals = lastCheckpoint.ResourceTotals;

            foreach(Guid factoryID in FactoryManager.Factories)
            {
                Factory factory = FactoryManager.GetFactory(factoryID);
                resourceTotals = factory.Produce(lastCheckpoint, resourceTotals);
            }

            return resourceTotals;
        }
    }
}
