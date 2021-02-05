
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

using backend.Models;
using backend.request;


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

        public void create(ReuqesterREQ body, Profile req_prepare, string trackingId)
        {
            List<ScrapMatrialimoSchema> data = new List<ScrapMatrialimoSchema>();

            // Parallel.ForEach(body.scrapImo, item =>
            // {
            //     data.Add(new ScrapMatrialimoSchema
            //     {
            //         no = item.no,
            //         dept = body.dept,
            //         div = body.div,
            //         date = body.date,
            //         time = body.time,
            //         biddingType = "-",
            //         containerType = item.containerType,

            //         qtyOfContainer = item.qtyOfContainer,
                
            //         totalWeight = item.totalWeight,
            //         req_prepared = req_prepare,
            //         status = "req-prepared",
            //         req_checked = new Profile { empNo = "-", name = "-", band = "-", dept = "-", div = "-", tel = "-" },
            //         req_approved = new Profile { empNo = "-", name = "-", band = "-", dept = "-", div = "-", tel = "-" },

            //         fae_checked = new Profile { empNo = "-", name = "-", band = "-", dept = "-", div = "-", tel = "-" },
            //         fae_approved = new Profile { empNo = "-", name = "-", band = "-", dept = "-", div = "-", tel = "-" },
            //     });
            // });
            // _scrapMatrial.InsertMany(data);
        }
    }
}