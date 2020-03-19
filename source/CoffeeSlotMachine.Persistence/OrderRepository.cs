using System;
using CoffeeSlotMachine.Core.Contracts;
using CoffeeSlotMachine.Core.Entities;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace CoffeeSlotMachine.Persistence
{
    public class OrderRepository : IOrderRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public OrderRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IEnumerable<Order> GetAllWithProduct()
        {
            List<Order> resultList = new List<Order>();

            foreach (var order in _dbContext.Orders)
            {
                resultList.Add(order);
            }

            return resultList;
        }

        public void AddOrder(Order newOrder) => _dbContext.Orders.Add(newOrder);
    }
}
