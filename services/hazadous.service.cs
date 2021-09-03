
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

            data = _Hazadous.Find<HazadousSchema>(item => item.status == status && item.dept == dept).ToList();
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
                update = Builders<HazadousSchema>.Update.Set("fae_checked", user).Set("status", status);
            }
            else if (status == "fae-approved")
            {
                update = Builders<HazadousSchema>.Update.Set("fae_approved", user).Set("status", status);
            }
            _Hazadous.UpdateOne(findId, update);
        }

        public HazadousSchema getById(string id)
        {

            return _Hazadous.Find<HazadousSchema>(item => item._id == id).FirstOrDefault();
        }

        public void faePrepare_setcommend(string id, string description, request.Profile user)
        {
            FilterDefinition<HazadousSchema> filter = Builders<HazadousSchema>.Filter.Eq("_id", id);
            UpdateDefinition<HazadousSchema> update = null;

            if (user != null)
            {
                update = Builders<HazadousSchema>.Update.Set("description", description).Set("fae_prepared", user);
            }
            else
            {
                update = Builders<HazadousSchema>.Update.Set("description", description);
            }

            _Hazadous.UpdateOne(filter, update);
        }
        public void faePrepare(string id, string no, bool allowed, bool burn, bool recycle)
        {
            var filter = Builders<HazadousSchema>.Filter.Eq("_id", id) & Builders<HazadousSchema>.Filter.Eq("items.no", no);
            UpdateDefinition<HazadousSchema> update = Builders<HazadousSchema>.Update
            .Set("items.$.allowed", allowed)
            .Set("items.$.burn", burn)
            .Set("items.$.recycle", recycle)
            .Set("status", "fae-prepared");

            _Hazadous.UpdateOne(filter, update);

        }

        public HazadousItems getSubId(string id, string no)
        {
            // FilterDefinition<HazadousSchema> filterNo = Builders<HazadousSchema>.Filter.Eq("items.$.no", no);
            var filterId = Builders<HazadousSchema>.Filter.Eq("_id", id) & Builders<HazadousSchema>.Filter.Eq("items.no", no);

            return _Hazadous.Find<HazadousSchema>(filterId)?.FirstOrDefault()?.items.FirstOrDefault();

        }

        public void reject(string id, string commend)
        {

            FilterDefinition<HazadousSchema> filter = Builders<HazadousSchema>.Filter.Eq("_id", id);

            UpdateDefinition<HazadousSchema> update = Builders<HazadousSchema>.Update.Set("rejectCommend", commend).Set("status", "reject");

            _Hazadous.UpdateOne(filter, update);
        }
        public void deleteWithfileName(string filename) {
            FilterDefinition<HazadousSchema> filter = Builders<HazadousSchema>.Filter.Eq("filename", filename);

            _Hazadous.DeleteMany(filter);
        }
    }
}