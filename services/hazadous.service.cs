
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using backend.Models;
using backend.request;

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

        public List<HazadousSchema> getAll()
        {
            return _Hazadous.Find<HazadousSchema>(item => true).ToList();
        }
        public void create(ReuqesterREQ body, Profile req_prepare, string trackingId)
        {
            List<HazadousSchema> items = new List<HazadousSchema>();

            Parallel.ForEach(body.hazardous, item =>
            {
                items.Add(new HazadousSchema
                {
                    date = body.date,
                    dept = body.date,
                    time = body.time,
                    div = body.div,
                    trackingId = trackingId,
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

        public void updateStatus(string id, string status)
        {
            FilterDefinition<HazadousSchema> filter = Builders<HazadousSchema>.Filter.Eq(item => item._id, id);
            UpdateDefinition<HazadousSchema> update = Builders<HazadousSchema>.Update.Set("status", status);

            _Hazadous.UpdateOne(filter, update);
        }

    }
}