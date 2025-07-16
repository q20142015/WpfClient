using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace WpfClient
{
    internal class Data
    {
        public string ProductCode { get; set; }
        public int Amount { get; set; }

        public Data(string productCode, int amount)
        {
            ProductCode = productCode;
            Amount = amount;
        }

    }
}
