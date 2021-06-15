

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

        public void create(SummaryInvoiceSchema body)
        {
            _tb.InsertOne(body);
        }
        public List<SummaryInvoiceSchema> getByStatus(string status)
        {

            return _tb.Find<SummaryInvoiceSchema>(item => item.status == status).ToList<SummaryInvoiceSchema>();
        }

        public SummaryInvoiceSchema getById(string id)
        {
            return _tb.Find(item => item._id == id).FirstOrDefault();
        }
        public void updateStatus(string id, string status, request.Profile profile)
        {
            FilterDefinition<SummaryInvoiceSchema> filter = Builders<SummaryInvoiceSchema>.Filter.Eq(item => item._id, id);

            if (status == "checked")
            {
                UpdateDefinition<SummaryInvoiceSchema> update = Builders<SummaryInvoiceSchema>.Update.Set("status", status).Set("check", profile);
                _tb.UpdateOne(filter, update);

            }
            else if (status == "approved")
            {
                UpdateDefinition<SummaryInvoiceSchema> update = Builders<SummaryInvoiceSchema>.Update.Set("status", status).Set("approve", profile);
                _tb.UpdateOne(filter, update);
            }
        }

        public void updateTotal(string id, string totalPrice, string totalWeight)
        {
            FilterDefinition<SummaryInvoiceSchema> filter = Builders<SummaryInvoiceSchema>.Filter.Eq(item => item._id, id);
            UpdateDefinition<SummaryInvoiceSchema> update = Builders<SummaryInvoiceSchema>.Update.Set("totalWeight", totalWeight).Set("totalPrice", totalPrice);

            _tb.UpdateOne(filter, update);

        }

        public void updateToInvoice(string id)
        {
            FilterDefinition<SummaryInvoiceSchema> filter = Builders<SummaryInvoiceSchema>.Filter.Eq(item => item._id, id);
            UpdateDefinition<SummaryInvoiceSchema> update = Builders<SummaryInvoiceSchema>.Update.Set("status", "toInvoice");

            _tb.UpdateOne(filter, update);
        }
    
        public List<SummaryInvoiceSchema> ITC_getsummary_approved() {
            return _tb.Find<SummaryInvoiceSchema>(item => item.status == "approved" && item.type == "Summary BOI/Non-BOI").ToList();
        }
    }
}