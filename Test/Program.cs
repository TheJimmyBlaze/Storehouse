using Newtonsoft.Json;
using Storehouse;
using Storehouse.Factories;
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
        private const string CHECKPOINT_SAVE_PATH = "Checkpoint.json";
        private const string FACTORY_SAVE_PATH = "Factory.csv";

        public static Resource Worker;
        public static Resource Wood;
        public static Resource Stone;
        public static Resource Wheat;
        public static Resource Planks;
        public static Resource Blocks;
        public static Resource Bread;

        public static Store Store { get; set; }

        static void Main(string[] args)
        {
            Store = new Store(new JsonStatePersister(null));
            Store.CheckpointUpdateEventHandler += OnCheckpointUpdated;

            Worker = Store.RegisterResource(new Resource("Worker", null));
            Wood = Store.RegisterResource(new Resource("Wood", null));
            Stone = Store.RegisterResource(new Resource("Stone", null));
            Wheat = Store.RegisterResource(new Resource("Wheat", null));
            Planks = Store.RegisterResource(new Resource("Planks", Wood));
            Blocks = Store.RegisterResource(new Resource("Blocks", Stone));
            Bread = Store.RegisterResource(new Resource("Bread", Wheat));

            LoadCheckpoint();
            LoadFactories();

            if (Store.LastCheckpoint == null)
                SetStartingResource();

            Task.Run(() => Print(Store));

            while (true)
            {
                ConsoleKeyInfo input = Console.ReadKey(true);

                switch (input.KeyChar)
                {
                    case 't':
                        CreateTownHall();
                        break;
                    case 'w':
                        CreateForestryCamp();
                        break;
                    case 'f':
                        CreateFarm();
                        break;
                    case 'l':
                        CreateLumberMill();
                        break;
                    case 'b':
                        CreateBakery();
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
            SaveCheckpoint();

            CreateTownHall(true);
            SaveFactories();
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

                Console.WriteLine("Resources:");
                foreach (ResourceAmount resourceCollection in resourceAmounts)
                {
                    Console.WriteLine("{0}: {1}", resourceCollection.Resource.name, resourceCollection.Count.ToString("0.0").PadRight(16));
                }

                Console.WriteLine();
                Console.WriteLine("Factories:");
                foreach (Factory factory in storehouse.FactoryManager.Factories)
                {
                    Console.WriteLine("{0}: {1}", factory.name, string.Join(", ", factory.cost.Select(x => string.Format("{0} {1}", x.Count, x.Resource.name))));
                }

                lastPrint = DateTime.UtcNow;
            }
        }

        private static void OnCheckpointUpdated(object sender, CheckpointUpdateEventArgs e)
        {
            SaveCheckpoint();
            SaveFactories();
        }

        #region Factory Creators
        private static Factory CreateTownHall(bool fromLoad = false)
        {
            List<ResourceAmount> cost = new List<ResourceAmount>()
            {
                new ResourceAmount(Planks, 200d),
                new ResourceAmount(Bread, 150d)
            };

            List<Consumer> consumers = new List<Consumer>() { new Consumer(Bread, 0.1d) };
            List<Provider> providers = new List<Provider>() { new Provider(Worker, 0.01d) };

            Factory townHall = new Factory("Town Hall", cost, consumers, providers);

            if (fromLoad)
                return Store.LoadFactory(townHall);
            return Store.AddFactory(townHall);
        }

        private static Factory CreateForestryCamp(bool fromLoad = false)
        {
            List<ResourceAmount> cost = new List<ResourceAmount>()
            {
                new ResourceAmount(Wood, 50d),
                new ResourceAmount(Worker, 1d)
            };

            List<Consumer> consumers = new List<Consumer>() { };
            List<Provider> providers = new List<Provider>() { new Provider(Wood, 0.2d) };

            Factory forestryCamp = new Factory("Forestry Camp", cost, consumers, providers);
            forestryCamp.AddProvider(Wood, 0.2d);

            if (fromLoad)
                return Store.LoadFactory(forestryCamp);
            return Store.AddFactory(forestryCamp);
        }

        private static Factory CreateFarm(bool fromLoad = false)
        {
            List<ResourceAmount> cost = new List<ResourceAmount>()
            {
                new ResourceAmount(Wood, 100d),
                new ResourceAmount(Worker, 1d)
            };

            List<Consumer> consumers = new List<Consumer>() { };
            List<Provider> providers = new List<Provider>() { new Provider(Wheat, 0.05d) };

            Factory farm = new Factory("Farm", cost, consumers, providers);

            if (fromLoad)
                return Store.LoadFactory(farm);
            return Store.AddFactory(farm);
        }

        private static Factory CreateLumberMill(bool fromLoad = false)
        {
            List<ResourceAmount> cost = new List<ResourceAmount>()
            {
                new ResourceAmount(Wood, 100d),
                new ResourceAmount(Worker, 2d)
            };

            List<Consumer> consumers = new List<Consumer>() { new Consumer(Wood, 0.1d) };
            List<Provider> providers = new List<Provider>() { new Provider(Planks, 0.5d) };

            Factory lumberMill = new Factory("Lumber Mill", cost, consumers, providers);

            if (fromLoad)
                return Store.LoadFactory(lumberMill);
            return Store.AddFactory(lumberMill);
        }

        private static Factory CreateBakery(bool fromLoad = false)
        {
            List<ResourceAmount> cost = new List<ResourceAmount>()
            {
                new ResourceAmount(Planks, 50d),
                new ResourceAmount(Worker, 1d)
            };

            List<Consumer> consumers = new List<Consumer>() { new Consumer(Wheat, 0.5d), new Consumer(Wood, 0.01d) };
            List<Provider> providers = new List<Provider>() { new Provider(Bread, 0.25d) };

            Factory bakery = new Factory("Bakery", cost, consumers, providers);

            if (fromLoad)
                return Store.LoadFactory(bakery);
            return Store.AddFactory(bakery);
        }
        #endregion
    }
}
