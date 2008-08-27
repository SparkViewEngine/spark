using System;
using System.Collections.Generic;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;

namespace MediumTrustHosting.Models
{
    public class ProductRepository
    {
        public IList<Product> ListAll()
        {
            return new[]
                       {
                           new Product {Id = 1, Name = "Orange"},
                           new Product {Id = 2, Name = "Apple"},
                           new Product {Id = 3, Name = "Banana"}
                       };
        }

        public Product Fetch(int id)
        {
            return ListAll().FirstOrDefault(p => p.Id == id);
        }
    }
}
