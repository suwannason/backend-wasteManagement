

using backend.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;


namespace backend.Services
{

    public class UserService
    {
        private readonly IMongoCollection<UserSchema> users;

        public UserService(ICompanieDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            users = database.GetCollection<UserSchema>("Users");
        }

        public UserSchema Get(string empNo)
        {
            return users.Find<UserSchema>(user => user.username == empNo).FirstOrDefault();
        }
        public List<UserSchema> Getlist(string empNo)
        {
            return users.Find<UserSchema>(user => user.username == empNo).ToList();
        }
        public void create(List<UserSchema> data)
        {
            users.DeleteMany(item => item.username != "admin");
            users.InsertMany(data);
        }
        public UserSchema getLastRecord()
        {
            return users.Find<UserSchema>(item => true).SortBy(item => item.username).FirstOrDefault();
        }
    }
}