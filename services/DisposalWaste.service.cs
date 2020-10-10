
using backend.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using System;

namespace backend.Services
{
    public class DisposalService {
        private readonly IMongoCollection<DisposalWaste> disposal;

        public DisposalService(ICompanieDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            disposal = database.GetCollection<DisposalWaste>("Disposal");
        }

        public List<DisposalWaste> Get()
        {
            // return disposal.Find(book => true).ToList();
            return disposal.Find<DisposalWaste>(disposal => disposal.year == DateTime.Now.Year.ToString() && disposal.status == "open").ToList();
        }

        public DisposalWaste Get(string id)
        {
            return disposal.Find<DisposalWaste>(disposal => disposal._id == id).FirstOrDefault();
        }

        public DisposalWaste Create(DisposalWaste book)
        {
            disposal.InsertOne(book);
            return book;
        }

        public void Update(string id, DisposalWaste bookIn)
        {
            disposal.ReplaceOne(book => book._id == id, bookIn);
        }

        public void Remove(DisposalWaste bookIn)
        {
            disposal.DeleteOne(book => book._id == bookIn._id);
        }

        public void Remove(string id)
        {
            disposal.DeleteOne(book => book._id == id);
        }
    }
}