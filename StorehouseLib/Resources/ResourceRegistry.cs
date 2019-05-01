using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorehouseLib.Resources
{
    public class ResourceRegistry
    {
        private Dictionary<Guid, Resource> resources = new Dictionary<Guid, Resource>();
        public List<Guid> Resources { get { return resources.Keys.ToList(); } }

        internal Resource RegisterResource(Resource resource)
        {
            if (resources.SingleOrDefault(x => x.Value.Name == resource.Name).Value != null)
                throw new ArgumentException(string.Format("A resource already exists with Name: {0}", resource.Name));

            resources.Add(resource.ID, resource);
            SortResources();

            return resource;
        }

        public Resource GetResource(Guid id)
        {
            resources.TryGetValue(id, out Resource resource);
            if (resource == null)
                throw new ArgumentException(string.Format("Resource could not be found with ID: {0}", id));

            return resource;
        }

        public Resource GetResource(string name)
        {
            Resource resource = resources.SingleOrDefault(x => x.Value.Name == name).Value;
            if (resource == null)
                throw new ArgumentException(string.Format("Resource could not be found with Name: {0}", name));

            return resource;
        }

        private void SortResources()
        {
            resources = resources.OrderBy(x => x.Value.NumParents).ThenBy(x => x.Value.Name).ToDictionary(x => x.Key, x => x.Value);
        }
    }
}
