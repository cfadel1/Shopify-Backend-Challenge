using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public class Merchant
    {
        public int available_cookies { get; set; }
        public List<Orders> orders { get; set; }
        public Pagination pagination { get; set; }
    }
    public class Orders
    {
        public bool fulfilled { get; set; }
        public int id { get; set; }
        public List<Product> products { get; set; }
    }
    public class Product
    {
        public int amount { get; set; }
        public string title { get; set; }
    }
    public class Pagination
    {
        public string total { get; set; }
    }
    public class Output
    {
        public int remaining_cookies { get; set; }
        public List<int> unfulfilled_orders { get; set; }
    }
}
