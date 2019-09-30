﻿using Newtonsoft.Json;
using Storehouse.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storehouse.Buffs
{
    public class Buff
    {
        [JsonIgnore] public readonly Guid id;
        [JsonIgnore] public readonly Resource resource;
        [JsonIgnore] public readonly double percentageIncrease;
        [JsonIgnore] public readonly TimeSpan duration;
        public readonly string name;

        public Buff(string name, Resource resource, double percentageIncrease, TimeSpan duration)
        {
            id = Guid.NewGuid();

            this.name = name;
            this.resource = resource;
            this.percentageIncrease = percentageIncrease;
            this.duration = duration;
        }
    }
}
