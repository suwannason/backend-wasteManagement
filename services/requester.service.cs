
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;

using backend.Models;
using backend.request;

namespace backend.Services
{

    public class RequesterService
    {

        private readonly IMongoCollection<RequesterSchema> _requester;

        public RequesterService(ICompanieDatabaseSettings settings)
        {

            MongoClientBase client = new MongoClient(settings.ConnectionString);
            IMongoDatabase database = client.GetDatabase(settings.DatabaseName);

            _requester = database.GetCollection<RequesterSchema>("Requester");
        }

        public List<RequesterSchema> getAll() {
            return _requester.Find<RequesterSchema>(item => true).ToList();
        }
        public void create(ReuqesterREQ body, Profile req_prepare)
        {
            // _requester.InsertMany
            List<RequesterSchema> items = new List<RequesterSchema>();
            string g = System.Guid.NewGuid().ToString();

            foreach (FormA item in body.formA)
            {
                items.Add(new RequesterSchema
                {
                    date = body.date,
                    dept = body.date,
                    div = body.div,
                    trackingId = g,
                    bindingType = item.bindingType,
                    boiType = item.boiType,
                    companyApproveNo = item.companyApproveNo,
                    containerType = item.containerType,
                    contracterCompany = item.contracterCompany,
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

                    req_prepared = req_prepare,
                    req_checked = new Profile { empNo = "-", name = "-", band = "-", dept = "-", div = "-" },
                    req_approved = new Profile { empNo = "-", name = "-", band = "-", dept = "-", div = "-" },

                    fae_prepared = new Profile { empNo = "-", name = "-", band = "-", dept = "-", div = "-" },
                    fae_checked = new Profile { empNo = "-", name = "-", band = "-", dept = "-", div = "-" },
                    fae_approved = new Profile { empNo = "-", name = "-", band = "-", dept = "-", div = "-" },
                });
            }
            // foreach (FormB item in body.fromB)
            // {

            // }
            _requester.InsertMany(items);



        }

    }
}