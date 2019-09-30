using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storehouse.Buffs
{
    public class BuffDuration
    {
        public Buff Buff { get; set; }
        public DateTime ExpirationTimeUTC { get; set; }

        public BuffDuration() { }

        public BuffDuration(Buff buff)
        {
            Buff = buff;
            ExpirationTimeUTC = DateTime.UtcNow + Buff.duration;
        }

        public BuffDuration(Buff buff, DateTime expirationTime)
        {
            Buff = buff;
            ExpirationTimeUTC = expirationTime;
        }
    }
}
