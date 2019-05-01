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
        #region Event Handlers
        public class CheckpointUpdateEventArgs: EventArgs
        {
            public Checkpoint Checkpoint { get; set; }
        }
        public EventHandler<CheckpointUpdateEventArgs> CheckpointUpdateEventHandler;
        #endregion

        public readonly ResourceRegistry ResourceRegistry = new ResourceRegistry();
        public readonly FactoryManager FactoryManager = new FactoryManager();

        public Checkpoint LastCheckpoint { get; set; }

        public Storehouse() { }

        public void InitializeCheckpoint(List<ResourceAmount> startingResources)
        {
            LastCheckpoint = new Checkpoint(startingResources);
        }

        public Resource RegisterResource(Resource resource)
        {
            return ResourceRegistry.RegisterResource(resource);
        }

        public Factory RegisterFactory(Factory factory, bool fromLoad = false)
        {
            Factory registeredFactory = FactoryManager.AddFactory(factory);
            if (!fromLoad)
                UpdateCheckpoint(new Checkpoint(GetResourceAmounts()));

            return registeredFactory;
        }

        public bool ConsumeResource(ResourceAmount consumption, bool simulated)
        {
            List<ResourceAmount> currentAmounts = GetResourceAmounts();
            foreach(ResourceAmount currentAmount in currentAmounts)
            {
                if (currentAmount.Resource.ID == consumption.Resource.ID)
                {
                    if (currentAmount.Count >= consumption.Count)
                    {
                        if (!simulated)
                        {
                            currentAmount.Count -= consumption.Count;
                            UpdateCheckpoint(new Checkpoint(currentAmounts));
                        }
                        return true;
                    }
                }
            }
            return false;
        }

        private void UpdateCheckpoint(Checkpoint checkpoint)
        {
            LastCheckpoint = checkpoint;
            CheckpointUpdateEventArgs e = new CheckpointUpdateEventArgs() { Checkpoint = LastCheckpoint };
            CheckpointUpdateEventHandler?.Invoke(this, e);
        }

        public List<ResourceAmount> GetResourceAmounts()
        {
            List<ResourceAmount> resourceTotals = LastCheckpoint.ResourceTotals;
            Dictionary<Guid, double> resourceDictionary = resourceTotals.ToDictionary(x => x.Resource.ID, x => x.Count);

            foreach(Guid factoryID in FactoryManager.Factories)
            {
                Factory factory = FactoryManager.GetFactory(factoryID);
                resourceDictionary = factory.Produce(LastCheckpoint, resourceDictionary);
            }

            return resourceDictionary.Select(x => new ResourceAmount(ResourceRegistry.GetResource(x.Key), x.Value)).ToList();
        }
    }
}
