

using backend.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;

namespace backend.Services
{
    public class wasteGroupService {
        
        private readonly IMongoCollection<WasteGroup> wasteGroup;
        private readonly IMongoCollection<dynamic> maintype;

        public wasteGroupService(ICompanieDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            maintype = database.GetCollection<dynamic>("cptMainType");
            wasteGroup = database.GetCollection<WasteGroup>("wasteGroup");
        }

        public List<WasteGroup> Get()
        {
            return wasteGroup.Find(book => true).ToList();
        }

        public WasteGroup Get(string id)
        {
            return wasteGroup.Find<WasteGroup>(wasteGroup => wasteGroup._id == id).FirstOrDefault();
        }

        public WasteGroup Create(WasteGroup book)
        {
            wasteGroup.InsertOne(book);
            return book;
        }

        public void Update(string id, WasteGroup bookIn)
        {
            wasteGroup.ReplaceOne(book => book._id == id, bookIn);
        }

        public void Remove(WasteGroup bookIn)
        {
            wasteGroup.DeleteOne(book => book._id == bookIn._id);
        }

        public void Remove(string id)
        {
            wasteGroup.DeleteOne(book => book._id == id);
        }

        public dynamic getMaintype() {
            return maintype.Find(items => true).ToList();
        }
    }
}