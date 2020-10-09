


using backend.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;


namespace backend.Services {
    public class SubWasteTypeService {
        
        private readonly IMongoCollection<SubWasteType> subWaste;

        public SubWasteTypeService(ICompanieDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            subWaste = database.GetCollection<SubWasteType>("SubWasteType");

        }

        public List<SubWasteType> Get()
        {
            return subWaste.Find(book => true).ToList();
        }

        public SubWasteType Get(string id)
        {
            return subWaste.Find<SubWasteType>(waste => waste._id == id).FirstOrDefault();
        }

        public SubWasteType Create(SubWasteType body)
        {
            subWaste.InsertOne(body);
            return body;
        }

        public void Update(string id, SubWasteType input)
        {
            subWaste.ReplaceOne(record => record._id == id, input);
        }

        public void Remove(SubWasteType input)
        {
            subWaste.DeleteOne(record => record._id == input._id);
        }

        public void Remove(string id)
        {
            subWaste.DeleteOne(record => record._id == id);
        }
    }
}