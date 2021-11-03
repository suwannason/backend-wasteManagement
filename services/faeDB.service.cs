
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
        public faeDBschema getFirst() {
            return _faedb.Find<faeDBschema>(item => true).FirstOrDefault();
        }
        public faeDBschema getByKind(string kind)
        {
            return _faedb.Find<faeDBschema>(item => item.kind == kind).FirstOrDefault<faeDBschema>();
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
            return _faedb.Find<faeDBschema>(item => item.biddingType.ToLower() == biddingType.ToLower()).FirstOrDefault();
        }
        public faeDBschema getByMatcodeAndKind(string matrialCode, string kind)
        {
            return _faedb.Find<faeDBschema>(item => item.matrialCode.ToLower() == matrialCode.ToLower() && item.kind.ToLower() == kind.ToLower()).FirstOrDefault();
        }

        public faeDBschema getByKind_matCode_matName(string kind, string matrialCode, string matrialName)
        {
            return _faedb.Find<faeDBschema>(item =>
            item.kind == kind
            && item.matrialCode == matrialCode
            && item.matrialName == matrialName
            )
            .FirstOrDefault();
        }
        public List<faeDBschema> getByMatCodeAndMatName(string matrialCode, string matrialName)
        {

            return _faedb.Find<faeDBschema>(item =>
                        item.matrialCode.ToLower() == matrialCode.ToLower()
                        && item.matrialName.ToLower() == matrialName.ToLower()
            ).ToList();
        }

        public faeDBschema getByBiddingTypeAndColor(string matrialCode, string matrialName, string biddingType, string color)
        {

            return _faedb.Find<faeDBschema>(item =>
                item.matrialCode.ToLower() == matrialCode.ToLower()
                && item.matrialName.ToLower() == matrialName.ToLower()
                && item.biddingType.ToLower() == biddingType.ToLower()
                && item.color.ToLower() == color.ToLower()
            ).FirstOrDefault();
        }
        public faeDBschema getByBiddingTypeAndColor(string matrialName, string biddingType, string color)
        {

            return _faedb.Find<faeDBschema>(item =>
                item.matrialName.ToLower() == matrialName.ToLower()
                && item.biddingType.ToLower() == biddingType.ToLower()
                && item.color.ToLower() == color.ToLower()
            ).FirstOrDefault();
        }
    }
}