using CoffeeSlotMachine.Core.Contracts;
using CoffeeSlotMachine.Core.Entities;
using CoffeeSlotMachine.Persistence;
using System.Linq;
using System;
using System.Collections.Generic;

namespace CoffeeSlotMachine.Core.Logic
{
    /// <summary>
    /// Verwaltet einen Bestellablauf. 
    /// </summary>
    public class OrderController : IDisposable
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ICoinRepository _coinRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;

        private Order _currentOrder = null;

        public OrderController()
        {
            _dbContext = new ApplicationDbContext();

            _coinRepository = new CoinRepository(_dbContext);
            _orderRepository = new OrderRepository(_dbContext);
            _productRepository = new ProductRepository(_dbContext);
        }


        /// <summary>
        /// Gibt alle Produkte sortiert nach Namen zurück
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Product> GetProducts() => _dbContext.Products
                                                                .OrderBy(p => p.Name)
                                                                .ToList();

        /// <summary>
        /// Eine Bestellung wird für das Produkt angelegt.
        /// </summary>
        /// <param name="product"></param>
        public Order OrderCoffee(Product product)
        {
            _currentOrder = new Order()
            {
                Time = DateTime.Now,
                Product = product,
            };

            _orderRepository.AddOrder(_currentOrder);

            return _currentOrder;
        }
        
        /// <summary>
        /// Münze einwerfen. 
        /// Wurde zumindest der Produktpreis eingeworfen, Münzen in Depot übernehmen
        /// und für Order Retourgeld festlegen. Bestellug abschließen.
        /// </summary>
        /// <returns>true, wenn die Bestellung abgeschlossen ist</returns>
        public bool InsertCoin(Order order, int coinValue)
        {
            if(order == null)
            {
                throw new ArgumentNullException(nameof(order), "Bestellung darf nicht Null sein");
            }

            if(!order.InsertCoin(coinValue))
            {
                return false;
            }
            else
            {
                List<Coin> currentDepot;
                string[] cents = order.ThrownInCoinValues.Split(";");

                foreach (var cent in cents)
                {
                    if(!String.IsNullOrWhiteSpace(cent))
                    {
                        Coin tmp = _dbContext.Coins.FirstOrDefault(c => c.CoinValue == Convert.ToInt32(cent));
                        tmp.Amount++;
                        _dbContext.Update(tmp);
                    }
                }
                    
                currentDepot = _dbContext.Coins.ToList<Coin>();
                order.FinishPayment(currentDepot);

                string[] returnCents = order.ReturnCoinValues.Split(";");

                foreach (var cent in returnCents)
                {
                    if (!String.IsNullOrWhiteSpace(cent))
                    {
                        Coin tmp = _dbContext.Coins.FirstOrDefault(c => c.CoinValue == Convert.ToInt32(cent));
                        tmp.Amount--;
                        _dbContext.Update(tmp);
                    }
                }

                _dbContext.SaveChanges();
                return true;
            }
        }

        /// <summary>
        /// Gibt den aktuellen Inhalt der Kasse, sortiert nach Münzwert absteigend zurück
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Coin> GetCoinDepot() => _dbContext.Coins.OrderByDescending(p => p.CoinValue);


        /// <summary>
        /// Gibt den Inhalt des Münzdepots als String zurück
        /// </summary>
        /// <returns></returns>
        public string GetCoinDepotString()
        {
            IEnumerable<Coin> coins = GetCoinDepot();
            string result = String.Empty;

            foreach (var coin in coins)
            {
                if(String.IsNullOrEmpty(result))
                {
                    result += $"{coin.Amount}*{coin.CoinValue}";
                }
                else
                {
                    result += $" + {coin.Amount}*{coin.CoinValue}";
                }
            }

            return result;
        }

        /// <summary>
        /// Liefert alle Orders inkl. der Produkte zurück
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Order> GetAllOrdersWithProduct() => _orderRepository.GetAllWithProduct();
        
        /// <summary>
        /// IDisposable:
        ///
        /// - Zusammenräumen (zB. des ApplicationDbContext).
        /// </summary>
        public void Dispose()
        {
            _dbContext?.Dispose();
        }
    }
}
