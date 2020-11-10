
using backend.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Globalization;

namespace backend.Services
{
    public class RecycleService
    {
        private readonly IMongoCollection<Waste> recycle;

        public RecycleService(ICompanieDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            recycle = database.GetCollection<Waste>("Waste");
        }

        public List<Waste> GetOpen()
        {
            // return recycle.Find(book => true).ToList();
            return recycle.Find<Waste>(recycle => recycle.year == DateTime.Now.Year.ToString() && recycle.status == "open").ToList();
        }

        public List<Waste> GetApprove()
        {
            return recycle.Find<Waste>(recycle => recycle.year == DateTime.Now.Year.ToString() && recycle.status == "checked").ToList();
        }

        public Waste Get(string id)
        {
            return recycle.Find<Waste>(recycle => recycle._id == id).FirstOrDefault();
        }

        public Waste Create(Waste book)
        {
            recycle.InsertOne(book);
            return book;
        }

        public void Update(string id, Waste bookIn)
        {
            var filter = Builders<Waste>.Filter.Eq(item => item._id, id);
            var update = Builders<Waste>.Update
            .Set("date", bookIn.date)
            .Set("time", bookIn.time)
            .Set("phase", bookIn.phase)
            .Set("companyApprove", bookIn.companyApprove)
            .Set("containerType", bookIn.containerType)
            .Set("containerWeight", bookIn.containerWeight)
            .Set("cptType", bookIn.cptType)
            .Set("files", bookIn.files)
            .Set("gennerateGroup", bookIn.gennerateGroup)
            .Set("lotNo", bookIn.lotNo)
            .Set("netWasteWeight", bookIn.netWasteWeight)
            .Set("totalWeight", bookIn.totalWeight)
            .Set("typeBoi", bookIn.typeBoi).Set("status", bookIn.status)
            .Set("wasteContractor", bookIn.wasteContractor).Set("wasteGroup", bookIn.wasteGroup).Set("wasteName", bookIn.wasteName);
            
            recycle.UpdateOne(filter, update);
            // recycle.ReplaceOne(book => book._id == id, bookIn);
        }

        public void Remove(string id)
        {
            var filter = Builders<Waste>.Filter.Eq(item => item._id, id);
            var update = Builders<Waste>.Update.Set("status", "deleted");

            recycle.UpdateOne(filter, update);
        }

        public void updateStatus(string id, string status)
        {
            var filter = Builders<Waste>.Filter.Eq(item => item._id, id);
            var update = Builders<Waste>.Update.Set("status", status);

            recycle.UpdateOne(filter, update);
        }

        public List<Waste> getHistory(Int64 startDate, Int64 endDate)
        {
            Console.WriteLine(startDate + " ==> " + endDate);

            var gte = Builders<Waste>.Filter.Gte(item => item.createDate, startDate);
            var lte = Builders<Waste>.Filter.Lte(item => item.createDate, endDate);
            // return filter;

            return recycle.Find(Builders<Waste>.Filter.And(gte & lte)).ToList();
        }
    }
}