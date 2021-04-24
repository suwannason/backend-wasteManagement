
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

        public void create(ReuqesterREQ body, Profile req_prepare)
        {
            List<requesterUploadSchema> data = new List<requesterUploadSchema>();

            string currentYear = DateTime.Now.Year.ToString();
            SortDefinitionBuilder<requesterUploadSchema> builder = Builders<requesterUploadSchema>.Sort;
            SortDefinition<requesterUploadSchema> sort = builder.Descending("record");

            Parallel.ForEach(body.scrapImo, item =>
            {

                data.Add(new requesterUploadSchema
                {
                    no = item.no,
                    date = body.date,
                    div = body.div,
                    dept = body.dept,
                    biddingType = "<DEFAULT DB BY ITC>",

                    qtyOfContainer = item.qtyOfContainer,
                    matrialName = item.matrialName,
                    moveOutDate = item.moveOutDate,
                    lotNo = "<CREATE API FOR FN GET LAST LOT>",
                    color = "<DEFAULT DB BY ITC>",
                    biddingNo = "<DEFAULT DB BY ITC>",
                    unitPrice = "<DEFAULT DB BY ITC>",
                    totalPrice = "item.netWasteWeight * <DEFAULT DB BY ITC (unit price)>",
                    boiType = item.boiType,
                    containerWeight = item.containerWeight,
                    unit = item.unit,
                    groupBoiName = item.groupBoiName,
                    groupBoiNo = item.groupBoiNo,
                    matrialCode = item.matrialCode,
                    netWasteWeight = item.netWasteWeight,
                    totalWeight = item.totalWeight,
                    req_prepared = req_prepare,
                    status = "req-prepared",
                    itc_approved = new Profile { empNo = "-", name = "-", band = "-", dept = "-", div = "-", tel = "-" },
                    itc_checked = new Profile { empNo = "-", name = "-", band = "-", dept = "-", div = "-", tel = "-" },
                    req_checked = new Profile { empNo = "-", name = "-", band = "-", dept = "-", div = "-", tel = "-" },
                    req_approved = new Profile { empNo = "-", name = "-", band = "-", dept = "-", div = "-", tel = "-" },
                    year = currentYear,
                    fae_checked = new Profile { empNo = "-", name = "-", band = "-", dept = "-", div = "-", tel = "-" },
                    fae_approved = new Profile { empNo = "-", name = "-", band = "-", dept = "-", div = "-", tel = "-" },
                });
            });
            _scrapMatrial.InsertMany(data);
        }
        public List<requesterUploadSchema> getByLotNoAndStatus(string lotNo, string status)
        {
            return _scrapMatrial
            .Find<requesterUploadSchema>(item => item.lotNo == lotNo && item.status == status)
            .ToList<requesterUploadSchema>();
        }

        public void updateStatus(string lotNo, string status)
        {
            FilterDefinition<requesterUploadSchema> filter = Builders<requesterUploadSchema>.Filter.Eq(item => item.lotNo, lotNo);
            UpdateDefinition<requesterUploadSchema> update = Builders<requesterUploadSchema>.Update.Set("status", status);

            _scrapMatrial.UpdateMany(filter, update);
        }
        public void signedProfile(string lotNo, string status, Profile user)
        {
            Console.WriteLine("signedProfile: " + status);
            FilterDefinition<requesterUploadSchema> eqlotNo = Builders<requesterUploadSchema>.Filter.Eq(item => item.lotNo, lotNo);
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
            Console.WriteLine(dept);
            return _scrapMatrial
            .Find<requesterUploadSchema>(item => item.status == status & item.dept == dept)
            .ToList<requesterUploadSchema>();
        }

        public List<requesterUploadSchema> getByStatus_fae(string status)
        {
            return _scrapMatrial.Find<requesterUploadSchema>(item => item.status == status).ToList<requesterUploadSchema>();
        }

        public List<requesterUploadSchema> getHistory(string startDate, string endDate)
        {

            Console.WriteLine(startDate + " --> " + endDate);
            FilterDefinition<requesterUploadSchema> start = Builders<requesterUploadSchema>.Filter.Gte(item => item.moveOutDate, startDate);
            FilterDefinition<requesterUploadSchema> end = Builders<requesterUploadSchema>.Filter.Lte(item => item.moveOutDate, endDate);

            return _scrapMatrial.Find<requesterUploadSchema>(start & end).ToList<requesterUploadSchema>();
        }

        public void updateRefInvoice(string lotNo)
        {
            FilterDefinition<requesterUploadSchema> filter = Builders<requesterUploadSchema>.Filter.Eq(item => item.lotNo, lotNo);
            UpdateDefinition<requesterUploadSchema> update = Builders<requesterUploadSchema>.Update.Set("invoiceRef", true);

            _scrapMatrial.UpdateMany(filter, update);
        }
    }
}