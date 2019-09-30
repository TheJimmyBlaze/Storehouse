using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storehouse.Buffs
{
    public class BuffRegistry
    {
        private readonly Dictionary<Guid, Buff> buffs = new Dictionary<Guid, Buff>();
        public List<Buff> Buffs { get { return buffs.Values.ToList(); } }

        internal Buff RegisterBuff(Buff buff)
        {
            if (buffs.Where(x => x.Value.name == buff.name).Count() > 0)
                throw new ArgumentException(string.Format("A buff already exists with the Name: {0}", buff.name));

            buffs.Add(buff.id, buff);

            return buff;
        }

        public Buff GetBuff(Guid id)
        {
            buffs.TryGetValue(id, out Buff buff);
            if (buff == null)
                throw new ArgumentException(string.Format("Buff could not be found with ID: {0}", id));

            return buff;
        }

        public Buff GetBuff(string name)
        {
            Buff buff = buffs.SingleOrDefault(x => x.Value.name == name).Value;
            if (buff == null)
                throw new ArgumentException(string.Format("Factory could not be found with the Name: {0}", name));

            return buff;
        }
    }
}
