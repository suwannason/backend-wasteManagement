
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

        public InfectionSchema getById(string id)
        {

            return _infection.Find(item => item._id == id).FirstOrDefault();
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
                _infection.UpdateOne(findId, update);
            }
            else if (status == "req-approved")
            {
                update = Builders<InfectionSchema>.Update.Set("req_approved", user).Set("status", status);
                _infection.UpdateOne(findId, update);
            }
            else if (status == "fae-prepared")
            {

                update = Builders<InfectionSchema>.Update.Set("fae_prepared", user).Set("status", status);
                _infection.UpdateOne(findId, update);
            }
            else if (status == "fae-approved")
            {

                update = Builders<InfectionSchema>.Update.Set("fae_approved", user).Set("status", status);
                _infection.UpdateOne(findId, update);
            }
        }

        public void reject(string id, string commend)
        {

            FilterDefinition<InfectionSchema> filter = Builders<InfectionSchema>.Filter.Eq("_id", id);
            UpdateDefinition<InfectionSchema> update = Builders<InfectionSchema>.Update.Set("rejectCommend", commend).Set("status", "reject");

            _infection.UpdateOne(filter, update);
        }

        public void deleteByFileName(string filename)
        {
            FilterDefinition<InfectionSchema> filter = Builders<InfectionSchema>.Filter.Eq("filename", filename);

            _infection.DeleteMany(filter);
        }
        public List<InfectionSchema> getHistory(string month, string year, string dept)
        {
            FilterDefinition<InfectionSchema> monthFilter = Builders<InfectionSchema>.Filter.Eq("month", month);
            FilterDefinition<InfectionSchema> yearFilter = Builders<InfectionSchema>.Filter.Eq("year", year);
            FilterDefinition<InfectionSchema> deptFilter = Builders<InfectionSchema>.Filter.Eq("dept", dept);

            return _infection.Find<InfectionSchema>(monthFilter & yearFilter & deptFilter).ToList();
        }
    }
}