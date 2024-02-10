using Catalog.API.Entities;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;

namespace Catalog.API.Data
{
    public class CatalogContextSeed
    {
        public static void SeedData(IMongoCollection<Product> productCollection)
        {
            //bool existProduct = productCollection.Find(p => true).Any();
            //if (!existProduct)
            //{
            //    productCollection.InsertManyAsync(GetPreconfiguredProducts());
            //}
        }

    }
}
