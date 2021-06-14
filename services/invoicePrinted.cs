
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
    }
}