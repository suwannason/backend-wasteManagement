
using backend.Models;
using backend.request;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using System;

namespace backend.Services
{

    public class PricingService
    {
        private readonly IMongoCollection<pricingSchema> _pricing;

        public PricingService(ICompanieDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _pricing = database.GetCollection<pricingSchema>("Pricing");
        }


        public List<pricingSchema> getAll() {
            return _pricing.Find<pricingSchema>(item => true).ToList<pricingSchema>();
        }

    }
}