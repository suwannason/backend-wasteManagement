using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;

using backend.Models;

namespace backend.Services
{

    public class ITC_IMO_DB_service
    {

        private readonly IMongoCollection<ITC_IMO_SCHEMA> _imo;
        
        public ITC_IMO_DB_service(ICompanieDatabaseSettings settings)
        {

            MongoClientBase client = new MongoClient(settings.ConnectionString);
            IMongoDatabase database = client.GetDatabase(settings.DatabaseName);

            _imo = database.GetCollection<ITC_IMO_SCHEMA>("itc-imo-dbs");

            System.Console.WriteLine(_imo.CollectionNamespace.CollectionName);
        }
        public ITC_IMO_SCHEMA matCode_name(string matrialCode, string matrialName)
        {
            System.Console.WriteLine(matrialCode + " ===> " + matrialName);

            ITC_IMO_SCHEMA data = _imo.Find<ITC_IMO_SCHEMA>(item => item.matrialCode == matrialCode).FirstOrDefault();
            
            System.Console.WriteLine("DATA: " + data);
            return data;
        }
    }
}