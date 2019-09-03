using Storehouse.Factories;
using Storehouse.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storehouse
{
    public class State
    {
        public ResourceCheckpoint LastCheckpoint { get; set; }
        public FactoryManager FactoryManager { get; set; }

        private readonly IStatePersister persister;

        public State(IStatePersister persister)
        {
            this.persister = persister;
        }

        public State(IStatePersister persister, ResourceCheckpoint lastCheckpoint, FactoryManager factoryManager)
        {
            this.persister = persister;

            LastCheckpoint = lastCheckpoint;
            FactoryManager = factoryManager;
        }

        public void Save()
        {
            persister.Save(LastCheckpoint, FactoryManager);
        }

        public static State Load(IStatePersister persister, ResourceRegistry resourceRegistry, FactoryRegistry factoryRegistry)
        {
            return persister.Load(resourceRegistry, factoryRegistry);
        }
    }
}
