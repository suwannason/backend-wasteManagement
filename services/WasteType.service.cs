

using backend.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;


namespace backend.Services {
    public class WasteTypeService {
        private readonly IMongoCollection<WasteType> waste;
        private readonly IMongoCollection<SubWasteType> subWaste;

        public WasteTypeService(ICompanieDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            waste = database.GetCollection<WasteType>("WasteType");
            subWaste = database.GetCollection<SubWasteType>("SubWasteType");

        }

        public List<WasteType> Get()
        {
            return waste.Find(book => true).ToList();
        }

        public WasteType Get(string id)
        {
            return waste.Find<WasteType>(waste => waste._id == id).FirstOrDefault();
        }

        public WasteType Create(WasteType body)
        {
            waste.InsertOne(body);
            return body;
        }

        public void Update(string id, WasteType input)
        {
            waste.ReplaceOne(record => record._id == id, input);
        }

        public void Remove(WasteType input)
        {
            waste.DeleteOne(record => record._id == input._id);
        }

        public void Remove(string id)
        {
            waste.DeleteOne(record => record._id == id);
            subWaste.DeleteMany(record => record.mainType == id);
        }
    }
}