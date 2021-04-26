
using backend.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using System;
using backend.request;

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
            FilterDefinition<Waste> filter = Builders<Waste>.Filter.Eq(item => item._id, id);
            var update = Builders<Waste>.Update
            .Set("moveOutDate", bookIn.moveOutDate)
            .Set("time", bookIn.time)
            .Set("phase", bookIn.phase)
            .Set("companyApprove", bookIn.companyApprove)
            .Set("containerType", bookIn.containerType)
            .Set("containerWeight", bookIn.containerWeight)
            .Set("cptMainType", bookIn.cptMainType)
            .Set("files", bookIn.files)
            .Set("lotNo", bookIn.lotNo)
            .Set("department", bookIn.department)
            .Set("division", bookIn.division)
            .Set("biddingType", bookIn.biddingType)
            .Set("netWasteWeight", bookIn.netWasteWeight)
            .Set("totalWeight", bookIn.totalWeight)
            .Set("qtyOfContainer", bookIn.qtyOfContainer)
            .Set("status", bookIn.status)
            .Set("contractorCompany", bookIn.contractorCompany)
            .Set("wasteGroup", bookIn.wasteGroup).Set("wasteName", bookIn.wasteName);

            recycle.UpdateOne(filter, update);
            // recycle.ReplaceOne(book => book._id == id, bookIn);
        }

        public void Remove(string id)
        {
            FilterDefinition<Waste> filter = Builders<Waste>.Filter.Eq(item => item._id, id);
            UpdateDefinition<Waste> update = Builders<Waste>.Update.Set("status", "deleted");

            recycle.UpdateOne(filter, update);
        }

        public void updateStatus(string id, string status, Profile profile)
        {
            FilterDefinition<Waste> filter = Builders<Waste>.Filter.Eq(item => item._id, id);

            Profile user = new Profile();

            user.band = profile.band;
            user.dept = profile.dept;
            user.div = profile.div;
            user.empNo = profile.empNo;
            user.name = profile.name;
            if (status == "checked")
            {
                UpdateDefinition<Waste> update = Builders<Waste>.Update.Set("status", status).Set("checkBy", user);
                recycle.UpdateOne(filter, update);

            }
            else if (status == "approved")
            {
                UpdateDefinition<Waste> update = Builders<Waste>.Update.Set("status", status).Set("approveBy", user);
                recycle.UpdateOne(filter, update);
            }
            else if (status == "toInvoice")
            {
                UpdateDefinition<Waste> update = Builders<Waste>.Update.Set("status", status).Set("makingBy", user);
                recycle.UpdateOne(filter, update);
            }
        }

        public List<Waste> getHistory(Int64 startDate, Int64 endDate)
        {
            Console.WriteLine(startDate + " ==> " + endDate);

            FilterDefinition<Waste> gte = Builders<Waste>.Filter.Gte(item => item.createDate, startDate);
            FilterDefinition<Waste> lte = Builders<Waste>.Filter.Lte(item => item.createDate, endDate);
            // return filter;

            return recycle.Find(Builders<Waste>.Filter.Or(gte & lte)).ToList();
        }

        public List<Waste> getToInvoiceAll(request.RequestInvoiceDataAll request)
        {
            FilterDefinition<Waste> dateFilter = Builders<Waste>.Filter.Eq(item => item.moveOutDate, request.date);
            FilterDefinition<Waste> lotNoFilter = Builders<Waste>.Filter.Eq(item => item.lotNo, request.lotNo);
            FilterDefinition<Waste> wasteNameFilter = Builders<Waste>.Filter.Eq(item => item.wasteName, request.wasteName);

            FilterDefinition<Waste> deletedFilter = Builders<Waste>.Filter.Ne(item => item.status, "deleted");
            FilterDefinition<Waste> approveFilter = Builders<Waste>.Filter.Eq(item => item.status, "approve");
            FilterDefinition<Waste> invoiceFilter = Builders<Waste>.Filter.Ne(item => item.status, "toInvoice");

            FilterDefinition<Waste> andOpration = Builders<Waste>.Filter.And(approveFilter & lotNoFilter & wasteNameFilter & deletedFilter & invoiceFilter);

            return recycle.Find(Builders<Waste>.Filter.Or(dateFilter | andOpration)).ToList();
            // return recycle.Find(Builders<Waste>.Filter.And(dateFilter & boiFilter & lotNoFilter & deletedFilter & invoiceFilter)).ToList();
        }

        public List<Waste> getToInvoiceName(request.RequestInvoiceDataWithName request)
        {

            FilterDefinition<Waste> dateFilter = Builders<Waste>.Filter.Eq(item => item.wasteName, request.wasteName);
            FilterDefinition<Waste> deletedFilter = Builders<Waste>.Filter.Ne(item => item.status, "deleted");
            FilterDefinition<Waste> invoiceFilter = Builders<Waste>.Filter.Ne(item => item.status, "toInvoice");

            return recycle.Find(Builders<Waste>.Filter.And(dateFilter & deletedFilter & invoiceFilter)).ToList();
        }

        public void updateInvoiceRef(string lotNo)
        {
            FilterDefinition<Waste> filter = Builders<Waste>.Filter.Eq(item => item.lotNo, lotNo);

            UpdateDefinition<Waste> update = Builders<Waste>.Update.Set("invoiceRef", true);
            recycle.UpdateMany(filter, update);
        }
        public List<Waste> searchToInvoice(string startDate, string endDate)
        {

            FilterDefinition<Waste> start = Builders<Waste>.Filter.Gte(item => item.moveOutDate, startDate);
            FilterDefinition<Waste> end = Builders<Waste>.Filter.Lte(item => item.moveOutDate, endDate);

            return recycle.Find<Waste>(start & end).ToList<Waste>();

        }
    }
}