using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoffeeSlotMachine.Core.Entities
{
    /// <summary>
    /// Bestellung verwaltet das bestellte Produkt, die eingeworfenen Münzen und
    /// die Münzen die zurückgegeben werden.
    /// </summary>
    public class Order : EntityObject
    {
        /// <summary>
        /// Datum und Uhrzeit der Bestellung
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        /// Werte der eingeworfenen Münzen als Text. Die einzelnen 
        /// Münzwerte sind durch ; getrennt (z.B. "10;20;10;50")
        /// </summary>
        public String ThrownInCoinValues { get; set; }

        /// <summary>
        /// Zurückgegebene Münzwerte mit ; getrennt
        /// </summary>
        public String ReturnCoinValues { get; set; }

        /// <summary>
        /// Summe der eingeworfenen Cents.
        /// </summary>
        public int ThrownInCents => CalculateThrownInCents();

        private int CalculateThrownInCents()
        {
            string[] lines = ThrownInCoinValues.Split(";");
            int result = 0;

            foreach (var line in lines)
            {
                if(!String.IsNullOrWhiteSpace(line))
                {
                    result += Convert.ToInt32(line);
                }
            }

            return result;
        }


        /// <summary>
        /// Summe der Cents die zurückgegeben werden
        /// </summary>
        public int ReturnCents => CalculateReturnCents();

        private int CalculateReturnCents()
        {
            return ThrownInCents - Product.PriceInCents;
        }

        public int ProductId { get; set; }

        [ForeignKey(nameof(ProductId))]
        public Product Product { get; set; }

        /// <summary>
        /// Kann der Automat mangels Kleingeld nicht
        /// mehr herausgeben, wird der Rest als Spende verbucht
        /// </summary>
        [NotMapped]
        public int DonationCents { get; set; }

        /// <summary>
        /// Münze wird eingenommen.
        /// </summary>
        /// <param name="coinValue"></param>
        /// <returns>isFinished ist true, wenn der Produktpreis zumindest erreicht wurde</returns>
        public bool InsertCoin(int coinValue)
        {
            ThrownInCoinValues += $"{coinValue};";

            if(ThrownInCents >= Product.PriceInCents)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Übernahme des Einwurfs in das Münzdepot.
        /// Rückgabe des Retourgeldes aus der Kasse. Staffelung des Retourgeldes
        /// hängt vom Inhalt der Kasse ab.
        /// </summary>
        /// <param name="coins">Aktueller Zustand des Münzdepots</param>
        public void FinishPayment(IEnumerable<Coin> coins)
        {
            List<Coin> coinsList = (List<Coin>)coins;
            coinsList.Reverse();

            int restMoney = ReturnCents;
            int possibleSum = 0;

            foreach (var coin in coinsList)
            {
                if(coin.CoinValue <= restMoney)
                {
                    possibleSum += coin.CoinValue * coin.Amount;
                }
            }

            if(restMoney > 0 && restMoney <= possibleSum)
            {
                foreach (var coin in coinsList)
                {
                    while (restMoney >= coin.CoinValue && coin.Amount > 0)
                    {
                        if (coin.Amount > 0 && restMoney - coin.CoinValue >= 0)
                        {
                            if (String.IsNullOrEmpty(ReturnCoinValues))
                            {
                                ReturnCoinValues += $"{coin.CoinValue}";
                                coin.Amount--;
                            }
                            else
                            {
                                ReturnCoinValues += $";{coin.CoinValue}";
                                coin.Amount--;
                            }

                            restMoney -= coin.CoinValue;
                        }
                    }
                }
            }

            if(restMoney > 0)
            {
                DonationCents = restMoney;
            }
            else
            {
                DonationCents = 0;
            }
        }
    }
}
