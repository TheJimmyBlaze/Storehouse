using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storehouse.Factories
{
    public class FactoryRegistry
    {
        private Dictionary<Guid, Factory> factories = new Dictionary<Guid, Factory>();
        public List<Factory> Factories { get { return factories.Values.ToList(); } }

        internal Factory RegisterFactory(Factory factory)
        {
            if (factories.SingleOrDefault(x => x.Value.name == factory.name).Value != null)
                throw new ArgumentException(string.Format("A factory already exists with the Name: {0}", factory.name));

            factories.Add(factory.id, factory);
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

        public Factory GetFactory(string name)
        {
            Factory factory = factories.SingleOrDefault(x => x.Value.name == name).Value;
            if (factory == null)
                throw new ArgumentException(string.Format("Factory could not be found with the Name: {0}", name));

            return factory;
        }

        private void SortFactories()
        {
            factories = factories.OrderBy(x => x.Value.MaxConsumedResourceParentNum)
                                .ThenBy(x => x.Value.ConsumerCount)
                                .ThenBy(x => x.Value.ProviderCount)
                                .ToDictionary(x => x.Key, x => x.Value);
        }
    }
}
