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
        public readonly FactoryRegistry factoryRegistry;

        private State State { get; set; }
        private readonly IStatePersister statePersister;

        public FactoryManager FactoryManager { get { return State.FactoryManager; } }
        public ResourceCheckpoint LastCheckpoint { get { return State.LastCheckpoint; } }

        public Store(IStatePersister statePersister)
        {
            resourceRegistry = new ResourceRegistry();
            factoryRegistry = new FactoryRegistry();

            this.statePersister = statePersister;
            State = new State(statePersister);
        }

        public void Save()
        {
            State.Save();
        }

        public void Load()
        {
            State = State.Load(statePersister, resourceRegistry, factoryRegistry);
        }

        public void InitializeCheckpoint(List<ResourceAmount> startingResources)
        {
            State.LastCheckpoint = new ResourceCheckpoint(startingResources);
        }

        public void InitializeFactoryManager(List<FactoryAmount> startingFactories)
        {
            State.FactoryManager = new FactoryManager(startingFactories);
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

                Factory newFactory = State.FactoryManager.AddFactory(factory);
                UpdateCheckpoint(new ResourceCheckpoint(GetResourceAmounts()));

                return newFactory;
            }
            return null;
        }

        public Factory LoadFactory(Factory factory)
        {
            return State.FactoryManager.AddFactory(factory);
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
            State.LastCheckpoint = checkpoint;
            CheckpointUpdateEventArgs e = new CheckpointUpdateEventArgs() { Checkpoint = LastCheckpoint };
            CheckpointUpdateEventHandler?.Invoke(this, e);
        }

        public List<ResourceAmount> GetResourceAmounts()
        {
            List<ResourceAmount> resourceAmounts = LastCheckpoint.ResourceAmounts;
            Dictionary<Guid, double> resourceDictionary = resourceAmounts.ToDictionary(x => x.Resource.id, x => x.Count);

            resourceDictionary = State.FactoryManager.Produce(LastCheckpoint, resourceDictionary);

            return resourceDictionary.Select(x => new ResourceAmount(resourceRegistry.GetResource(x.Key), x.Value)).ToList();
        }
    }
}
