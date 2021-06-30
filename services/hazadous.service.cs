
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using backend.Models;
using backend.request;
using System;

namespace backend.Services
{

    public class HazadousService
    {
        private readonly IMongoCollection<HazadousSchema> _Hazadous;

        public HazadousService(ICompanieDatabaseSettings settings)
        {

            MongoClientBase client = new MongoClient(settings.ConnectionString);
            IMongoDatabase database = client.GetDatabase(settings.DatabaseName);

            _Hazadous = database.GetCollection<HazadousSchema>("Hazadous");
        }

        public void create(HazadousSchema body)
        {
            _Hazadous.InsertOne(body);
        }

        public List<HazadousSchema> getByStatus(string status, string dept)
        {

            List<HazadousSchema> data = new List<HazadousSchema>();

            if (dept.ToUpper() == "FAE")
            {
                data = _Hazadous.Find<HazadousSchema>(item => item.status == status).ToList();
            }
            else
            {
                data = _Hazadous.Find<HazadousSchema>(item => item.status == status && item.dept == dept).ToList();
            }
            return data;
        }
        public void updateStatus(string id, string status, request.Profile user)
        {

            FilterDefinition<HazadousSchema> findId = Builders<HazadousSchema>.Filter.Eq(item => item._id, id);

            UpdateDefinition<HazadousSchema> update = null;
            if (status == "req-checked")
            {
                update = Builders<HazadousSchema>.Update.Set("req_checked", user).Set("status", status);
            }
            else if (status == "req-approved")
            {
                update = Builders<HazadousSchema>.Update.Set("req_approved", user).Set("status", status);
            }
            else if (status == "fae-checked")
            {

            }
            else if (status == "fae-approved")
            {

            }
            _Hazadous.UpdateOne(findId, update);
        }
    }
}