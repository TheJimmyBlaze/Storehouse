using Storehouse.Factories;
using Storehouse.IO;
using Storehouse.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storehouse
{
    public class Store
    {
        public readonly ResourceRegistry resourceRegistry;
        public readonly FactoryRegistry factoryRegistry;

        private readonly IStoreIO storeIO;

        public ResourceCheckpoint ResourceCheckpoint { get; set; }
        public FactoryManager FactoryManager { get; set; }

        public Store(IStoreIO storeIO)
        {
            resourceRegistry = new ResourceRegistry();
            factoryRegistry = new FactoryRegistry();

            this.storeIO = storeIO;
        }

        public void Save()
        {
            StoreSaveState saveState = new StoreSaveState() { ResourceCheckpoint = ResourceCheckpoint, FactoryManager = FactoryManager };
            storeIO.Save(saveState);
        }

        public void Load()
        {
            StoreSaveState saveState = storeIO.Load(resourceRegistry, factoryRegistry);
            ResourceCheckpoint = saveState.ResourceCheckpoint;
            FactoryManager = saveState.FactoryManager;
        }

        public void InitializeCheckpoint(List<ResourceAmount> startingResources)
        {
            ResourceCheckpoint = new ResourceCheckpoint(startingResources);
        }

        public void InitializeFactoryManager(List<FactoryAmount> startingFactories)
        {
            FactoryManager = new FactoryManager(startingFactories);
        }

        public Resource RegisterResource(Resource resource)
        {
            return resourceRegistry.RegisterResource(resource);
        }

        public Factory RegisterFactory(Factory factory)
        {
            return factoryRegistry.RegisterFactory(factory);
        }

        public Factory AddFactory(Factory factory)
        {
            if (!factoryRegistry.Factories.Contains(factory))
                throw new ArgumentException("Factory: {0} not contained in the FactoryRegistry.", factory.name);

            bool canAfford = true;
            foreach(ResourceAmount resource in factory.cost)
            {
                if (!ConsumeResource(resource, true))
                {
                    canAfford = false;
                    break;
                }
            }

            if (canAfford)
            {
                foreach (ResourceAmount resource in factory.cost)
                    ConsumeResource(resource, false);

                Factory newFactory = FactoryManager.AddFactory(factory);
                UpdateCheckpoint(new ResourceCheckpoint(GetResourceAmounts()));

                return newFactory;
            }
            return null;
        }

        public Factory LoadFactory(Factory factory)
        {
            return FactoryManager.AddFactory(factory);
        }

        public bool ConsumeResource(ResourceAmount consumption, bool simulated)
        {
            List<ResourceAmount> currentAmounts = GetResourceAmounts();
            foreach(ResourceAmount currentAmount in currentAmounts)
            {
                if (currentAmount.Resource.id == consumption.Resource.id)
                {
                    if (currentAmount.Count >= consumption.Count)
                    {
                        if (!simulated)
                        {
                            currentAmount.Count -= consumption.Count;
                            UpdateCheckpoint(new ResourceCheckpoint(currentAmounts));
                        }
                        return true;
                    }
                }
            }
            return false;
        }

        private void UpdateCheckpoint(ResourceCheckpoint checkpoint)
        {
            ResourceCheckpoint = checkpoint;
            Save();
        }

        public List<ResourceAmount> GetResourceAmounts()
        {
            List<ResourceAmount> resourceAmounts = ResourceCheckpoint.ResourceAmounts;
            Dictionary<Guid, double> resourceDictionary = resourceAmounts.ToDictionary(x => x.Resource.id, x => x.Count);

            resourceDictionary = FactoryManager.Produce(ResourceCheckpoint, resourceDictionary);

            return resourceDictionary.Select(x => new ResourceAmount(resourceRegistry.GetResource(x.Key), x.Value)).ToList();
        }
    }
}
