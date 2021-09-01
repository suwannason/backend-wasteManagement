
using System.Collections.Generic;
using backend.Models;
using MongoDB.Driver;

namespace backend.Services
{

    public class InvoicePrintedService
    {

        private readonly IMongoCollection<InvoicePrintedSchema> _tb;

        public InvoicePrintedService(ICompanieDatabaseSettings setting)
        {
            MongoClient client = new MongoClient(setting.ConnectionString);
            IMongoDatabase database = client.GetDatabase(setting.DatabaseName);

            _tb = database.GetCollection<InvoicePrintedSchema>("InvoicePrinted");
        }

        public void create(InvoicePrintedSchema body)
        {
            _tb.InsertOne(body);
        }
        public List<InvoicePrintedSchema> getPrintedItem(string year, string month)
        {
            List<InvoicePrintedSchema> data = _tb.Find<InvoicePrintedSchema>(item => item.printingDate.Contains(year) && item.printingDate.Contains(month)).ToList();

            return data;
        }

        public List<InvoicePrintedSchema> getPrintByInvoice_id(string id)
        {
            return _tb.Find<InvoicePrintedSchema>(item => item.invoiceId == id).ToList();
        }
        public void setAttachmentDir(string invoiceId, string pathFile)
        {
            FilterDefinition<InvoicePrintedSchema> filter = Builders<InvoicePrintedSchema>.Filter.Eq(item => item.invoiceId, invoiceId);

            UpdateDefinition<InvoicePrintedSchema> update = Builders<InvoicePrintedSchema>.Update.Set("attatchmentFile", pathFile);

            _tb.UpdateMany(filter, update);
        }
    }
}