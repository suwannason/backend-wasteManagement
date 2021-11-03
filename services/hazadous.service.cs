
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;

using backend.Models;
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
            if (dept.ToLower().Contains("fae") && status.Contains("fae"))
            {
                data = _Hazadous.Find<HazadousSchema>(item => item.status == status).ToList();
            }
            else
            {
                data = _Hazadous.Find<HazadousSchema>(item => item.status == status && item.dept == dept).ToList();
            }

            return data;
        }

        public List<HazadousSchema> getForFAE_Acknowledge()
        {
            List<HazadousSchema> data = _Hazadous.Find<HazadousSchema>(item => item.status == "req-approved").ToList();
            return data;
        }
        public void updateStatus(string id, string status, request.Profile user)
        {

            FilterDefinition<HazadousSchema> findId = Builders<HazadousSchema>.Filter.Eq(item => item._id, id);
            Console.WriteLine(status);
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

                string runningNo = getLastRunningNo();
                update = Builders<HazadousSchema>.Update.Set("fae_approved", user).Set("status", status).Set("runningNo", runningNo);
                // _Hazadous.UpdateOne(findId, update);

                // update = Builders<HazadousSchema>.Update.Set("fae_approved", user).Set("status", status);
            }
            else if (status == "fae-recevied")
            {
                update = Builders<HazadousSchema>.Update.Set("fae_received", user).Set("status", status);
            }
            _Hazadous.UpdateOne(findId, update);
        }
        public string getLastRunningNo()
        {
            SortDefinition<HazadousSchema> sorting = Builders<HazadousSchema>.Sort.Descending("_id");
            HazadousSchema lastHazadous = _Hazadous.Find<HazadousSchema>(item => item.runningNo != "-" && item.runningNo != null && item.year == DateTime.Now.ToString("yyyy")).Sort(sorting).FirstOrDefault();


            string runningNo = "HZ-";
            if (lastHazadous == null)
            {
                runningNo += "001/" + DateTime.Now.ToString("yyyy");
            }
            else
            {
                string hazadousYearLast = lastHazadous?.runningNo?.Substring(lastHazadous.runningNo.IndexOf("/") + 1);


                string no = lastHazadous.runningNo.Substring(lastHazadous.runningNo.IndexOf("-") + 1, 3);
                runningNo += (Int32.Parse(no) + 1).ToString().PadLeft(3, '0') + "/" + DateTime.Now.ToString("yyyy");


                // _Hazadous.UpdateOne(findId, update);
            }
            return runningNo;
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
            .Set("rejectCommend", "-")
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

            string status = "";

            HazadousSchema data = _Hazadous.Find(filter).FirstOrDefault();

            if (data.status.Contains("fae"))
            {
                status = "req-approved";
            }
            else if (data.status.Contains("req"))
            {
                status = "reject";
            }
            UpdateDefinition<HazadousSchema> update = Builders<HazadousSchema>.Update.Set("rejectCommend", commend).Set("status", status);

            _Hazadous.UpdateOne(filter, update);
        }

        public List<HazadousSchema> getTracking(string month, string year, string dept)
        {
            FilterDefinition<HazadousSchema> monthFilter = Builders<HazadousSchema>.Filter.Eq("month", month);
            FilterDefinition<HazadousSchema> yearFilter = Builders<HazadousSchema>.Filter.Eq("year", year);
            FilterDefinition<HazadousSchema> deptFilter = Builders<HazadousSchema>.Filter.Eq("dept", dept);
            return _Hazadous.Find<HazadousSchema>(monthFilter & yearFilter & deptFilter).ToList();
        }

        public List<HazadousSchema> getBymonthYear(string month, string year)
        {

            FilterDefinition<HazadousSchema> monthFilter = Builders<HazadousSchema>.Filter.Eq("month", month);
            FilterDefinition<HazadousSchema> yearFilter = Builders<HazadousSchema>.Filter.Eq("year", year);
            return _Hazadous.Find<HazadousSchema>(monthFilter & yearFilter).ToList();
        }


        public void deleteByFileName(string filename)
        {
            FilterDefinition<HazadousSchema> filter = Builders<HazadousSchema>.Filter.Eq("filename", filename);

            _Hazadous.DeleteMany(filter);
        }
    }
}