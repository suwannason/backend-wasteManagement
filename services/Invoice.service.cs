

using backend.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using System;
using backend.request;

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

        public void Create(Invoices data)
        {
            invoice.InsertOne(data);
        }

        public void Update(string id, Invoices bookIn)
        {
            invoice.DeleteOne(item => item._id == id);
            invoice.InsertOne(bookIn);
            // invoice.ReplaceOne(book => book._id == id, bookIn);
        }

        // prepared --> checked --> approved --> makingApproved
        public void updateStatus(string id, string status, Profile user)
        {
            var filter = Builders<Invoices>.Filter.Eq(item => item._id, id);
            UpdateDefinition<Invoices> update = null;
            if (status == "fae-checked")
            {
                update = Builders<Invoices>.Update.Set("status", status).Set("fae_checked", user);
            }
            else if (status == "fae-approved")
            {
                update = Builders<Invoices>.Update.Set("status", status).Set("fae_approved", user);
            }
            else if (status == "makingApproved")
            {
                update = Builders<Invoices>.Update.Set("status", status).Set("gm_approved", user);
            }
            else if (status == "acc-checked")
            {
                update = Builders<Invoices>.Update.Set("status", status).Set("acc_check", user);
            }
            else if (status == "acc-approved")
            {
                update = Builders<Invoices>.Update.Set("status", status).Set("acc_approve", user);
            }
            invoice.UpdateMany(filter, update);
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

        public Invoices getById(string id)
        {
            return invoice.Find<Invoices>(item => item._id == id).FirstOrDefault();
        }

        public void accPrepare(string id, string dueDate, string invoiceNo, string termsOfPayment)
        {
            FilterDefinition<Invoices> filter = Builders<Invoices>.Filter.Eq(item => item._id, id);
            UpdateDefinition<Invoices> update = Builders<Invoices>.Update.Set("status", "acc-prepared")
                                                .Set("termsOfPayment", termsOfPayment)
                                                .Set("dueDate", dueDate)
                                                .Set("invoiceNo", invoiceNo);

            invoice.UpdateOne(filter, update);
        }
        public void rejectInvoice(string id, string commend)
        {
            FilterDefinition<Invoices> filter = Builders<Invoices>.Filter.Eq(item => item._id, id);
            UpdateDefinition<Invoices> update = Builders<Invoices>.Update
            .Set("rejectCommend", commend)
            .Set("status", "reject");

            invoice.UpdateOne(filter, update);
        }
    }
}