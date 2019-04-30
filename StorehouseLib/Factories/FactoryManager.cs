using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorehouseLib.Factories
{
    public class FactoryManager
    {
        private Dictionary<Guid, Factory> factories = new Dictionary<Guid, Factory>();
        public List<Guid> Factories { get { return factories.Keys.ToList(); } }
        
        internal Factory RegisterNewFactory(Factory factory)
        {
            factories.Add(factory.ID, factory);
            SortFactories();

            return factory;
        }

        public Factory GetFactory(Guid id)
        {
            factories.TryGetValue(id, out Factory factory);
            if (factory == null)
                throw new ArgumentException(string.Format("Factory could not be found with ID: {0}", id));

            return factory;
        }

        private void SortFactories()
        {
            factories = factories.OrderBy(x => x.Value.MaxConsumedResourceParentNum).ToDictionary(x => x.Key, x => x.Value);
        }
    }
}
