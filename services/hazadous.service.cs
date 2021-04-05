
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

        public long countItemsByTracking(string lotNo)
        {
            FilterDefinition<HazadousSchema> dataFilter = Builders<HazadousSchema>.Filter.Eq(item => item.lotNo, lotNo);

            return _Hazadous.CountDocuments(dataFilter);
        }

        public List<HazadousSchema> getAll()
        {
            return _Hazadous.Find<HazadousSchema>(item => true).ToList();
        }
        public void create(ReuqesterREQ body, Profile req_prepare)
        {
            List<HazadousSchema> items = new List<HazadousSchema>();

            string currentYear = DateTime.Now.Year.ToString();
            SortDefinitionBuilder<HazadousSchema> builder = Builders<HazadousSchema>.Sort;
            SortDefinition<HazadousSchema> sort = builder.Descending("record");

            Parallel.ForEach(body.hazardous, item =>
            {
                HazadousSchema lastItem = _Hazadous.Find<HazadousSchema>(item => item.year == currentYear).Sort(sort).FirstOrDefault();

                string record = "";

                if (lastItem == null)
                {
                    record = "001";
                }
                else
                {
                    record = (Int32.Parse(lastItem.record) + 1).ToString().PadLeft(3, '0');
                }
                items.Add(new HazadousSchema
                {
                    date = body.date,
                    dept = body.date,
                    time = body.time,
                    div = body.div,
                    binddingType = item.binddingType,
                    boiType = item.boiType,
                    companyApproveNo = item.companyApproveNo,
                    containerType = item.containerType,
                    contractorCompany = item.contractorCompany,
                    cptMainType = item.cptMainType,
                    lotNo = item.lotNo,
                    partNormalType = item.partNormalType,
                    phase = item.phase,
                    productType = item.productType,
                    qtyOfContainer = item.qtyOfContainer,
                    totalWeight = item.totalWeight,
                    wasteGroup = item.wasteGroup,
                    wasteName = item.wasteName,
                    wasteType = item.wasteType,
                    weightPerContainer = item.weightPerContainer,
                    status = "req-prepared",
                    record = record,
                    year = currentYear,
                    req_prepared = req_prepare,
                    req_checked = new Profile { empNo = "-", name = "-", band = "-", dept = "-", div = "-", tel = "-" },
                    req_approved = new Profile { empNo = "-", name = "-", band = "-", dept = "-", div = "-", tel = "-" },

                    fae_prepared = new Profile { empNo = "-", name = "-", band = "-", dept = "-", div = "-", tel = "-" },
                    fae_checked = new Profile { empNo = "-", name = "-", band = "-", dept = "-", div = "-", tel = "-" },
                    fae_approved = new Profile { empNo = "-", name = "-", band = "-", dept = "-", div = "-", tel = "-" },
                });
            });
            _Hazadous.InsertMany(items);
        }

        public void updateStatus(string lotNo, string status)
        {
            FilterDefinition<HazadousSchema> filter = Builders<HazadousSchema>.Filter.Eq(item => item.lotNo, lotNo);
            UpdateDefinition<HazadousSchema> update = Builders<HazadousSchema>.Update.Set("status", status);

            _Hazadous.UpdateMany(filter, update);
        }

        public List<HazadousSchema> getByStatus(string status, string dept)
        {
            return _Hazadous
            .Find<HazadousSchema>(item => item.status == status & item.dept == dept)
            .ToList<HazadousSchema>();
        }

        public List<HazadousSchema> getByStatus_fae(string status) {
            return _Hazadous.Find<HazadousSchema>(item => item.status == status).ToList<HazadousSchema>();
        }
        public List<HazadousSchema> getByLotnoIdAndStatus(string lotNo, string status)
        {
            return _Hazadous
            .Find<HazadousSchema>(item => item.lotNo == lotNo && item.status == status)
            .ToList<HazadousSchema>();
        }
        public string getLastRecord()
        {
            string currentYear = DateTime.Now.Year.ToString();
            SortDefinitionBuilder<HazadousSchema> builder = Builders<HazadousSchema>.Sort;
            SortDefinition<HazadousSchema> sort = builder.Descending("record");
            HazadousSchema data = _Hazadous.Find(item => item.year == currentYear).Sort(sort).FirstOrDefault();

            if (data == null)
            {
                return "000";
            }
            return data.record;
        }
    }
}