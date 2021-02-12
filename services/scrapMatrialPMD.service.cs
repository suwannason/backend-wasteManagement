

using backend.Models;
using backend.request;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using System;


namespace backend.Services
{
    public class ScrapMatrialPMDService
    {
        private readonly IMongoCollection<ScrapMatrialpmdSchema> _pmd;
        public ScrapMatrialPMDService(ICompanieDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _pmd = database.GetCollection<ScrapMatrialpmdSchema>("ScrapMatrialPMD");
        }

        public void create()
        {

        }
        public void updateStatus(string lotNo, string status)
        {
            FilterDefinition<ScrapMatrialpmdSchema> filter = Builders<ScrapMatrialpmdSchema>.Filter.Eq(item => item.lotNo, lotNo);
            UpdateDefinition<ScrapMatrialpmdSchema> update = Builders<ScrapMatrialpmdSchema>.Update.Set("status", status);

            _pmd.UpdateMany(filter, update);
        }
    }
}