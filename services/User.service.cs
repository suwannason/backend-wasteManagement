

using backend.Models;
using backend.request;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using System;

namespace backend.Services
{

    public class UserService
    {
        private readonly IMongoCollection<User> users;

        public UserService(ICompanieDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            users = database.GetCollection<User>("Users");
        }

        public User Get(string empNo)
        {
            return users.Find<User>(user => user.username == empNo).FirstOrDefault();
        }
    }
}