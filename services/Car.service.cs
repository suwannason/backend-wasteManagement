

using backend.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;


namespace backend.Services {
    public class CarService {
        private readonly IMongoCollection<Cars> cars;

        public CarService(ICompanieDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            cars = database.GetCollection<Cars>("Cars");
        }

        public List<Cars> Get()
        {
            return cars.Find(book => true).ToList();
        }

        public Cars Get(string id)
        {
            return cars.Find<Cars>(cars => cars._id == id).FirstOrDefault();
        }

        public Cars Create(Cars body)
        {
            cars.InsertOne(body);
            return body;
        }

        public void Update(string id, Cars bookIn)
        {
            cars.ReplaceOne(book => book._id == id, bookIn);
        }

        public void Remove(Cars bookIn)
        {
            cars.DeleteOne(book => book._id == bookIn._id);
        }

        public void Remove(string id)
        {
            cars.DeleteOne(book => book._id == id);
        }
    }
}