using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PdfRendering.Model
{
    public class InvoiceRepository
    {
        public Invoice GetInvoice(string invoiceNumber)
        {
            return new Invoice
                   {
                       InvoiceNumber = invoiceNumber,
                       PurchaseOrder = "00394773",
                       ShipTo = new Account
                                {
                                    Name = "Ford Prefect",
                                    Address1 = "301 North 5th Street",
                                    City = "Western Exxon",
                                    State = "MN",
                                    Zip = "655321"
                                },
                       BillTo = new Account
                                {
                                    Name = "The Guide",
                                    Address1 = "P.O. Box 54553",
                                    Address2 = "Attn: Expenses"
                                },
                       Items = new[]
                               {
                                   new LineItem
                                   {
                                       Line = 1,
                                       Product = new Product
                                                 {
                                                     PartNumber = "t34234",
                                                     Name = "Tea Pot",
                                                     Price = 19.95m
                                                 },
                                       Quantity = 1,
                                   },
                                   new LineItem
                                   {
                                       Line = 1,
                                       Product = new Product
                                                 {
                                                     PartNumber = "51 12 8 041 192",
                                                     Name = "BMW 3 Series",
                                                     Price = 42103.00m
                                                 },
                                       Quantity = 3,
                                   },
                               },
                   };
        }
    }
}
