using Storehouse.Factories;
using Storehouse.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storehouse.IO
{
    public interface IStoreIO
    {
        void Save(StoreSaveState saveState);

        StoreSaveState Load(ResourceRegistry resourceRegistry, FactoryRegistry factoryRegistry);
    }
}
