using Newtonsoft.Json;
using Storehouse;
using Storehouse.Modifiers;
using Storehouse.Factories;
using Storehouse.IO;
using Storehouse.Resources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Storehouse.Store;

namespace Test
{
    class Program
    {
        public static Resource Worker;
        public static Resource Wood;
        public static Resource Stone;
        public static Resource Wheat;
        public static Resource Planks;
        public static Resource Blocks;
        public static Resource Bread;

        public static Factory TownHall;
        public static Factory ForestryCamp;
        public static Factory Farm;
        public static Factory LumberMill;
        public static Factory Bakery;

        public static Modifier BonusWood;
        public static Modifier UberWood;

        public static Store Store { get; set; }

        static void Main(string[] args)
        {
            Store = new Store(new JsonStoreIO(null));

            Worker = Store.RegisterResource(new Resource("Worker", null));
            Wood = Store.RegisterResource(new Resource("Wood", null));
            Stone = Store.RegisterResource(new Resource("Stone", null));
            Wheat = Store.RegisterResource(new Resource("Wheat", null));
            Planks = Store.RegisterResource(new Resource("Planks", Wood));
            Blocks = Store.RegisterResource(new Resource("Blocks", Stone));
            Bread = Store.RegisterResource(new Resource("Bread", Wheat));

            TownHall = RegisterTownHall();
            ForestryCamp = RegisterForestryCamp();
            Farm = RegisterFarm();
            LumberMill = RegisterLumberMill();
            Bakery = RegisterBakery();

            BonusWood = Store.RegisterModifier(new Modifier("Bonus Wood", Wood, 10, null));
            UberWood = Store.RegisterModifier(new Modifier("Uber Wood", Wood, 100, new TimeSpan(0, 0, 5)));

            try
            {
                Store.Load();
            }
            catch (FileNotFoundException)
            {
                SetStartingResource();
            }

            Task.Run(() => Print(Store));

            while (true)
            {
                ConsoleKeyInfo input = Console.ReadKey(true);

                switch (input.KeyChar)
                {
                    case 't':
                        Store.AddFactory(TownHall);
                        break;
                    case 'w':
                        Store.AddFactory(ForestryCamp);
                        break;
                    case 'f':
                        Store.AddFactory(Farm);
                        break;
                    case 'l':
                        Store.AddFactory(LumberMill);
                        break;
                    case 'b':
                        Store.AddFactory(Bakery);
                        break;
                    case 'p':
                        Store.ProvideResource(new ResourceAmount(Worker, 5));
                        break;
                    case 'o':
                        Store.AddModifier(BonusWood);
                        break;
                    case 'i':
                        Store.AddModifier(UberWood);
                        break;
                }
            }
        }

        private static void SetStartingResource()
        {
            List<ResourceAmount> startingResouces = new List<ResourceAmount>
            {
                new ResourceAmount(Worker, 3d),
                new ResourceAmount(Bread, 50d),
                new ResourceAmount(Wood, 100d)
            };
            Store.InitializeCheckpoint(startingResouces);

            List<FactoryAmount> startingFactories = new List<FactoryAmount>
            {
                new FactoryAmount(TownHall, 1),
                new FactoryAmount(ForestryCamp, 2),
                new FactoryAmount(Farm, 3)
            };
            Store.InitializeFactoryManager(startingFactories);

            List<ModifierDuration> startingModifiers = new List<ModifierDuration> { };
            Store.InitializeModifierManager(startingModifiers);

            Store.Save();
        }

