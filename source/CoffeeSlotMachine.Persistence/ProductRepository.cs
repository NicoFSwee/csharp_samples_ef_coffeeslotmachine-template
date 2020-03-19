using CoffeeSlotMachine.Core.Contracts;
using CoffeeSlotMachine.Core.Entities;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace CoffeeSlotMachine.Persistence
{
    public class ProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public ProductRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IEnumerable<Product> GetProducts()
        {
            List<Product> products = new List<Product>();

            using (_dbContext)
            {
                foreach (var product in _dbContext.Products)
                {
                    products.Add(product);
                }

                _dbContext.SaveChanges();
            }

            return products.OrderBy(p => p.Name);
        }
    }
}
