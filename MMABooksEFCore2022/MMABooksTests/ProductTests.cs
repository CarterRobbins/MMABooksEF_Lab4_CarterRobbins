using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using MMABooksEFClasses.Models;


namespace MMABooksTests
{
    [TestFixture]
    public class ProductTests
    {
        
        MMABooksContext dbContext;
        Product? p;
        List<Product> products;

        [SetUp]
        public void Setup()
        {
            dbContext = new MMABooksContext();
            dbContext.Database.ExecuteSqlRaw("call usp_testingResetData()");
        }

        [Test]
        public void GetAllTest()
        {
            products = dbContext.Products
                    .OrderBy(x => x.Description)
                    .ToList();

            Assert.IsNotNull(products);
            Assert.Greater(products.Count, 0);
            foreach (var row in products) Console.WriteLine(row);
        }

        [Test]
        public void GetByPrimaryKeyTest()
        {
            var code = dbContext.Products
                    .Select(x => x.ProductCode)
                    .OrderBy(x => x)
                    .First();

            p = dbContext.Products.Find(code);
            Assert.IsNotNull(p);
            Assert.AreEqual(code, p!.ProductCode);
            Console.WriteLine(p); // print the product
        }

        [Test]
        public void GetUsingWhere()
        {
            // get a list of all of the products that have a unit price of 56.50
            products = dbContext.Products
                    .Where(x => x.UnitPrice == 56.50m)
                    .OrderBy(x => x.ProductCode)
                    .ToList();

            Assert.IsNotNull(products);
            Assert.IsNotEmpty(products);
            Assert.True(products.All(x => x.UnitPrice == 56.50m));
            foreach (var row in products) Console.WriteLine(row);
        }

        [Test]
        public void GetWithCalculatedFieldTest()
        {
            // get a list of objects that include the productcode, unitprice, quantity and inventoryvalue
            var products = dbContext.Products.Select(
            p => new { p.ProductCode, p.UnitPrice, p.OnHandQuantity, Value = p.UnitPrice * p.OnHandQuantity }).
            OrderBy(p => p.ProductCode).ToList();
            Assert.AreEqual(16, products.Count);
            foreach (var p in products)
            {
                Console.WriteLine(p);
            }
        }

        [Test]
        public void DeleteTest()
        {
            // create a temp product to delete
            p = new Product
            {
                ProductCode = "DL" + Guid.NewGuid().ToString("N").Substring(0, 6).ToUpperInvariant(),
                Description = "Temp Delete Product",
                UnitPrice = 1.00m,
                OnHandQuantity = 1
            };
            dbContext.Products.Add(p);
            dbContext.SaveChanges();

            var code = p.ProductCode;

            dbContext.Products.Remove(p);
            dbContext.SaveChanges();

            Assert.IsNull(dbContext.Products.Find(code));

        }

        [Test]
        public void CreateTest()
        {
            p = new Product
            {
                ProductCode = "EF" + Guid.NewGuid().ToString("N").Substring(0, 6).ToUpperInvariant(),
                Description = "EF Temp Product",
                UnitPrice = 12.34m,
                OnHandQuantity = 5
            };

            dbContext.Products.Add(p);
            dbContext.SaveChanges();

            var created = dbContext.Products.Find(p.ProductCode);
            Assert.IsNotNull(created);

        }

        [Test]
        public void UpdateTest()
        {
            p = dbContext.Products.OrderBy(x => x.ProductCode).First();
            var originalPrice = p.UnitPrice;

            p.UnitPrice = originalPrice + 1.00m;
            dbContext.SaveChanges();

            var changed = dbContext.Products.Find(p.ProductCode);
            Assert.IsNotNull(changed);
            Assert.AreEqual(originalPrice + 1.00m, changed!.UnitPrice);

            // revert for clean reruns
            changed.UnitPrice = originalPrice;
            dbContext.SaveChanges();

            var reverted = dbContext.Products.Find(p.ProductCode);
            Assert.IsNotNull(reverted);
            Assert.AreEqual(originalPrice, reverted!.UnitPrice);
        }
       
    }
}