        private static void Print(Store storehouse)
        {
            DateTime lastPrint = DateTime.UtcNow;
            while (true)
            {
                while (lastPrint.AddMilliseconds(1000/60) > DateTime.UtcNow)
                    Thread.Sleep(50);

                Console.CursorTop = 0;
                Console.CursorLeft = 0;

                List<ResourceAmount> resourceAmounts = storehouse.GetResourceAmounts();

                Console.WriteLine("Resources:".PadRight(24));
                foreach (ResourceAmount resourceCollection in resourceAmounts)
                {
                    Console.WriteLine("{0}: {1}", resourceCollection.Resource.name, resourceCollection.Count.ToString("0.0").PadRight(16));
                }

                Console.WriteLine("".PadRight(24));
                Console.WriteLine("Factories:".PadRight(24));
                foreach (FactoryAmount factoryAmount in storehouse.FactoryManager.FactoryAmounts)
                {
                    Console.WriteLine("{0}: {1}", factoryAmount.Factory.name, factoryAmount.Count.ToString("0.0").PadRight(16));
                }

                Console.WriteLine("".PadRight(24));
                Console.WriteLine("Modifiers:".PadRight(24));
                foreach(ModifierDuration modifierDuration in storehouse.ModifierManager.ModifierDurations.Where(x => x.ExpirationTimeUTC == null || x.ExpirationTimeUTC > DateTime.UtcNow))
                {
                    string secondsUntilExpiration = "static";
                    if (modifierDuration.ExpirationTimeUTC is DateTime expiration)
                        secondsUntilExpiration = (expiration - DateTime.UtcNow).TotalSeconds + "s";

                    Console.WriteLine("{0}: {1}", modifierDuration.Modifier.name, secondsUntilExpiration);
                }

                for(int i = 0; i < 5; i++)
                {
                    Console.WriteLine("".PadRight(24));
                }

                lastPrint = DateTime.UtcNow;
            }
        }

        #region Factory Creators
        private static Factory RegisterTownHall()
        {
            List<ResourceAmount> cost = new List<ResourceAmount>()
            {
                new ResourceAmount(Planks, 200d),
                new ResourceAmount(Bread, 150d)
            };

            List<Consumer> consumers = new List<Consumer>() { new Consumer(Bread, 0.1d) };
            List<Provider> providers = new List<Provider>() { new Provider(Worker, 0.01d) };

            Factory townHall = new Factory("Town Hall", cost, consumers, providers);
            return Store.RegisterFactory(townHall);
        }

        private static Factory RegisterForestryCamp()
        {
            List<ResourceAmount> cost = new List<ResourceAmount>()
            {
                new ResourceAmount(Wood, 50d),
                new ResourceAmount(Worker, 1d)
            };

            List<Consumer> consumers = new List<Consumer>() { };
            List<Provider> providers = new List<Provider>() { new Provider(Wood, 0.2d) };

            Factory forestryCamp = new Factory("Forestry Camp", cost, consumers, providers);
            return Store.RegisterFactory(forestryCamp);
        }

        private static Factory RegisterFarm()
        {
            List<ResourceAmount> cost = new List<ResourceAmount>()
            {
                new ResourceAmount(Wood, 100d),
                new ResourceAmount(Worker, 1d)
            };

            List<Consumer> consumers = new List<Consumer>() { };
            List<Provider> providers = new List<Provider>() { new Provider(Wheat, 0.05d) };

            Factory farm = new Factory("Farm", cost, consumers, providers);
            return Store.RegisterFactory(farm);
        }

        private static Factory RegisterLumberMill()
        {
            List<ResourceAmount> cost = new List<ResourceAmount>()
            {
                new ResourceAmount(Wood, 100d),
                new ResourceAmount(Worker, 2d)
            };

            List<Consumer> consumers = new List<Consumer>() { new Consumer(Wood, 0.1d) };
            List<Provider> providers = new List<Provider>() { new Provider(Planks, 0.5d) };

            Factory lumberMill = new Factory("Lumber Mill", cost, consumers, providers);
            return Store.RegisterFactory(lumberMill);
        }

        private static Factory RegisterBakery()
        {
            List<ResourceAmount> cost = new List<ResourceAmount>()
            {
                new ResourceAmount(Planks, 50d),
                new ResourceAmount(Worker, 1d)
            };

            List<Consumer> consumers = new List<Consumer>() { new Consumer(Wheat, 0.5d), new Consumer(Wood, 0.01d) };
            List<Provider> providers = new List<Provider>() { new Provider(Bread, 0.25d) };

            Factory bakery = new Factory("Bakery", cost, consumers, providers);
            return Store.RegisterFactory(bakery);
        }
        #endregion
    }
}
