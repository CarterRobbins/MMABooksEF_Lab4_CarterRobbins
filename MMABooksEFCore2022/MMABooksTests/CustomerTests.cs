using System.Collections.Generic;
using System.Linq;
using System;

using NUnit.Framework;
using MMABooksEFClasses.MarisModels;
using Microsoft.EntityFrameworkCore;

namespace MMABooksTests
{
    [TestFixture]
    public class CustomerTests
    {
        
        MMABooksContext dbContext;
        Customer? c;
        List<Customer>? customers;

        [SetUp]
        public void Setup()
        {
            dbContext = new MMABooksContext();
            dbContext.Database.ExecuteSqlRaw("call usp_testingResetData()");
        }

        [Test]
        public void GetAllTest()
        {
            customers = dbContext.Customers
                     .OrderBy(x => x.Name)
                     .ToList();

            Assert.IsNotNull(customers);
            Assert.Greater(customers.Count, 0);
            PrintAll(customers);
        }

        [Test]
        public void GetByPrimaryKeyTest()
        {
            var id = dbContext.Customers
                  .Select(x => x.CustomerId)
                  .OrderBy(x => x)
                  .First();

            c = dbContext.Customers.Find(id);
            Assert.IsNotNull(c);
            Assert.AreEqual(id, c!.CustomerId);
            Console.WriteLine(c);
        }

        [Test]
        public void GetUsingWhere()
        {
            var firstLetter = dbContext.Customers
                           .Select(x => x.Name.Substring(0, 1))
                           .First();

            customers = dbContext.Customers
                                 .Where(x => x.Name.StartsWith(firstLetter))
                                 .OrderBy(x => x.Name)
                                 .ToList();

            Assert.IsNotNull(customers);
            Assert.IsNotEmpty(customers);
            Assert.True(customers.All(x => x.Name.StartsWith(firstLetter)));

            PrintAll(customers);
        }

        [Test]
        public void GetWithInvoicesTest()
        {
            // get the customer whose id is 20 and all of the invoices for that customer
            var custIdWithInvoice = dbContext.Invoices
                                             .Select(i => i.CustomerId)
                                             .FirstOrDefault();
            Assert.Greater(custIdWithInvoice, 0, "Seed data should include at least one invoice.");

            var customerWithInvoices = dbContext.Customers
                                                .Include("Invoices")
                                                .SingleOrDefault(x => x.CustomerId == custIdWithInvoice);

            Assert.IsNotNull(customerWithInvoices);
            Assert.IsNotNull(customerWithInvoices!.Invoices);
            Assert.IsNotEmpty(customerWithInvoices.Invoices);
            Console.WriteLine($"{customerWithInvoices.CustomerId} {customerWithInvoices.Name} has {customerWithInvoices.Invoices.Count} invoices.");
        }

        [Test]
        public void GetWithJoinTest()
        {
            // get a list of objects that include the customer id, name, statecode and statename
            var customers = dbContext.Customers.Join(
               dbContext.States,
               c => c.StateCode,
               s => s.StateCode,
               (c, s) => new { c.CustomerId, c.Name, c.StateCode, s.StateName }).OrderBy(r => r.StateName).ToList();
            Assert.AreEqual(696, customers.Count);
            // I wouldn't normally print here but this lets you see what each object looks like
            foreach (var c in customers)
            {
                Console.WriteLine(c);
            }
        }

        [Test]
        public void DeleteTest()
        {
            //quick create to avoid errors
            c = new Customer
            {
                Name = "Temp Delete " + Guid.NewGuid().ToString("N").Substring(0, 6),
                Address = "123 Test Ave",
                City = "Testville",
                StateCode = dbContext.States.Select(s => s.StateCode).First(),
                ZipCode = "99999"
            };
            dbContext.Customers.Add(c);
            dbContext.SaveChanges();
            var id = c.CustomerId;

            // delete it
            dbContext.Customers.Remove(c);
            dbContext.SaveChanges();

            Assert.IsNull(dbContext.Customers.Find(id));
        }

        [Test]
        public void CreateTest()
        {
            c = new Customer
            {
                Name = "Temp Create " + Guid.NewGuid().ToString("N").Substring(0, 6),
                Address = "456 Create St",
                City = "Create City",
                StateCode = dbContext.States.Select(s => s.StateCode).First(),
                ZipCode = "00000"
            };

            dbContext.Customers.Add(c);
            dbContext.SaveChanges();

            var created = dbContext.Customers.Find(c.CustomerId);
            Assert.IsNotNull(created);
        }

        [Test]
        public void UpdateTest()
        {
            c = dbContext.Customers.OrderBy(x => x.CustomerId).First();
            var originalName = c.Name;

            c.Name = originalName + " *";
            dbContext.SaveChanges();

            var changed = dbContext.Customers.Find(c.CustomerId);
            Assert.IsNotNull(changed);
            Assert.AreEqual(originalName + " *", changed!.Name);

            // revert so reruns stay clean
            changed.Name = originalName;
            dbContext.SaveChanges();

            var reverted = dbContext.Customers.Find(c.CustomerId);
            Assert.IsNotNull(reverted);
            Assert.AreEqual(originalName, reverted!.Name);
        }

        public void PrintAll(List<Customer> customers)
        {
            foreach (Customer c in customers)
            {
                Console.WriteLine(c);
            }
        }
        
    }
}