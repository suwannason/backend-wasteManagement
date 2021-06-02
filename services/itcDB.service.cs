
using MongoDB.Driver;
using backend.Models;
using System.Collections.Generic;

namespace backend.Services {

    public class itcDBservice {
        
        private readonly IMongoCollection<ITCDB> _itcdb;

        public itcDBservice(ICompanieDatabaseSettings settings) {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _itcdb = database.GetCollection<ITCDB>("itcDB");
        }

        public void replace(List<ITCDB> items) {
            _itcdb.DeleteMany(Builders<ITCDB>.Filter.Empty);

            _itcdb.InsertMany(items);
        }

        public ITCDB matCode_name(string matrialCode) {

            return _itcdb.Find<ITCDB>(item => item.matrialCode == matrialCode).FirstOrDefault();
        }
    }
}