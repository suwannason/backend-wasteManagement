
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

using backend.Models;
using backend.request;
using System;

namespace backend.Services
{
    public class InfectionsService
    {
        private readonly IMongoCollection<InfectionSchema> _Infections;
        public InfectionsService(ICompanieDatabaseSettings settings)
        {
            MongoClientBase client = new MongoClient(settings.ConnectionString);
            IMongoDatabase database = client.GetDatabase(settings.DatabaseName);

            _Infections = database.GetCollection<InfectionSchema>("Infections");
        }

        public long countItemsByTracking(string status)
        {
            FilterDefinition<InfectionSchema> dataFilter = Builders<InfectionSchema>.Filter.Eq(item => item.status, status);

            return _Infections.CountDocuments(dataFilter);
        }
        public void create(ReuqesterREQ body, Profile req_prepare)
        {
            List<InfectionSchema> data = new List<InfectionSchema>();

            string currentYear = DateTime.Now.Year.ToString();
            SortDefinitionBuilder<InfectionSchema> builder = Builders<InfectionSchema>.Sort;
            SortDefinition<InfectionSchema> sort = builder.Descending("record");

            Parallel.ForEach(body.infections, item =>
            {
                InfectionSchema lastItem = _Infections.Find<InfectionSchema>(item => item.year == currentYear).Sort(sort).FirstOrDefault();

                string record = "";

                if (lastItem == null)
                {
                    record = "001";
                }
                else
                {
                    record = (Int32.Parse(lastItem.record) + 1).ToString().PadLeft(3, '0');
                }
                data.Add(new InfectionSchema
                {
                    no = item.no,
                    dept = body.dept,
                    div = body.div,
                    date = body.date,
                    time = body.time,
                    biddingType = item.biddingType,
                    containerType = item.containerType,
                    contractorCompany = item.contractorCompany,
                    evaluation = "Waiting for FAE",
                    qtyOfContainer = item.qtyOfContainer,
                    suggestionByFae = "Waiting for FAE",
                    totalWeight = item.totalWeight,
                    valuableType = "Waiting for FAE",
                    wasteEachDept = item.wasteEachDept,
                    wasteName = item.wasteName,
                    weightPerContainer = item.weightPerContainer,
                    req_prepared = req_prepare,
                    status = "req-prepared",
                    record = record,
                    year = currentYear,
                    req_checked = new Profile { empNo = "-", name = "-", band = "-", dept = "-", div = "-", tel = "-" },
                    req_approved = new Profile { empNo = "-", name = "-", band = "-", dept = "-", div = "-", tel = "-" },

                    fae_prepared = new Profile { empNo = "-", name = "-", band = "-", dept = "-", div = "-", tel = "-" },
                    fae_checked = new Profile { empNo = "-", name = "-", band = "-", dept = "-", div = "-", tel = "-" },
                    fae_approved = new Profile { empNo = "-", name = "-", band = "-", dept = "-", div = "-", tel = "-" },
                });
            });
            _Infections.InsertMany(data);
        }
        public List<InfectionSchema> getByStatus(string status, string dept)
        {
            FilterDefinition<InfectionSchema> statusFilter = Builders<InfectionSchema>.Filter.Eq(item => item.status, status);
            FilterDefinition<InfectionSchema> deptFilter = Builders<InfectionSchema>.Filter.Eq(item => item.dept, dept);

            List<InfectionSchema> data = _Infections.Find(Builders<InfectionSchema>.Filter.And(statusFilter & deptFilter)).ToList<InfectionSchema>();
            return data;
        }

        public List<InfectionSchema> getByStatus_fae(string status)
        {
            FilterDefinition<InfectionSchema> statusFilter = Builders<InfectionSchema>.Filter.Eq(item => item.status, status);

            List<InfectionSchema> data = _Infections.Find(statusFilter).ToList<InfectionSchema>();

            return data;
        }

        public List<InfectionSchema> getByStatus(string status)
        {
            return _Infections
            .Find<InfectionSchema>(item => item.status == status)
            .ToList<InfectionSchema>();
        }

        public void updateStatus(string id, string status)
        {
            FilterDefinition<InfectionSchema> filter = Builders<InfectionSchema>.Filter.Eq(item => item._id, id);
            UpdateDefinition<InfectionSchema> update = Builders<InfectionSchema>.Update.Set("status", status);

            _Infections.UpdateMany(filter, update);
        }
        public string getLastRecord()
        {
            string currentYear = DateTime.Now.Year.ToString();
            SortDefinitionBuilder<InfectionSchema> builder = Builders<InfectionSchema>.Sort;
            SortDefinition<InfectionSchema> sort = builder.Descending("record");
            InfectionSchema data = _Infections.Find(item => item.year == currentYear).Sort(sort).FirstOrDefault();

            if (data == null)
            {
                return "000";
            }
            return data.record;
        }
    }
}