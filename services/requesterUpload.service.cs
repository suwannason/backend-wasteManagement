
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

using backend.Models;
using backend.request;
using System;

namespace backend.Services
{
    public class requesterUploadServices
    {
        private readonly IMongoCollection<requesterUploadSchema> _scrapMatrial;
        public requesterUploadServices(ICompanieDatabaseSettings settings)
        {
            MongoClientBase client = new MongoClient(settings.ConnectionString);
            IMongoDatabase database = client.GetDatabase(settings.DatabaseName);

            _scrapMatrial = database.GetCollection<requesterUploadSchema>("Requester");
        }

        public requesterUploadSchema getById(string id)
        {

            return _scrapMatrial.Find(item => item._id == id).FirstOrDefault();
        }
        public List<requesterUploadSchema> getByLotNoAndStatus(string lotNo, string status)
        {
            return _scrapMatrial
            .Find<requesterUploadSchema>(item => item.lotNo == lotNo && item.status == status)
            .ToList<requesterUploadSchema>();
        }

        public void updateStatus(string id, string status)
        {
            FilterDefinition<requesterUploadSchema> filter = Builders<requesterUploadSchema>.Filter.Eq(item => item._id, id);
            UpdateDefinition<requesterUploadSchema> update = Builders<requesterUploadSchema>.Update.Set("status", status);

            _scrapMatrial.UpdateMany(filter, update);
        }
        public void signedProfile(string id, string status, Profile user)
        {
            Console.WriteLine("signedProfile: " + status);
            FilterDefinition<requesterUploadSchema> eqlotNo = Builders<requesterUploadSchema>.Filter.Eq(item => item._id, id);
            FilterDefinition<requesterUploadSchema> eqStatus = Builders<requesterUploadSchema>.Filter.Eq(item => item.status, status);

            UpdateDefinition<requesterUploadSchema> update = null;
            if (status == "req-checked")
            {
                update = Builders<requesterUploadSchema>.Update.Set("req_checked", user);
            }
            else if (status == "req-approved")
            {
                update = Builders<requesterUploadSchema>.Update.Set("req_approved", user);
            }
            else if (status == "pdc-prepared")
            {
                update = Builders<requesterUploadSchema>.Update.Set("pdc_prepared", user);
            }
            else if (status == "pdc-checked")
            {
                update = Builders<requesterUploadSchema>.Update.Set("pdc_checked", user);
            }
            else if (status == "pdc-approved")
            {
                update = Builders<requesterUploadSchema>.Update.Set("pdc_approved", user);
            }
            else if (status == "itc-checked")
            {
                update = Builders<requesterUploadSchema>.Update.Set("itc_checked", user);
            }
            else if (status == "itc-approved")
            {
                update = Builders<requesterUploadSchema>.Update.Set("itc_approved", user);
            }
            else if (status == "fae-prepared")
            {
                update = Builders<requesterUploadSchema>.Update.Set("fae_prepared", user);
            }
            else if (status == "fae-checked")
            {
                Console.WriteLine("skhdl");
                update = Builders<requesterUploadSchema>.Update.Set("fae_checked", user);
            }
            else if (status == "fae-approved")
            {
                update = Builders<requesterUploadSchema>.Update.Set("fae_approved", user);
            }

            _scrapMatrial.UpdateMany(eqlotNo & eqStatus, update);
        }

        public void handleUpload(List<requesterUploadSchema> body)
        {
            _scrapMatrial.InsertMany(body);
        }
        public List<requesterUploadSchema> getByStatus(string status, string dept)
        {
            return _scrapMatrial
            .Find<requesterUploadSchema>(item => item.status == status & item.dept == dept)
            .ToList<requesterUploadSchema>();
        }

        public List<requesterUploadSchema> getByStatus_fae(string status)
        {
            Console.WriteLine(status);
            try
            {
                List<requesterUploadSchema> data = _scrapMatrial.Find<requesterUploadSchema>(item => item.status == status).ToList();
                return data;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return new List<requesterUploadSchema>();
            }
            // Console.WriteLine(_scrapMatrial.Count<requesterUploadSchema>(item => item.status == status));
        }

        public List<requesterUploadSchema> getHistory(string month, string year, string dept)
        {
            FilterDefinition<requesterUploadSchema> start = Builders<requesterUploadSchema>.Filter.Gte(item => item.requestMonth, month);
            FilterDefinition<requesterUploadSchema> end = Builders<requesterUploadSchema>.Filter.Lte(item => item.requestYear, year);
            FilterDefinition<requesterUploadSchema> department = Builders<requesterUploadSchema>.Filter.Lte(item => item.dept, dept);

            return _scrapMatrial.Find<requesterUploadSchema>(start & end & department).ToList<requesterUploadSchema>();
        }

