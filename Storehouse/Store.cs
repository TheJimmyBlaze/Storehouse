using Storehouse.Factories;
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
        #region Event Handlers
        public class CheckpointUpdateEventArgs: EventArgs
        {
            public ResourceCheckpoint Checkpoint { get; set; }
        }
        public EventHandler<CheckpointUpdateEventArgs> CheckpointUpdateEventHandler;
        #endregion

        public readonly ResourceRegistry resourceRegistry;

        internal readonly State state;

        public FactoryManager FactoryManager { get { return state.factoryManager; } }
        public ResourceCheckpoint LastCheckpoint { get { return state.LastCheckpoint; } }

        public Store(IStatePersister statePersister)
        {
            resourceRegistry = new ResourceRegistry();
            state = new State(statePersister);
        }

        public void InitializeCheckpoint(List<ResourceAmount> startingResources)
        {
            state.LastCheckpoint = new ResourceCheckpoint(startingResources);
        }

        public Resource RegisterResource(Resource resource)
        {
            return resourceRegistry.RegisterResource(resource);
        }

        public Factory AddFactory(Factory factory)
        {
            bool canAfford = true;
            foreach(ResourceAmount resource in factory.cost)
            {
                if (!ConsumeResource(resource, true))
                    canAfford = false;
            }

            if (canAfford)
            {
                foreach (ResourceAmount resource in factory.cost)
                    ConsumeResource(resource, false);

                Factory newFactory = state.factoryManager.AddFactory(factory);
                UpdateCheckpoint(new ResourceCheckpoint(GetResourceAmounts()));

                return newFactory;
            }
            return null;
        }

        public Factory LoadFactory(Factory factory)
        {
            return state.factoryManager.AddFactory(factory);
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
            state.LastCheckpoint = checkpoint;
            CheckpointUpdateEventArgs e = new CheckpointUpdateEventArgs() { Checkpoint = LastCheckpoint };
            CheckpointUpdateEventHandler?.Invoke(this, e);
        }

        public List<ResourceAmount> GetResourceAmounts()
        {
            List<ResourceAmount> resourceAmounts = LastCheckpoint.ResourceAmounts;
            Dictionary<Guid, double> resourceDictionary = resourceAmounts.ToDictionary(x => x.Resource.id, x => x.Count);

            resourceDictionary = state.factoryManager.Produce(LastCheckpoint, resourceDictionary);

            return resourceDictionary.Select(x => new ResourceAmount(resourceRegistry.GetResource(x.Key), x.Value)).ToList();
        }
    }
}
