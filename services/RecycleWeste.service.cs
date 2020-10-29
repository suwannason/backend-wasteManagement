
using backend.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using System;

namespace backend.Services
{
    public class RecycleService {
        private readonly IMongoCollection<Waste> recycle;

        public RecycleService(ICompanieDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            recycle = database.GetCollection<Waste>("Waste");
        }

        public List<Waste> GetOpen()
        {
            // return recycle.Find(book => true).ToList();
            return recycle.Find<Waste>(recycle => recycle.year == DateTime.Now.Year.ToString() && recycle.status == "open").ToList();
        }

        public List<Waste> GetApprove() {
            return recycle.Find<Waste>(recycle => recycle.year == DateTime.Now.Year.ToString() && recycle.status == "checked").ToList();
        }

        public Waste Get(string id)
        {
            return recycle.Find<Waste>(recycle => recycle._id == id).FirstOrDefault();
        }

        public Waste Create(Waste book)
        {
            recycle.InsertOne(book);
            return book;
        }

        public void Update(string id, Waste bookIn)
        {
            recycle.ReplaceOne(book => book._id == id, bookIn);
        }

        public void Remove(Waste bookIn)
        {
            recycle.DeleteOne(book => book._id == bookIn._id);
        }

        public void updateStatus(string id, string status) {
            var filter = Builders<Waste>.Filter.Eq(item => item._id, id);
            var update = Builders<Waste>.Update.Set("status", status);

            recycle.UpdateOne(filter, update);
        }
    }
}