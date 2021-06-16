
using backend.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;

namespace backend.Services
{

    public class ITC_invoiceService
    {

        private readonly IMongoCollection<ITCinvoiceSchema> _tb;

        public ITC_invoiceService(ICompanieDatabaseSettings settings)
        {
            MongoClient client = new MongoClient(settings.ConnectionString);
            IMongoDatabase database = client.GetDatabase(settings.DatabaseName);

            _tb = database.GetCollection<ITCinvoiceSchema>("invoice_itc");
        }

        public void create(ITCinvoiceSchema data) {
            _tb.InsertOne(data);
        }
        public List<ITCinvoiceSchema> getByStatus(string status) {

            return _tb.Find<ITCinvoiceSchema>(item => item.status == status).ToList();
        }
    }

}