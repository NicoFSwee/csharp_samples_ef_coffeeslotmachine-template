using CoffeeSlotMachine.Core.Logic;
using CoffeeSlotMachine.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace CoffeeSlotMachine.ControllerTest
{
    [TestClass]
    public class ControllerTest
    {
        [TestInitialize]
        public void MyTestInitialize()
        {
            using (ApplicationDbContext applicationDbContext = new ApplicationDbContext())
            {
                applicationDbContext.Database.EnsureDeleted();
                applicationDbContext.Database.Migrate();
            }
        }


        [TestMethod]
        public void T01_GetCoinDepot_CoinTypesCount_ShouldReturn6Types_3perType_SumIs1155Cents()
        {
            using (OrderController controller = new OrderController())
            {
                var depot = controller.GetCoinDepot().ToArray();
                Assert.AreEqual(6, depot.Count(), "Sechs Münzarten im Depot");
                foreach (var coin in depot)
                {
                    Assert.AreEqual(3, coin.Amount, "Je Münzart sind drei Stück im Depot");
                }

                int sumOfCents = depot.Sum(coin => coin.CoinValue * coin.Amount);
                Assert.AreEqual(1155, sumOfCents, "Beim Start sind 1155 Cents im Depot");
            }
        }
        
        [TestMethod]
        public void T02_GetProducts_9Products_FromCappuccinoToRistretto()
        {
            using (OrderController statisticsController = new OrderController())
            {
                var products = statisticsController.GetProducts().ToArray();
                Assert.AreEqual(9, products.Length, "Neun Produkte wurden erzeugt");
                Assert.AreEqual("Cappuccino", products[0].Name);
                Assert.AreEqual("Ristretto", products[8].Name);
            }
        }
        
        [TestMethod]
        public void T03_BuyOneCoffee_OneCoinIsEnough_CheckCoinsAndOrders()
        {
            using (OrderController controller = new OrderController())
            {
                var products = controller.GetProducts();
                var product = products.Single(p => p.Name == "Cappuccino");
                var order = controller.OrderCoffee(product);
                bool isFinished = controller.InsertCoin(order, 100);
                Assert.AreEqual(true, isFinished, "100 Cent genügen");
                Assert.AreEqual(100, order.ThrownInCents, "Einwurf stimmt nicht");
                Assert.AreEqual(100 - product.PriceInCents, order.ReturnCents);
                Assert.AreEqual(0, order.DonationCents);
                Assert.AreEqual("20;10;5", order.ReturnCoinValues);

                // Depot überprüfen
                var coins = controller.GetCoinDepot().ToArray();
                int sumOfCents = coins.Sum(c => c.CoinValue * c.Amount);
                Assert.AreEqual(1220, sumOfCents, "Beim Start sind 1155 Cents + 65 Cents für Cappuccino");
                Assert.AreEqual("3*200 + 4*100 + 3*50 + 2*20 + 2*10 + 2*5", controller.GetCoinDepotString());

                var orders = controller.GetAllOrdersWithProduct().ToArray();
                Assert.AreEqual(1, orders.Length, "Es ist genau eine Bestellung");
                Assert.AreEqual(0, orders[0].DonationCents, "Keine Spende");
                Assert.AreEqual(100, orders[0].ThrownInCents, "100 Cents wurden eingeworfen");
                Assert.AreEqual("Cappuccino", orders[0].Product.Name, "Produktname Cappuccino");
            }
        }
        
        [TestMethod]
        public void T04_BuyOneCoffee_ExactThrowInOneCoin_CheckCoinsAndOrders()
        {
            using (OrderController controller = new OrderController())
            {
                var products = controller.GetProducts();
                var product = products.Single(p => p.Name == "Espresso");
                var order = controller.OrderCoffee(product);
                bool isFinished = controller.InsertCoin(order, 50);
                Assert.AreEqual(true, isFinished, "50 Cent genügen");
                Assert.AreEqual(50, order.ThrownInCents, "Einwurf stimmt nicht");
                Assert.AreEqual(50 - product.PriceInCents, order.ReturnCents);
                Assert.AreEqual(0, order.DonationCents);
                Assert.AreEqual(null, order.ReturnCoinValues);

                // Depot überprüfen
                var coins = controller.GetCoinDepot().ToArray();
                int sumOfCents = coins.Sum(c => c.CoinValue * c.Amount);
                Assert.AreEqual(1205, sumOfCents, "Beim Start sind 1155 Cents + 50 Cents für Espreso");
                Assert.AreEqual("3*200 + 3*100 + 4*50 + 3*20 + 3*10 + 3*5", controller.GetCoinDepotString());

                var orders = controller.GetAllOrdersWithProduct().ToArray();
                Assert.AreEqual(1, orders.Length, "Es ist genau eine Bestellung");
                Assert.AreEqual(0, orders[0].DonationCents, "Keine Spende");
                Assert.AreEqual(50, orders[0].ThrownInCents, "50 Cents wurden eingeworfen");
                Assert.AreEqual("Espresso", orders[0].Product.Name, "Produktname Espresso");
            }
        }
        
        [TestMethod]
        public void T05_BuyOneCoffee_MoreCoins_CheckCoinsAndOrders()
        {
            using (OrderController controller = new OrderController())
            {
                var products = controller.GetProducts();
                var product = products.Single(p => p.Name == "Espresso");
                var order = controller.OrderCoffee(product);
                bool isFinished = controller.InsertCoin(order, 20);
                isFinished = controller.InsertCoin(order, 10);
                isFinished = controller.InsertCoin(order, 10);
                isFinished = controller.InsertCoin(order, 10);
                Assert.AreEqual(true, isFinished, "50 Cent genügen");
                Assert.AreEqual(50, order.ThrownInCents, "Einwurf stimmt nicht");
                Assert.AreEqual(50 - product.PriceInCents, order.ReturnCents);
                Assert.AreEqual(0, order.DonationCents);
                Assert.AreEqual(null, order.ReturnCoinValues);

                // Depot überprüfen
                var coins = controller.GetCoinDepot().ToArray();
                int sumOfCents = coins.Sum(c => c.CoinValue * c.Amount);
                Assert.AreEqual(1205, sumOfCents, "Beim Start sind 1155 Cents + 50 Cents für Espreso");
                Assert.AreEqual("3*200 + 3*100 + 3*50 + 4*20 + 6*10 + 3*5", controller.GetCoinDepotString());

                var orders = controller.GetAllOrdersWithProduct().ToArray();
                Assert.AreEqual(1, orders.Length, "Es ist genau eine Bestellung");
                Assert.AreEqual(0, orders[0].DonationCents, "Keine Spende");
                Assert.AreEqual(50, orders[0].ThrownInCents, "50 Cents wurden eingeworfen");
                Assert.AreEqual("Espresso", orders[0].Product.Name, "Produktname Espresso");
            }
        }
        
        [TestMethod()]
        public void T06_BuyMoreCoffees_OneCoins_CheckCoinsAndOrders()
        {
            using (OrderController controller = new OrderController())
            {
                var products = controller.GetProducts();
                var product1 = products.Single(p => p.Name == "Espresso");
                var product2 = products.Single(p => p.Name == "Latte");
                var product3 = products.Single(p => p.Name == "Espresso");
                var order1 = controller.OrderCoffee(product1);
                bool isFinished1 = controller.InsertCoin(order1, 50);
                var order2 = controller.OrderCoffee(product2);
                bool isFinished2 = controller.InsertCoin(order2, 100);
                var order3 = controller.OrderCoffee(product3);
                bool isFinished3 = controller.InsertCoin(order3, 200);
                Assert.AreEqual(true, isFinished1, "50 Cent genügen");
                Assert.AreEqual(true, isFinished2, "100 Cent genügen");
                Assert.AreEqual(true, isFinished3, "200 Cent genügen");
                Assert.AreEqual(50, order1.ThrownInCents, "Einwurf stimmt nicht");
                Assert.AreEqual(100, order2.ThrownInCents, "Einwurf stimmt nicht");
                Assert.AreEqual(200, order3.ThrownInCents, "Einwurf stimmt nicht");
                Assert.AreEqual(50 - product1.PriceInCents, order1.ReturnCents);
                Assert.AreEqual(100 - product2.PriceInCents, order2.ReturnCents);
                Assert.AreEqual(200 - product3.PriceInCents, order3.ReturnCents);
                Assert.AreEqual(0, order1.DonationCents);
                Assert.AreEqual(0, order2.DonationCents);
                Assert.AreEqual(0, order3.DonationCents);
                Assert.AreEqual(null, order1.ReturnCoinValues);
                Assert.AreEqual("50", order2.ReturnCoinValues);
                Assert.AreEqual("100;50", order3.ReturnCoinValues);

                // Depot überprüfen
                var coins = controller.GetCoinDepot().ToArray();
                int sumOfCents = coins.Sum(c => c.CoinValue * c.Amount);
                Assert.AreEqual(1305, sumOfCents, "Beim Start sind 1155 Cents + 150 Cents für die Getränke");
                Assert.AreEqual("4*200 + 3*100 + 2*50 + 3*20 + 3*10 + 3*5", controller.GetCoinDepotString());

                var orders = controller.GetAllOrdersWithProduct().ToArray();
                Assert.AreEqual(3, orders.Length, "Es ist genau eine Bestellung");
                Assert.AreEqual(0, orders[0].DonationCents, "Keine Spende");
                Assert.AreEqual(0, orders[2].DonationCents, "Keine Spende");
                Assert.AreEqual(50, orders[0].ThrownInCents, "50 Cents wurden eingeworfen");
                Assert.AreEqual(100, orders[1].ThrownInCents, "100 Cents wurden eingeworfen");
                Assert.AreEqual(200, orders[2].ThrownInCents, "200 Cents wurden eingeworfen");
                Assert.AreEqual("Espresso", orders[0].Product.Name, "Produktname Espresso");
                Assert.AreEqual("Latte", orders[1].Product.Name, "Produktname Latte");
                Assert.AreEqual("Espresso", orders[2].Product.Name, "Produktname Espresso");
            }
        }

        [TestMethod()]
        public void T07_BuyMoreCoffees_UntilDonation_CheckCoinsAndOrders()
        {
            using (OrderController controller = new OrderController())
            {
                var products = controller.GetProducts();
                var product1 = products.Single(p => p.Name == "Espresso");
                var product2 = products.Single(p => p.Name == "Latte");
                var product3 = products.Single(p => p.Name == "Espresso");
                var product4 = products.Single(p => p.Name == "Latte");
                var product5 = products.Single(p => p.Name == "Latte");
                var product6 = products.Single(p => p.Name == "Doppio");
                var order1 = controller.OrderCoffee(product1);
                bool isFinished1 = controller.InsertCoin(order1, 100);
                var order2 = controller.OrderCoffee(product2);
                bool isFinished2 = controller.InsertCoin(order2, 100);
                var order3 = controller.OrderCoffee(product3);
                bool isFinished3 = controller.InsertCoin(order3, 100);
                var order4 = controller.OrderCoffee(product4);
                bool isFinished4 = controller.InsertCoin(order4, 100);
                var order5 = controller.OrderCoffee(product5);
                bool isFinished5 = controller.InsertCoin(order5, 100);
                var order6 = controller.OrderCoffee(product6);
                bool isFinished6 = controller.InsertCoin(order6, 100);
                Assert.AreEqual(100, order1.ThrownInCents, "Einwurf stimmt nicht");
                Assert.AreEqual(100, order2.ThrownInCents, "Einwurf stimmt nicht");
                Assert.AreEqual(100, order3.ThrownInCents, "Einwurf stimmt nicht");
                Assert.AreEqual(100, order4.ThrownInCents, "Einwurf stimmt nicht");
                Assert.AreEqual(100, order5.ThrownInCents, "Einwurf stimmt nicht");
                Assert.AreEqual(100, order6.ThrownInCents, "Einwurf stimmt nicht");
                Assert.AreEqual(100 - product1.PriceInCents, order1.ReturnCents);
                Assert.AreEqual(100 - product2.PriceInCents, order2.ReturnCents);
                Assert.AreEqual(100 - product3.PriceInCents, order3.ReturnCents);
                Assert.AreEqual(100 - product4.PriceInCents, order4.ReturnCents);
                Assert.AreEqual(100 - product5.PriceInCents, order5.ReturnCents);
                Assert.AreEqual(100 - product6.PriceInCents, order6.ReturnCents);
                Assert.AreEqual(0, order1.DonationCents);
                Assert.AreEqual(0, order2.DonationCents);
                Assert.AreEqual(0, order3.DonationCents);
                Assert.AreEqual(0, order4.DonationCents);
                Assert.AreEqual(0, order5.DonationCents);
                Assert.AreEqual(20, order6.DonationCents);
                Assert.AreEqual("50", order1.ReturnCoinValues);
                Assert.AreEqual("50", order2.ReturnCoinValues);
                Assert.AreEqual("50", order3.ReturnCoinValues);
                Assert.AreEqual("20;20;10", order4.ReturnCoinValues);
                Assert.AreEqual("20;10;10;5;5", order5.ReturnCoinValues);
                Assert.AreEqual(null, order6.ReturnCoinValues, "Keine Geldrückgabe wegen Donation");

                // Depot überprüfen
                var coins = controller.GetCoinDepot().ToArray();
                int sumOfCents = coins.Sum(c => c.CoinValue * c.Amount);
                Assert.AreEqual(1505, sumOfCents, "Beim Start sind 1155 Cents + 330 Cents für die Getränke und 20 Cents Donation");
                Assert.AreEqual("3*200 + 9*100 + 0*50 + 0*20 + 0*10 + 1*5", controller.GetCoinDepotString());

                var orders = controller.GetAllOrdersWithProduct().ToArray();
                Assert.AreEqual(6, orders.Length, "Es ist genau eine Bestellung");
                Assert.AreEqual(0, orders[0].DonationCents, "Keine Spende");
                Assert.AreEqual(0, orders[4].DonationCents, "Keine Spende");
                Assert.AreEqual(20, orders[5].DonationCents, "Keine Spende");
                Assert.AreEqual(100, orders[0].ThrownInCents, "100 Cents wurden eingeworfen");
                Assert.AreEqual(100, orders[1].ThrownInCents, "100 Cents wurden eingeworfen");
                Assert.AreEqual(100, orders[2].ThrownInCents, "100 Cents wurden eingeworfen");
                Assert.AreEqual(100, orders[3].ThrownInCents, "100 Cents wurden eingeworfen");
                Assert.AreEqual(100, orders[4].ThrownInCents, "100 Cents wurden eingeworfen");
                Assert.AreEqual("Espresso", orders[0].Product.Name, "Produktname Espresso");
                Assert.AreEqual("Latte", orders[1].Product.Name, "Produktname Latte");
                Assert.AreEqual("Espresso", orders[2].Product.Name, "Produktname Espresso");
                Assert.AreEqual("Latte", orders[3].Product.Name, "Produktname Latte");
                Assert.AreEqual("Latte", orders[4].Product.Name, "Produktname Latte");
                Assert.AreEqual("Doppio", orders[5].Product.Name, "Produktname Doppio");
            }
        }
        
    }
}
