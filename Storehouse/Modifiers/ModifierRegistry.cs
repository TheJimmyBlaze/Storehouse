using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storehouse.Modifiers
{
    public class ModifierRegistry
    {
        private readonly Dictionary<Guid, Modifier> modifiers = new Dictionary<Guid, Modifier>();
        public List<Modifier> Modifiers { get { return modifiers.Values.ToList(); } }

        internal Modifier RegisterModifier(Modifier modifier)
        {
            if (modifiers.Where(x => x.Value.name == modifier.name).Count() > 0)
                throw new ArgumentException(string.Format("A modifier already exists with the Name: {0}", modifier.name));

            modifiers.Add(modifier.id, modifier);

            return modifier;
        }

        public Modifier GetModifier(Guid id)
        {
            modifiers.TryGetValue(id, out Modifier modifier);
            if (modifier == null)
                throw new ArgumentException(string.Format("Modifier could not be found with ID: {0}", id));

            return modifier;
        }

        public Modifier GetModifier(string name)
        {
            Modifier modifier = modifiers.SingleOrDefault(x => x.Value.name == name).Value;
            if (modifier == null)
                throw new ArgumentException(string.Format("Factory could not be found with the Name: {0}", name));

            return modifier;
        }
    }
}
