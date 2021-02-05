
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

using backend.Models;
using backend.request;


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

        public void create(ReuqesterREQ body, Profile req_prepare, string trackingId)
        {
            List<InfectionSchema> data = new List<InfectionSchema>();

            Parallel.ForEach(body.infections, item =>
            {
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
                    trackingId = trackingId,
                    valuableType = "Waiting for FAE",
                    wasteEachDept = item.wasteEachDept,
                    wasteName = item.wasteName,
                    weightPerContainer = item.weightPerContainer,
                    req_prepared = req_prepare,
                    status = "req-prepared",
                    req_checked = new Profile { empNo = "-", name = "-", band = "-", dept = "-", div = "-", tel = "-" },
                    req_approved = new Profile { empNo = "-", name = "-", band = "-", dept = "-", div = "-", tel = "-" },

                    fae_prepared = new Profile { empNo = "-", name = "-", band = "-", dept = "-", div = "-", tel = "-" },
                    fae_checked = new Profile { empNo = "-", name = "-", band = "-", dept = "-", div = "-", tel = "-" },
                    fae_approved = new Profile { empNo = "-", name = "-", band = "-", dept = "-", div = "-", tel = "-" },
                });
            });
            _Infections.InsertMany(data);
        }
    }
}