        public void updateRefInvoice(string lotNo)
        {
            FilterDefinition<requesterUploadSchema> filter = Builders<requesterUploadSchema>.Filter.Eq(item => item.lotNo, lotNo);
            UpdateDefinition<requesterUploadSchema> update = Builders<requesterUploadSchema>.Update.Set("invoiceRef", true);

            _scrapMatrial.UpdateMany(filter, update);
        }
        public List<requesterUploadSchema> searchToInvoice(string startDate, string endDate)
        {

            FilterDefinition<requesterUploadSchema> start = Builders<requesterUploadSchema>.Filter.Gte(item => item.moveOutDate, startDate);
            FilterDefinition<requesterUploadSchema> end = Builders<requesterUploadSchema>.Filter.Lte(item => item.moveOutDate, endDate);

            FilterDefinition<requesterUploadSchema> nonBoi = Builders<requesterUploadSchema>.Filter.Ne(item => item.boiType, "BOI");

            return _scrapMatrial.Find<requesterUploadSchema>((start & end) & nonBoi).ToList<requesterUploadSchema>();

        }

        public List<requesterUploadSchema> getByLotno(string lotNo)
        {
            FilterDefinition<requesterUploadSchema> lotNoFilter = Builders<requesterUploadSchema>.Filter.Eq(item => item.lotNo, lotNo);
            return _scrapMatrial.Find<requesterUploadSchema>(lotNoFilter).ToList<requesterUploadSchema>();
        }

        public void setFaeDB(requesterUploadSchema data, string id)
        {
            UpdateDefinition<requesterUploadSchema> update = Builders<requesterUploadSchema>.Update
            .Set("biddingNo", data.biddingNo)
            .Set("biddingType", data.biddingType)
            .Set("color", data.color)
            .Set("unitPrice", data.unitPrice)
            .Set("totalPrice", data.totalPrice);

            FilterDefinition<requesterUploadSchema> filter = Builders<requesterUploadSchema>.Filter.Eq(item => item._id, id);

            _scrapMatrial.UpdateOne(filter, update);
        }

        public List<requesterUploadSchema> faeSummarySearch(string lotNo, string startDate, string endDate, string wasteName, string phase)
        {

            List<requesterUploadSchema> data = _scrapMatrial.Find(item => item.status == "fae-approved").ToList();

            if (lotNo != "-" && lotNo != "")
            {
                data = data.FindAll(e => e.lotNo == lotNo);
            }
            if (wasteName != "-" && wasteName != "")
            {
                data = data.FindAll(e => e.wasteName == wasteName);
            }

            if (startDate != "-" && startDate != "")
            {
                foreach (requesterUploadSchema item in data)
                {

                    string startDateConvert = DateTime.ParseExact(item.moveOutDate, "dd-MMMM-yyyy", System.Globalization.CultureInfo.InvariantCulture).ToString("yyyy/MM/dd");
                    item.moveOutDate = startDateConvert;

                }
                data = data.FindAll(e => startDate.CompareTo(e.moveOutDate) <= 0);
                data = data.FindAll(e => endDate.CompareTo(e.moveOutDate) >= 0);
            }
            return data;
        }

        public List<requesterUploadSchema> getLotNo()
        {
            return _scrapMatrial.Find<requesterUploadSchema>(item => item.status == "fae-approved").Project<requesterUploadSchema>("{lotNo: 1, }").ToList();
        }

        public void updateStatusById(string id, string status)
        {
            FilterDefinition<requesterUploadSchema> filter = Builders<requesterUploadSchema>.Filter.Eq(item => item._id, id);
            UpdateDefinition<requesterUploadSchema> update = Builders<requesterUploadSchema>.Update.Set("status", status);

            _scrapMatrial.UpdateOne(filter, update);
        }

        public List<requesterUploadSchema> getGroupingItems(string moveOutDate, string phase, string boiType, string status, string dept)
        {
            if (dept != "PDC" && dept != "ITC" && dept != "FAE")
            {
                return _scrapMatrial.Find<requesterUploadSchema>(item => item.moveOutDate == moveOutDate && item.phase == phase && item.boiType == boiType && item.status == status && item.dept == dept).ToList();
            }
            return _scrapMatrial.Find<requesterUploadSchema>(item => item.moveOutDate == moveOutDate && item.phase == phase && item.boiType == boiType && item.status == status).ToList();

        }

        public List<requesterUploadSchema> getGroupingTracking(string moveOutDate, string phase, string boiType)
        {

            return _scrapMatrial.Find<requesterUploadSchema>(item => item.moveOutDate == moveOutDate && item.phase == phase && item.boiType == boiType).ToList();
        }

        public List<requesterUploadSchema> getGroupingTracking_dept(string moveOutDate, string phase, string boiType, string dept)
        {

            return _scrapMatrial.Find<requesterUploadSchema>(item => item.moveOutDate == moveOutDate && item.phase == phase && item.boiType == boiType && item.dept == dept).ToList();
        }


        public List<requesterUploadSchema> getTracking()
        {
            return _scrapMatrial.Find<requesterUploadSchema>(item => item.status != "itc-approved" && item.status != "fae-prepared" && item.status != "fae-checked" && item.status != "fae-approved" && item.status != "toInvoice" && item.status != "toSummary").ToList();
        }
    }
}