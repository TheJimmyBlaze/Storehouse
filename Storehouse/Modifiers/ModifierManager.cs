using Storehouse.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storehouse.Modifiers
{
    public class ModifierManager
    {
        public List<ModifierDuration> ModifierDurations { get; set; } = new List<ModifierDuration>();

        public ModifierManager() { }

        public ModifierManager(List<ModifierDuration> modifierDurations)
        {
            ModifierDurations = modifierDurations;
        }

        public ModifierManager(List<ModifierDuration> modifierDurations, ModifierRegistry modifierRegistry)
        {
            ModifierDurations = new List<ModifierDuration>();

            foreach(ModifierDuration modifierDuration in modifierDurations)
            {
                try
                {
                    Modifier modifier = modifierRegistry.GetModifier(modifierDuration.Modifier.name);
                    ModifierDurations.Add(new ModifierDuration(modifier, modifierDuration.ExpirationTimeUTC));
                }
                catch (ArgumentException) { }
            }
        }

        internal Modifier AddModifier(Modifier modifier)
        {
            ModifierDurations.RemoveAll(x => x.Modifier.id == modifier.id);
            ModifierDurations.Add(new ModifierDuration(modifier));

            return modifier;
        }

        public ModifierDuration GetModifierDuration(Guid id)
        {
            ModifierDuration modifierDuration = ModifierDurations.SingleOrDefault(x => x.Modifier.id == id);
            if (modifierDuration == null)
                throw new ArgumentException(string.Format("ModifierDuration could not be found with ID: {0}", id));

            return modifierDuration;
        }

        public List<ModifierDuration> GetModifierDurations(Guid resourceID)
        {
            return ModifierDurations.Where(x => x.Modifier.resource.id == resourceID).ToList();
        }

        internal double GetModifiedAmount(ResourceAmount startingAmount, ResourceCheckpoint lastCheckpoint)
        {
            Resource resource = startingAmount.Resource;

            double modifiedAmount = startingAmount.Count;
            foreach(ModifierDuration modifier in GetModifierDurations(resource.id))
            {
                double percentageModifiedTime = 1d;

                if (modifier.ExpirationTimeUTC is DateTime expirationTime)
                {
                    if (expirationTime < DateTime.UtcNow)
                    {
                        TimeSpan modifiedTime = expirationTime - lastCheckpoint.CheckpointTimeUTC;
                        TimeSpan totalTime = DateTime.UtcNow - lastCheckpoint.CheckpointTimeUTC;
                        percentageModifiedTime = modifiedTime.TotalSeconds / totalTime.TotalSeconds;
                    }
                }

                double amountToBeModified = startingAmount.Count * percentageModifiedTime;
                modifiedAmount += amountToBeModified * modifier.Modifier.percentageIncrease;
            }

            return modifiedAmount;
        }

        public void RemoveExpiredModifiers()
        {
            ModifierDurations.RemoveAll(x => x.ExpirationTimeUTC < DateTime.UtcNow);
        }
    }
}
