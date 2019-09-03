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
        private ResourceCheckpoint lastCheckpoint = null;
        internal ResourceCheckpoint LastCheckpoint
        {
            get
            {
                if (lastCheckpoint == null)
                    throw new Exception("Resource Checkpoint has not been instanciated, or loaded with any values.");
                return lastCheckpoint;
            }
            set { lastCheckpoint = value; }
        }

        internal readonly FactoryManager factoryManager;

        private readonly IStatePersister persister;

        public State(IStatePersister persister)
        {
            factoryManager = new FactoryManager();
            this.persister = persister;
        }

        public State(ResourceCheckpoint lastCheckpoint, FactoryManager factoryManager)
        {
            LastCheckpoint = lastCheckpoint;
            this.factoryManager = factoryManager;
        }

        public void Save()
        {
            persister.Save(lastCheckpoint, factoryManager);
        }

        public static State Load(IStatePersister persister)
        {
            return persister.Load();
        }
    }
}
