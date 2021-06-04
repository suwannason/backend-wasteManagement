
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

        public faeDBschema getByWasteName(string matrialCode, string kind)
        {
            // System.Console.WriteLine("matrialCode: " + matrialCode + " kind: " + kind);
            faeDBschema response = null;
            if (matrialCode == null)
            {
                response = _faedb.Find<faeDBschema>(item => item.wasteName == kind).FirstOrDefault();
            }
            else if (matrialCode.IndexOf("-") != -1)
            {
                response = _faedb.Find<faeDBschema>(item => item.matrialCode == matrialCode && item.kind == kind).FirstOrDefault();
            }
            else
            {
                response = _faedb.Find<faeDBschema>(item => item.biddingType == matrialCode).FirstOrDefault();
            }
            return response;
        }
        public faeDBschema getByBiddingType(string biddingType)
        {
            return _faedb.Find<faeDBschema>(item => item.biddingType == biddingType).FirstOrDefault();
        }

        public faeDBschema getByKind(string kind)
        {
            return _faedb.Find<faeDBschema>(item => item.kind == kind).FirstOrDefault();
        }
    }
}