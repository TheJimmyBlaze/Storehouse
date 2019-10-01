using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storehouse.Modifiers
{
    public class ModifierDuration
    {
        public Modifier Modifier { get; set; }
        public DateTime? ExpirationTimeUTC { get; set; }

        public ModifierDuration() { }

        public ModifierDuration(Modifier modifier)
        {
            Modifier = modifier;

            if (Modifier.duration is TimeSpan durationSpan)
                ExpirationTimeUTC = DateTime.UtcNow + durationSpan;
        }

        public ModifierDuration(Modifier modifier, DateTime? expirationTime)
        {
            Modifier = modifier;
            ExpirationTimeUTC = expirationTime;
        }
    }
}
