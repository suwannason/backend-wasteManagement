

using backend.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;

namespace backend.Services
{
    public class SummaryInvoiceService
    {

        private readonly IMongoCollection<SummaryInvoiceSchema> _tb;

        public SummaryInvoiceService(ICompanieDatabaseSettings settings)
        {
            MongoClient client = new MongoClient(settings.ConnectionString);
            IMongoDatabase database = client.GetDatabase(settings.DatabaseName);

            _tb = database.GetCollection<SummaryInvoiceSchema>("summaryInvoice");
        }

        public void create(SummaryInvoiceSchema body) {
            _tb.InsertOne(body);
        }
        public List<SummaryInvoiceSchema> getByStatus(string status) {

            return _tb.Find<SummaryInvoiceSchema>(item => item.status == status).ToList<SummaryInvoiceSchema>();
        }
        
        public SummaryInvoiceSchema getById(string id) {
            return _tb.Find(item => item._id == id).FirstOrDefault();
        }
    }
}