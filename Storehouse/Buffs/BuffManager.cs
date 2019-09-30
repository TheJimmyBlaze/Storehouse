using Storehouse.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storehouse.Buffs
{
    public class BuffManager
    {
        public List<BuffDuration> BuffDurations { get; set; } = new List<BuffDuration>();

        public BuffManager() { }

        public BuffManager(List<BuffDuration> buffDurations)
        {
            BuffDurations = buffDurations;
        }

        public BuffManager(List<BuffDuration> buffDurations, BuffRegistry buffRegistry)
        {
            BuffDurations = new List<BuffDuration>();

            foreach(BuffDuration buffDuration in buffDurations)
            {
                try
                {
                    Buff buff = buffRegistry.GetBuff(buffDuration.Buff.name);
                    BuffDurations.Add(new BuffDuration(buff, buffDuration.ExpirationTimeUTC));
                }
                catch (ArgumentException) { }
            }
        }

        internal Buff AddBuff(Buff buff)
        {
            BuffDurations.RemoveAll(x => x.Buff.id == buff.id);
            BuffDurations.Add(new BuffDuration(buff));

            return buff;
        }

        public BuffDuration GetBuffDuration(Guid id)
        {
            BuffDuration buffDuration = BuffDurations.SingleOrDefault(x => x.Buff.id == id);
            if (buffDuration == null)
                throw new ArgumentException(string.Format("BuffDruation could not be found with ID: {0}", id));

            return buffDuration;
        }

        public List<BuffDuration> GetBuffDurations(Guid resourceID)
        {
            return BuffDurations.Where(x => x.Buff.resource.id == resourceID).ToList();
        }

        internal double GetBuffedAmount(ResourceAmount startingAmount, ResourceCheckpoint lastCheckpoint)
        {
            Resource resource = startingAmount.Resource;

            double buffedAmount = startingAmount.Count;
            foreach(BuffDuration buff in GetBuffDurations(resource.id))
            {
                double percentageBuffTime = 1d;

                if (buff.ExpirationTimeUTC < DateTime.UtcNow)
                {
                    TimeSpan buffedTime = buff.ExpirationTimeUTC - lastCheckpoint.CheckpointTimeUTC;
                    TimeSpan totalTime = DateTime.UtcNow - lastCheckpoint.CheckpointTimeUTC;
                    percentageBuffTime = buffedTime.TotalSeconds / totalTime.TotalSeconds;
                }

                double amountToBeBuffed = startingAmount.Count * percentageBuffTime;
                buffedAmount += amountToBeBuffed * buff.Buff.percentageIncrease;
            }

            return buffedAmount;
        }

        public void RemoveExpiredBuffs()
        {
            BuffDurations.RemoveAll(x => x.ExpirationTimeUTC < DateTime.UtcNow);
        }
    }
}
