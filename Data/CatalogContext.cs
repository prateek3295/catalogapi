using Catalog.API.Data.Interfaces;
using Catalog.API.Entities;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Catalog.API.Data
{
    public class CatalogContext : ICatalogContext
    {
        public CatalogContext(IConfiguration configuration)
        {
            var mongoConnectionString = $"mongodb://{configuration.GetValue<string>("DatabaseSettings:MongoDbUser")}:{configuration.GetValue<string>("DatabaseSettings:MongoDbPassword")}" +
                $"@{configuration.GetValue<string>("DatabaseSettings:MongoDbHost")}:{configuration.GetValue<string>("DatabaseSettings:MongoDbPort")}";
            var client = new MongoClient(mongoConnectionString);
            var database = client.GetDatabase(configuration.GetValue<string>("DatabaseSettings:DatabaseName"));

            Products = database.GetCollection<Product>(configuration.GetValue<string>("DatabaseSettings:CollectionName"));
            CatalogContextSeed.SeedData(Products);
        }

        public IMongoCollection<Product> Products { get; }
    }
}
