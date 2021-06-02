
using backend.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;

namespace backend.Services
{
    public class CompanyService
    {
        private readonly IMongoCollection<Companies> company;

        public CompanyService(ICompanieDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            company = database.GetCollection<Companies>(settings.CompanyCollectionName);
        }

        public List<Companies> Get()
        {
            return company.Find(item => item.companyName != "-").Project<Companies>("{_id: 1, companyName: 1}").ToList();
        }

        public Companies Get(string id)
        {
            return company.Find<Companies>(company => company._id == id).FirstOrDefault();
        }

        public Companies Create(Companies book)
        {
            company.InsertOne(book);
            return book;
        }
        public Companies getFirst()
        {
            return company.Find(item => item.companyName != "-").FirstOrDefault();
        }

        public void Update(string id, Companies bookIn)
        {
            company.ReplaceOne(book => book._id == id, bookIn);
        }

        public void Remove(Companies bookIn)
        {
            company.DeleteOne(book => book._id == bookIn._id);
        }

        public void Remove(string id)
        {
            company.DeleteOne(book => book._id == id);
        }

        public void upload(List<Companies> data)
        {
            company.DeleteMany(item => true);

            company.InsertMany(data);
        }
        public Companies getByName(string name) {

            return company.Find<Companies>(item => item.companyName == name).FirstOrDefault();
        }
    }
}