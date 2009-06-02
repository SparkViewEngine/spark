using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PdfRendering.Model
{
    public static class InvoiceExtensions
    {
        public static Decimal TotalPrice(this LineItem item)
        {
            return item.Quantity * item.Product.Price;
        }

        public static Decimal TotalPrice(this Invoice invoice)
        {
            return invoice.Items.Select(item => item.Quantity * item.Product.Price).Sum();
        }
    }
}
