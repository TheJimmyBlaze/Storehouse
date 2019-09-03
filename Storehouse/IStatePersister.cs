using Storehouse.Factories;
using Storehouse.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storehouse
{
    public interface IStatePersister
    {
        void Save(ResourceCheckpoint resourceCheckpoint, FactoryManager factoryManager);

        State Load();
    }
}
