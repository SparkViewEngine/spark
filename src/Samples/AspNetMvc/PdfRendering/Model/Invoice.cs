using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PdfRendering.Model
{
    public class Invoice
    {
        public string InvoiceNumber { get; set; }
        public string PurchaseOrder { get; set; }
        public Account ShipTo { get; set; }
        public Account BillTo { get; set; }
        public IList<LineItem> Items { get; set; }
    }

    public class LineItem
    {
        public int Line { get; set; }
        public Product Product { get; set; }
        public decimal Quantity { get; set; }
    }

    public class Product
    {
        public string PartNumber { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
    }

    public class Account
    {
        public string Name { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }

        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
    }
}
