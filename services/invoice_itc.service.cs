
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

        public void create(ITCinvoiceSchema data)
        {
            ITCinvoiceSchema invoice = _tb.Find(item => item.summaryId == data.summaryId).FirstOrDefault();

            if (invoice != null) {
                FilterDefinition<ITCinvoiceSchema> filter = Builders<ITCinvoiceSchema>.Filter.Eq(item => item._id, invoice._id);
                invoice.files = data.files;
                invoice.status = "prepared";
                invoice.createDate = System.DateTime.Now.ToString("yyyy/MM/dd");
                invoice.prepare = invoice.prepare;

                _tb.ReplaceOne(filter, invoice);
            } else {
                _tb.InsertOne(data);
            }
            
        }
        public List<ITCinvoiceSchema> getByStatus(string status)
        {

            return _tb.Find<ITCinvoiceSchema>(item => item.status == status).ToList();
        }
        public void updateStatus(string id, string status)
        {
            FilterDefinition<ITCinvoiceSchema> filter = Builders<ITCinvoiceSchema>.Filter.Eq(item => item._id, id);
            UpdateDefinition<ITCinvoiceSchema> update = Builders<ITCinvoiceSchema>.Update.Set("status", status);
            _tb.UpdateOne(filter, update);
        }
    }

}