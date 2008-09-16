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

namespace ClientRenderingViews.Models
{
    public class Cart
    {
        public Cart()
        {
            Id = Guid.NewGuid();
            CreatedUtc = DateTime.UtcNow;
            ModifiedUtc = CreatedUtc;
            Items = new List<CartItem>();
        }

        public Guid Id { get; set; }
        public DateTime CreatedUtc { get; set; }
        public DateTime ModifiedUtc { get; set; }

        public IList<CartItem> Items { get; set; }
    }
}
