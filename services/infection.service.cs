
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;

using backend.Models;

namespace backend.Services
{

    public class InfectionService
    {
        private readonly IMongoCollection<InfectionSchema> _infection;

        public InfectionService(ICompanieDatabaseSettings settings)
        {

            MongoClientBase client = new MongoClient(settings.ConnectionString);
            IMongoDatabase database = client.GetDatabase(settings.DatabaseName);

            _infection = database.GetCollection<InfectionSchema>("Infection");
        }

        public void create(InfectionSchema body)
        {
            _infection.InsertOne(body);
        }

        public List<InfectionSchema> getByStatus(string status, string dept)
        {

            List<InfectionSchema> data = new List<InfectionSchema>();

            if (dept.ToUpper() == "FAE")
            {
                data = _infection.Find<InfectionSchema>(item => item.status == status).ToList();
            }
            else
            {
                data = _infection.Find<InfectionSchema>(item => item.status == status && item.dept == dept).ToList();
            }
            return data;
        }

        public void updateStatus(string id, string status, request.Profile user)
        {

            FilterDefinition<InfectionSchema> findId = Builders<InfectionSchema>.Filter.Eq(item => item._id, id);

            UpdateDefinition<InfectionSchema> update = null;
            if (status == "req-checked")
            {
                update = Builders<InfectionSchema>.Update.Set("req_checked", user).Set("status", status);
            }
            else if (status == "req-approved")
            {
                update = Builders<InfectionSchema>.Update.Set("req_approved", user).Set("status", status);
            }
            _infection.UpdateOne(findId, update);
        }
    }
}