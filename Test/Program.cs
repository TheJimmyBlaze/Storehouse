using StorehouseLib;
using StorehouseLib.Factories;
using StorehouseLib.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Test
{
    class Program
    {
        public static Resource Wheat;
        public static Resource Bread;
        public static Resource Wood;
        public static Resource Planks;

        static void Main(string[] args)
        {
            Storehouse storehouse = new Storehouse();
            
            Wheat = storehouse.RegisterResource(new Resource("Wheat", null));
            Bread = storehouse.RegisterResource(new Resource("Bread", Wheat));
            Wood = storehouse.RegisterResource(new Resource("Wood", null));
            Planks = storehouse.RegisterResource(new Resource("Planks", Wood));

            CreateFarm(storehouse);
            CreateFarm(storehouse);
            CreateFarm(storehouse);
            CreateFarm(storehouse);

            CreateBakery(storehouse);

            CreateForestryCamp(storehouse);

            CreateLumberMill(storehouse);
            CreateLumberMill(storehouse);

            Print(storehouse);
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

                Dictionary<Guid, double> resourceTotals = storehouse.GetResourceTotals();

                foreach (KeyValuePair<Guid, double> resourceKeyValue in resourceTotals)
                {
                    Resource resource = storehouse.ResourceRegistry.GetResource(resourceKeyValue.Key);
                    Console.WriteLine("{0}: {1}", resource.Name, resourceKeyValue.Value.ToString("0.0").PadRight(16));
                }

                lastPrint = DateTime.UtcNow;
            }
        }

        #region Factory Creators
        private static void CreateFarm(Storehouse storehouse)
        {
            Factory farm = new Factory("Farm");
            farm.AddProvider(Wheat, 0.5d);

            storehouse.RegisterFactory(farm);
        }

        private static void CreateBakery(Storehouse storehouse)
        {
            Factory bakery = new Factory("Bakery");
            bakery.AddConsumer(Wheat, 1d);
            bakery.AddConsumer(Wood, 0.1d);
            bakery.AddProvider(Bread, 0.25);

            storehouse.RegisterFactory(bakery);
        }

        private static void CreateForestryCamp(Storehouse storehouse)
        {
            Factory forestryCamp = new Factory("Forestry Camp");
            forestryCamp.AddProvider(Wood, 1d);

            storehouse.RegisterFactory(forestryCamp);
        }

        private static void CreateLumberMill(Storehouse storehouse)
        {
            Factory lumberMill = new Factory("Lumber Mill");
            lumberMill.AddConsumer(Wood, 0.1);
            lumberMill.AddProvider(Planks, 0.5);

            storehouse.RegisterFactory(lumberMill);
        }
        #endregion
    }
}
