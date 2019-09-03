using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Storehouse.Factories;
using Storehouse.Resources;

namespace Storehouse
{
    public class JsonStatePersister : IStatePersister
    {
        public const string CHECKPOINT_FILE_NAME = "StorehouseCheckpointData.json";
        public const string FACTORY_FILE_NAME = "StorehouseFactoryData.csv";

        public readonly string directoryPath;

        public JsonStatePersister(string directoryPath)
        {
            if (!string.IsNullOrEmpty(directoryPath))
            {
                directoryPath.Trim();
                if (directoryPath[directoryPath.Length - 1] != '\\')
                    directoryPath += '\\';
            }

            this.directoryPath = directoryPath;
        }

        public void Save(ResourceCheckpoint resourceCheckpoint, FactoryManager factoryManager)
        {
            SaveCheckpoint(directoryPath, resourceCheckpoint);
        }

        public State Load(ResourceRegistry resourceRegistry)
        {
            ResourceCheckpoint checkpoint = LoadCheckpoint(directoryPath, resourceRegistry);
        }

        private static void SaveCheckpoint(string directoryPath, ResourceCheckpoint resourceCheckpoint)
        {
            string path = string.Format(@"{0}{1}", directoryPath, CHECKPOINT_FILE_NAME);
            using (FileStream fileStream = new FileStream(path, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(fileStream))
                {
                    writer.Write(JsonConvert.SerializeObject(resourceCheckpoint, Formatting.Indented));
                }
            }
        }

        private static ResourceCheckpoint LoadCheckpoint(string directoryPath, ResourceRegistry resourceRegistry)
        {
            string path = string.Format(@"{0}{1}", directoryPath, CHECKPOINT_FILE_NAME);
            try
            {
                using (FileStream fileStream = new FileStream(path, FileMode.Open))
                {
                    using (StreamReader reader = new StreamReader(fileStream))
                    {
                        ResourceCheckpoint loadedCheckpoint = JsonConvert.DeserializeObject<ResourceCheckpoint>(reader.ReadToEnd());
                        return new ResourceCheckpoint(loadedCheckpoint.CheckpointTimeUTC, loadedCheckpoint.ResourceAmounts, resourceRegistry);
                    }
                }
            }
            catch (FileNotFoundException)
            {
                throw new FileNotFoundException(string.Format("An existing save file could not be found to load at: {0}", path));
            }
        }

        private static void SaveFactories(string directoryPath, FactoryManager factoryManager)
        {
            string path = string.Format(@"{0}{1}", directoryPath, FACTORY_FILE_NAME);

            Dictionary<Guid, int> uniqueFactories = new Dictionary<Guid, int>();
            foreach (Factory factory in factoryManager.Factories)
            {
                if (!uniqueFactories.ContainsKey(factory.id))
                {
                    uniqueFactories.Add(factory.id, 1);
                    continue;
                }

                if (uniqueFactories.TryGetValue(factory.id, out int count))
                    count++;
                else
                    throw new InvalidOperationException("Attempt to get the value of an impossibly non-existant factory?");
            }

            using (FileStream fileStream = new FileStream(path, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(fileStream))
                {
                    foreach(KeyValuePair<Guid, int> uniqueFactory in uniqueFactories)
                    {
                        writer.WriteLine(string.Format("{0}, {1}", uniqueFactories.Keys, uniqueFactories.Values));
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
    }
}
