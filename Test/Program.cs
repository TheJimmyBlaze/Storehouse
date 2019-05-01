using Newtonsoft.Json;
using StorehouseLib;
using StorehouseLib.Factories;
using StorehouseLib.Resources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static StorehouseLib.Storehouse;

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

        public static Storehouse Storehouse { get; set; }

        static void Main(string[] args)
        {
            Storehouse = new Storehouse();
            Storehouse.CheckpointUpdateEventHandler += OnCheckpointUpdated;

            Worker = Storehouse.RegisterResource(new Resource("Worker", null));
            Wood = Storehouse.RegisterResource(new Resource("Wood", null));
            Stone = Storehouse.RegisterResource(new Resource("Stone", null));
            Wheat = Storehouse.RegisterResource(new Resource("Wheat", null));
            Planks = Storehouse.RegisterResource(new Resource("Planks", Wood));
            Blocks = Storehouse.RegisterResource(new Resource("Blocks", Stone));
            Bread = Storehouse.RegisterResource(new Resource("Bread", Wheat));

            LoadCheckpoint();
            LoadFactories();

            if (Storehouse.LastCheckpoint == null)
                SetStartingResource();

            Task.Run(() => Print(Storehouse));

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
            Storehouse.InitializeCheckpoint(startingResouces);
            SaveCheckpoint();

            CreateTownHall(true);
            SaveFactories();
        }

        private static void Print(Storehouse storehouse)
        {
            DateTime lastPrint = DateTime.UtcNow;
            while (true)
            {
                while (lastPrint.AddMilliseconds(1000/60) > DateTime.UtcNow)
                    Thread.Sleep(50);

                Console.CursorTop = 0;
                Console.CursorLeft = 0;

                List<ResourceAmount> resourceTotals = storehouse.GetResourceAmounts();

                foreach (ResourceAmount resourceCollection in resourceTotals)
                {
                    Console.WriteLine("{0}: {1}", resourceCollection.Resource.Name, resourceCollection.Count.ToString("0.0").PadRight(16));
                }

                lastPrint = DateTime.UtcNow;
            }
        }

        private static void SaveCheckpoint()
        {
            using (FileStream fileStream = new FileStream(CHECKPOINT_SAVE_PATH, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(fileStream))
                {
                    writer.Write(JsonConvert.SerializeObject(Storehouse.LastCheckpoint, Formatting.Indented));
                }
            }
        }

        private static void LoadCheckpoint()
        {
            try
            {
                using (FileStream fileStream = new FileStream(CHECKPOINT_SAVE_PATH, FileMode.Open))
                {
                    using (StreamReader reader = new StreamReader(fileStream))
                    {
                        Checkpoint loadedCheckpoint = JsonConvert.DeserializeObject<Checkpoint>(reader.ReadToEnd());
                        Storehouse.LastCheckpoint = new Checkpoint(loadedCheckpoint.CheckpointTimeUTC, loadedCheckpoint.ResourceTotals, Storehouse.ResourceRegistry);
                    }
                }
            }
            catch (FileNotFoundException) { }
        }

        private static void SaveFactories()
        {
            using (FileStream fileStream = new FileStream(FACTORY_SAVE_PATH, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(fileStream))
                {
                    foreach(Guid factoryID in Storehouse.FactoryManager.Factories)
                    {
                        string[] saveData = new string[]
                        {
                            Storehouse.FactoryManager.GetFactory(factoryID).Name
                        };
                        writer.WriteLine(string.Join(",", saveData));
                    }
                }
            }
        }

        private static void LoadFactories()
        {
            try
            {
                using (FileStream fileStream = new FileStream(FACTORY_SAVE_PATH, FileMode.Open))
                {
                    using (StreamReader reader = new StreamReader(fileStream))
                    {
                        while (!reader.EndOfStream)
                        {
                            string line = reader.ReadLine();
                            switch (line)
                            {
                                case "Town Hall":
                                    CreateTownHall(true);
                                    break;
                                case "Forestry Camp":
                                    CreateForestryCamp(true);
                                    break;
                                case "Farm":
                                    CreateFarm(true);
                                    break;
                                case "Lumber Mill":
                                    CreateLumberMill(true);
                                    break;
                                case "Bakery":
                                    CreateBakery(true);
                                    break;
                            }
                        }
                    }
                }
            }
            catch (FileNotFoundException) { }
        }

        private static void OnCheckpointUpdated(object sender, CheckpointUpdateEventArgs e)
        {
            SaveCheckpoint();
            SaveFactories();
        }

        #region Factory Creators
        private static void CreateTownHall(bool fromLoad = false)
        {
            Factory townHall = new Factory("Town Hall");
            townHall.AddConsumer(Bread, 0.1d);
            townHall.AddProvider(Worker, 0.01d);

            if (fromLoad)
            {
                Storehouse.RegisterFactory(townHall, fromLoad);
                return;
            }

            ResourceAmount plankCost = new ResourceAmount(Planks, 200d);
            ResourceAmount breadCost = new ResourceAmount(Bread, 150d);
            if (Storehouse.ConsumeResource(plankCost, true) && Storehouse.ConsumeResource(breadCost, true))
            {
                Storehouse.ConsumeResource(plankCost, false);
                Storehouse.ConsumeResource(breadCost, false);
                Storehouse.RegisterFactory(townHall);
            }
        }

        private static void CreateForestryCamp(bool fromLoad = false)
        {
            Factory forestryCamp = new Factory("Forestry Camp");
            forestryCamp.AddProvider(Wood, 0.2d);

            if (fromLoad)
            {
                Storehouse.RegisterFactory(forestryCamp, fromLoad);
                return;
            }

            ResourceAmount woodCost = new ResourceAmount(Wood, 50d);
            ResourceAmount workerCost = new ResourceAmount(Worker, 1d);
            if (Storehouse.ConsumeResource(woodCost, true) && Storehouse.ConsumeResource(workerCost, true))
            {
                Storehouse.ConsumeResource(woodCost, false);
                Storehouse.ConsumeResource(workerCost, false);
                Storehouse.RegisterFactory(forestryCamp);
            }
        }

        private static void CreateFarm(bool fromLoad = false)
        {
            Factory farm = new Factory("Farm");
            farm.AddProvider(Wheat, 0.05d);

            if (fromLoad)
            {
                Storehouse.RegisterFactory(farm, fromLoad);
                return;
            }

            ResourceAmount woodCost = new ResourceAmount(Wood, 100d);
            ResourceAmount workerCost = new ResourceAmount(Worker, 1d);
            if (Storehouse.ConsumeResource(woodCost, true) && Storehouse.ConsumeResource(workerCost, true))
            {
                Storehouse.ConsumeResource(woodCost, false);
                Storehouse.ConsumeResource(workerCost, false);
                Storehouse.RegisterFactory(farm);
            }
        }

        private static void CreateLumberMill(bool fromLoad = false)
        {
            Factory lumberMill = new Factory("Lumber Mill");
            lumberMill.AddConsumer(Wood, 0.1d);
            lumberMill.AddProvider(Planks, 0.5d);

            if (fromLoad)
            {
                Storehouse.RegisterFactory(lumberMill, fromLoad);
                return;
            }

            ResourceAmount woodCost = new ResourceAmount(Wood, 100d);
            ResourceAmount workerCost = new ResourceAmount(Worker, 2d);
            if (Storehouse.ConsumeResource(woodCost, true) && Storehouse.ConsumeResource(workerCost, true))
            {
                Storehouse.ConsumeResource(woodCost, false);
                Storehouse.ConsumeResource(workerCost, false);
                Storehouse.RegisterFactory(lumberMill);
            }
        }

        private static void CreateBakery(bool fromLoad = false)
        {
            Factory bakery = new Factory("Bakery");
            bakery.AddConsumer(Wheat, 0.5d);
            bakery.AddConsumer(Wood, 0.01d);
            bakery.AddProvider(Bread, 0.25d);

            if (fromLoad)
            {
                Storehouse.RegisterFactory(bakery, fromLoad);
                return;
            }

            ResourceAmount plankCost = new ResourceAmount(Planks, 50d);
            ResourceAmount workerCost = new ResourceAmount(Worker, 1d);
            if (Storehouse.ConsumeResource(plankCost, true) && Storehouse.ConsumeResource(workerCost, true))
            {
                Storehouse.ConsumeResource(plankCost, false);
                Storehouse.ConsumeResource(workerCost, false);
                Storehouse.RegisterFactory(bakery);
            }
        }
        #endregion
    }
}
