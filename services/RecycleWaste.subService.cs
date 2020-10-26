
using backend.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using System;

namespace backend.Services
{
    public class SubRecycleService {
        private readonly IMongoCollection<SubRecycleWaste> subRecycle;

        public SubRecycleService(ICompanieDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            subRecycle = database.GetCollection<SubRecycleWaste>("Recycle-detail");
        }

        public SubRecycleWaste Create(SubRecycleWaste book)
        {
            subRecycle.InsertOne(book);
            return book;
        }
        public List<SubRecycleWaste> GetByMapping(string mapping) {
            return subRecycle.Find(item => item.idMapping == mapping).ToList();
        }
        public void Update(string id, SubRecycleWaste bookIn)
        {
            subRecycle.ReplaceOne(book => book._id == id, bookIn);
        }

        public void Remove(string id)
        {
            subRecycle.DeleteOne(book => book._id == id);
        }
    }
}