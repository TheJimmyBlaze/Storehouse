using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Storehouse.Factories;
using Storehouse.Resources;

namespace Storehouse.IO
{
    public class JsonStoreIO : IStoreIO
    {
        public const string FILE_NAME = "StorehouseSaveStateData.json";

        public readonly string directoryPath;

        public JsonStoreIO(string directoryPath)
        {
            if (!string.IsNullOrEmpty(directoryPath))
            {
                directoryPath.Trim();
                if (directoryPath[directoryPath.Length - 1] != '\\')
                    directoryPath += '\\';
            }

            this.directoryPath = directoryPath;
        }

        public void Save(StoreSaveState saveState)
        {
            string path = string.Format(@"{0}{1}", directoryPath, FILE_NAME);

            using (FileStream fileStream = new FileStream(path, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(fileStream))
                {
                    writer.Write(JsonConvert.SerializeObject(saveState, Formatting.Indented));
                }
            }
        }

        public StoreSaveState Load(ResourceRegistry resourceRegistry, FactoryRegistry factoryRegistry)
        {
            string path = string.Format(@"{0}{1}", directoryPath, FILE_NAME);
            StoreSaveState storage = null;

            try
            {
                using (FileStream fileStream = new FileStream(path, FileMode.Open))
                {
                    using (StreamReader reader = new StreamReader(fileStream))
                    {
                        storage  = JsonConvert.DeserializeObject<StoreSaveState>(reader.ReadToEnd());
                    }
                }
            }
            catch (FileNotFoundException)
            {
                throw new FileNotFoundException(string.Format("An existing save file could not be found to load at: {0}", path));
            }

            ResourceCheckpoint checkpoint = new ResourceCheckpoint(storage.ResourceCheckpoint.CheckpointTimeUTC,
                                                                    storage.ResourceCheckpoint.ResourceAmounts,
                                                                    resourceRegistry);

            FactoryManager manager = new FactoryManager(storage.FactoryManager.FactoryAmounts,
                                                        factoryRegistry);

            return new StoreSaveState() { ResourceCheckpoint = checkpoint, FactoryManager = manager };
        }
    }
}
