
using backend.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;

namespace backend.Services
{
    public class WasteNameService {
        
        private readonly IMongoCollection<WasteName> wasteName;

        public WasteNameService(ICompanieDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            wasteName = database.GetCollection<WasteName>("WasteName");
        }

        public List<WasteName> Get()
        {
            return wasteName.Find(book => true).ToList();
        }

        public WasteName Get(string id)
        {
            return wasteName.Find<WasteName>(wasteName => wasteName._id == id).FirstOrDefault();
        }

        public WasteName Create(WasteName book)
        {
            wasteName.InsertOne(book);
            return book;
        }

        public void Update(string id, WasteName bookIn)
        {
            wasteName.ReplaceOne(book => book._id == id, bookIn);
        }

        public void Remove(WasteName bookIn)
        {
            wasteName.DeleteOne(book => book._id == bookIn._id);
        }

        public void Remove(string id)
        {
            wasteName.DeleteOne(book => book._id == id);
        }

        public WasteName getWastenameByname(string name) {
            return wasteName.Find<WasteName>(waste => waste.wasteName == name).FirstOrDefault();
        }
    }
}