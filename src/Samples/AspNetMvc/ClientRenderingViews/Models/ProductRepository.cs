using System;
using System.Collections.Generic;
using System.Data;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;

namespace ClientRenderingViews.Models
{
    public class ProductRepository
    {
        public IList<Product> GetProducts()
        {
            var appData = (string)AppDomain.CurrentDomain.GetData("DataDirectory");
            var ser = new DataContractSerializer(typeof (List<Product>));
            using (var stream = new FileStream(Path.Combine(appData, "Products.xml"), FileMode.Open, FileAccess.Read))
            {
                return (IList<Product>) ser.ReadObject(stream);
            }
        }

        public Product FindProduct(int id)
        {
            return GetProducts().FirstOrDefault(p => p.Id == id);
        }
    }

}
