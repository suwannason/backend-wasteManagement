
using backend.Models;
using backend.request;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using System;

namespace backend.Services
{

    public class prepareLotService
    {
        private readonly IMongoCollection<FAEPreparedLotSchema> _prepareLot;

        public prepareLotService(ICompanieDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _prepareLot = database.GetCollection<FAEPreparedLotSchema>("Preparedlot");
        }


        public void create(FAEPreparedLotSchema body)
        {
            _prepareLot.InsertOne(body);
        }

    }
}