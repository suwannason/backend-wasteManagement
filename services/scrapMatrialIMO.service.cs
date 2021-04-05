
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

using backend.Models;
using backend.request;
using System;

namespace backend.Services
{
    public class ScrapMatrialImoService
    {
        private readonly IMongoCollection<ScrapMatrialimoSchema> _scrapMatrial;
        public ScrapMatrialImoService(ICompanieDatabaseSettings settings)
        {
            MongoClientBase client = new MongoClient(settings.ConnectionString);
            IMongoDatabase database = client.GetDatabase(settings.DatabaseName);

            _scrapMatrial = database.GetCollection<ScrapMatrialimoSchema>("ScrapMatrialImo");
        }

        public void create(ReuqesterREQ body, Profile req_prepare)
        {
            List<ScrapMatrialimoSchema> data = new List<ScrapMatrialimoSchema>();

            string currentYear = DateTime.Now.Year.ToString();
            SortDefinitionBuilder<ScrapMatrialimoSchema> builder = Builders<ScrapMatrialimoSchema>.Sort;
            SortDefinition<ScrapMatrialimoSchema> sort = builder.Descending("record");

            Parallel.ForEach(body.scrapImo, item =>
            {

                data.Add(new ScrapMatrialimoSchema
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
        public List<ScrapMatrialimoSchema> getByLotNoAndStatus(string lotNo, string status)
        {
            return _scrapMatrial
            .Find<ScrapMatrialimoSchema>(item => item.lotNo == lotNo && item.status == status)
            .ToList<ScrapMatrialimoSchema>();
        }

        public void updateStatus(string lotNo, string status)
        {
            FilterDefinition<ScrapMatrialimoSchema> filter = Builders<ScrapMatrialimoSchema>.Filter.Eq(item => item.lotNo, lotNo);
            UpdateDefinition<ScrapMatrialimoSchema> update = Builders<ScrapMatrialimoSchema>.Update.Set("status", status);

            _scrapMatrial.UpdateMany(filter, update);
        }

        public void handleUpload(List<ScrapMatrialimoSchema> body)
        {
            _scrapMatrial.InsertMany(body);
        }
        public List<ScrapMatrialimoSchema> getByStatus(string status, string dept)
        {
            Console.WriteLine(dept);
            return _scrapMatrial
            .Find<ScrapMatrialimoSchema>(item => item.status == status & item.dept == dept)
            .ToList<ScrapMatrialimoSchema>();
        }

        public List<ScrapMatrialimoSchema> getByStatus_fae(string status) {
            return _scrapMatrial.Find<ScrapMatrialimoSchema>(item => item.status == status).ToList<ScrapMatrialimoSchema>();
        }
    
        public List<ScrapMatrialimoSchema> getHistory (string startDate, string endDate) {

            Console.WriteLine(startDate + " --> " + endDate);
            FilterDefinition<ScrapMatrialimoSchema> start = Builders<ScrapMatrialimoSchema>.Filter.Gte(item => item.moveOutDate, startDate);
            FilterDefinition<ScrapMatrialimoSchema> end = Builders<ScrapMatrialimoSchema>.Filter.Lte(item => item.moveOutDate, endDate);

            return _scrapMatrial.Find<ScrapMatrialimoSchema>(start & end).ToList<ScrapMatrialimoSchema>();
        }
    }
}