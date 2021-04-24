
using MongoDB.Driver;
using backend.Models;
using System.Collections.Generic;

namespace backend.Services
{

    public class faeDBservice
    {

        private readonly IMongoCollection<faeDBschema> _faedb;

        public faeDBservice(ICompanieDatabaseSettings settings)
        {

            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _faedb = database.GetCollection<faeDBschema>("Pricing");

        }

        public void replace(List<faeDBschema> items) {
            _faedb.DeleteMany(Builders<faeDBschema>.Filter.Empty);

            _faedb.InsertMany(items);
        }
    }
}