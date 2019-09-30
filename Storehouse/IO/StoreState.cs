using Storehouse.Buffs;
using Storehouse.Factories;
using Storehouse.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storehouse.IO
{
    public class StoreSaveState
    {
        public ResourceCheckpoint ResourceCheckpoint { get; set; }
        public FactoryManager FactoryManager { get; set; }
        public BuffManager BuffManager { get; set; }
    }
}
