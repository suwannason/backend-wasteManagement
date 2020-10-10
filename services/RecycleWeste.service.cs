
using backend.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using System;

namespace backend.Services
{
    public class RecycleService {
        private readonly IMongoCollection<RecycleWeste> recycle;

        public RecycleService(ICompanieDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            recycle = database.GetCollection<RecycleWeste>("Recycle");
        }

        public List<RecycleWeste> Get()
        {
            // return recycle.Find(book => true).ToList();
            return recycle.Find<RecycleWeste>(recycle => recycle.year == DateTime.Now.Year.ToString() && recycle.status == "open").ToList();
        }

        public RecycleWeste Get(string id)
        {
            return recycle.Find<RecycleWeste>(recycle => recycle._id == id).FirstOrDefault();
        }

        public RecycleWeste Create(RecycleWeste book)
        {
            recycle.InsertOne(book);
            return book;
        }

        public void Update(string id, RecycleWeste bookIn)
        {
            recycle.ReplaceOne(book => book._id == id, bookIn);
        }

        public void Remove(RecycleWeste bookIn)
        {
            recycle.DeleteOne(book => book._id == bookIn._id);
        }

        public void Remove(string id)
        {
            recycle.DeleteOne(book => book._id == id);
        }
    }
}