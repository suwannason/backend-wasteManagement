

using backend.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using System;


namespace backend.Services
{
    public class InvoiceService
    {
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
        public Invoices GetById(string id)
        {
            return invoice.Find<Invoices>(invoice => invoice._id == id).FirstOrDefault();
        }

        public void Create(List<Invoices> data)
        {
            invoice.InsertMany(data);
        }

        public void Update(string id, Invoices bookIn)
        {
            invoice.DeleteOne(item => item._id == id);
            invoice.InsertOne(bookIn);
            // invoice.ReplaceOne(book => book._id == id, bookIn);
        }

        // prepared --> checked --> approved --> makingApproved
        public void updateStatus(string id, string status)
        {
            var filter = Builders<Invoices>.Filter.Eq(item => item._id, id);
            var update = Builders<Invoices>.Update.Set("status", status);

            invoice.UpdateOne(filter, update);
        }

        public List<Invoices> getByStatus(string status)
        {
            List<Invoices> data = invoice.Find<Invoices>(item => item.status == status).ToList();
            return data;
        }
        public void deleteInvoice(string id)
        {
            // invoice.DeleteOne(item => item._id == id);
            var filter = Builders<Invoices>.Filter.Eq(item => item._id, id);
            var update = Builders<Invoices>.Update.Set("status", "deleted");

            invoice.UpdateOne(filter, update);
        }
    }
}