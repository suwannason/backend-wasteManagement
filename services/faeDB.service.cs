
using MongoDB.Driver;
using backend.Models;
using System.Collections.Generic;

namespace backend.Services
{

    public class faeDBservice
    {

        private readonly IMongoCollection<faeDBschema> _faedb;

        public faeDBservice(ICompanieDatabaseSettings settings)
        {

            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _faedb = database.GetCollection<faeDBschema>("Pricing");

        }

        public void replace(List<faeDBschema> items)
        {
            _faedb.DeleteMany(Builders<faeDBschema>.Filter.Empty);

            _faedb.InsertMany(items);
        }

        public List<faeDBschema> getWastename()
        {

            List<faeDBschema> wastename = _faedb.Find<faeDBschema>(item => true).Project<faeDBschema>("{wasteName: 1, biddingType: 1}").ToList();

            return wastename;
        }
        public faeDBschema getByMatcode(string matrialCode)
        {
            return _faedb.Find<faeDBschema>(item => item.matrialCode == matrialCode).FirstOrDefault<faeDBschema>();
        }
        public faeDBschema getByMatname(string matrialName)
        {
            return _faedb.Find<faeDBschema>(item => item.matrialName == matrialName).FirstOrDefault<faeDBschema>();
        }
    }
}