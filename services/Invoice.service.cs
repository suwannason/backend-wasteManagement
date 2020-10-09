

using backend.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;


namespace backend.Services {
    public class InvoiceService {
        private readonly IMongoCollection<Invoices> invoice;

        public InvoiceService(ICompanieDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            invoice = database.GetCollection<Invoices>("Invoices");
        }

        public List<Invoices> Get(string year)
        {
            return invoice.Find<Invoices>(invoice => invoice.year == year).ToList();
        }

        public Invoices Create(Invoices body)
        {
            invoice.InsertOne(body);
            return body;
        }

        public void Update(string id, Invoices bookIn)
        {
            invoice.ReplaceOne(book => book._id == id, bookIn);
        }
    }
}