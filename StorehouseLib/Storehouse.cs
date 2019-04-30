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
        public readonly FactoryManager FactoryRegistry = new FactoryManager();

        private Checkpoint lastCheckpoint;

        public Storehouse()
        {
            lastCheckpoint = new Checkpoint(new Dictionary<Guid, double>());
        }

        public Resource RegisterResource(Resource resource)
        {
            lastCheckpoint = new Checkpoint(GetResourceTotals());
            return ResourceRegistry.RegisterNewResource(resource);
        }

        public Factory RegisterFactory(Factory factory)
        {
            lastCheckpoint = new Checkpoint(GetResourceTotals());
            return FactoryRegistry.RegisterNewFactory(factory);
        }
        
        public Dictionary<Guid, double> GetResourceTotals()
        {
            Dictionary<Guid, double> resourceTotals = lastCheckpoint.ResourceTotals;

            foreach(Guid factoryID in FactoryRegistry.Factories)
            {
                Factory factory = FactoryRegistry.GetFactory(factoryID);
                resourceTotals = factory.Produce(lastCheckpoint, resourceTotals);
            }

            return resourceTotals;
        }
    }
}
