using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.IO;
using System.Runtime.Serialization.Json;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            RunAsync().Wait();
        }

        static async Task RunAsync()
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://backend-challenge-fall-2017.herokuapp.com/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpResponseMessage response = await client.GetAsync("orders.json");

                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        List<Orders> ordersList = new List<Orders>();
                        List<Tuple<int, int>> productsList = new List<Tuple<int, int>>();
                        Merchant order = await response.Content.ReadAsAsync<Merchant>();
                        List<Merchant> merchantsList = new List<Merchant>();
                        merchantsList.Add(order);
                        int numbOfPages = 1;

                        if (int.TryParse(order.pagination.total, out numbOfPages))
                        {
                            for (int i = 2; i <= numbOfPages; i++)
                            {
                                response = await client.GetAsync("orders.json?page=" + i);
                                if (response.IsSuccessStatusCode)
                                {
                                    order = await response.Content.ReadAsAsync<Merchant>();
                                    merchantsList.Add(order);
                                }
                            }
                        }

                        ExtractOrdersFromMerchands(merchantsList, ordersList);
                        ExtractProducstFromOrders(ordersList, productsList);
                        List<Tuple<int, int>> CookieOrderList = FulfillCustomerWithCookies(productsList, order);
                        OutputJSON(order, CookieOrderList);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
                //Pause the Console Appplication
                Console.ReadLine();
            }
        }
        private static void ExtractProducstFromOrders(List<Orders> ordersList, List<Tuple<int, int>> productsList)
        {
            foreach (Orders merchantOrder in ordersList)
            {
                foreach (Product merchantProduct in merchantOrder.products)
                {
                    if (merchantProduct.title == "Cookie")
                    {
                        Tuple<int, int> productTuple = new Tuple<int, int>(merchantOrder.id, merchantProduct.amount);
                        productsList.Add(productTuple);
                    }
                }
            }
        }
        private static List<Tuple<int, int>> FulfillCustomerWithCookies(List<Tuple<int, int>> productsList, Merchant order)
        {
            var list1 = productsList.OrderBy(l => l.Item2).ThenByDescending(l => l.Item1).ToList();

            for (int i = list1.Count - 1; i >= 0; i--)
            {
                if (list1.Select(x => x.Item2).ElementAt(i) <= order.available_cookies)
                {
                    order.available_cookies = (order.available_cookies - list1.Select(x => x.Item2).ElementAt(i));
                    list1.RemoveAt(i);
                }
            }
            return list1;
        }
        private static void ExtractOrdersFromMerchands(List<Merchant> merchantsList, List<Orders> ordersList)
        {
            foreach (Merchant merchant in merchantsList)
            {
                foreach (Orders merchantOrder in merchant.orders)
                {
                    if (!merchantOrder.fulfilled)
                    {
                        ordersList.Add(merchantOrder);
                    }
                }
            }
        }
        private static void OutputJSON(Merchant merchant, List<Tuple<int, int>> CookieOrderList)
        {
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(Output));
            Output output = new Output();
            MemoryStream ms = new MemoryStream();
            StreamReader sr = new StreamReader(ms);

            output.remaining_cookies = merchant.available_cookies;
            output.unfulfilled_orders = CookieOrderList.Select(x => x.Item1).OrderBy(p => p).ToList();

            ser.WriteObject(ms, output);
            ms.Position = 0;
            Console.WriteLine(sr.ReadToEnd());
        }
    }
}
