

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

        public List<User> Get()
        {
            return users.Find(data => true).ToList();
        }

        public User Get(string id)
        {
            return users.Find<User>(user => user._id == id).FirstOrDefault();
        }

        public User Create(User body)
        {
            users.InsertOne(body);
            return body;
        }

        public void Update(string id, User bookIn)
        {
            users.ReplaceOne(book => book._id == id, bookIn);
        }

        public void Remove(User bookIn)
        {
            users.DeleteOne(book => book._id == bookIn._id);
        }

        public void Remove(string id)
        {
            users.DeleteOne(book => book._id == id);
        }

        public User Login(string username, string password)
        {
            User data = users.Find(record => record.username == username && record.password == password).FirstOrDefault();

            return data;
        }
        public void changePassword(User body)
        {
            var filter = Builders<User>.Filter.Eq(item => item.username, body.username);
            var update = Builders<User>.Update
            .Set("password", body.password)
            .Set("name", body.name)
            .Set("canLogin", true)
            .Set("email", body.email)
            .Set("band", body.band)
            .Set("dept", body.dept)
            .Set("div", body.div)
            .Set("tel", body.tel);

            users.UpdateOne(filter, update);
        }
    }
}