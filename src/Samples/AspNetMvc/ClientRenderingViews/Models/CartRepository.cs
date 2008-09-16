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
    public class CartRepository
    {

        static readonly Dictionary<Guid, Cart> _carts = new Dictionary<Guid, Cart>();

        public Cart GetCurrentCart(Guid cartId)
        {
            lock(_carts)
            {
                Cart cart;
                if (!_carts.TryGetValue(cartId, out cart))
                {
                    cart = new Cart();
                    _carts[cart.Id] = cart;
                }
                return cart;
            }
        }

        public Cart Reset(Guid cartId)
        {
            lock(_carts)
            {
                if (_carts.ContainsKey(cartId))
                    _carts.Remove(cartId);
                return GetCurrentCart(Guid.Empty);
            }
        }
    }
}
