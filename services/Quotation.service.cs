
using backend.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;

namespace backend.Services
{
    public class QuotationService
    {
        private readonly IMongoCollection<Quotation> quotation;

        public QuotationService(ICompanieDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            quotation = database.GetCollection<Quotation>("Quotation");
        }

        public Quotation Create(Quotation body)
        {
            quotation.InsertOne(body);
            return body;
        }

        public List<Quotation> getData() {
            return quotation.Find(item => true).ToList();
        }

        public Quotation getByWasteName(string wasteName) {
            return quotation.Find(item => item.wasteName == wasteName).FirstOrDefault();
        }

        public void deleteQuotation(string id) {
            quotation.DeleteOne(item => item._id == id);
        }

        public void updateItem(string id, Quotation bookIn) {
            quotation.ReplaceOne(book => book._id == id, bookIn);
        }
    }
}