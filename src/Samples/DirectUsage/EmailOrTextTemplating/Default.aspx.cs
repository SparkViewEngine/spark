using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using EmailOrTextTemplating.Services;

namespace EmailOrTextTemplating
{
    public partial class _Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Label1.Text = MessageBuilder.Current.Transform(
                "Welcome",
                new
                    {
                        siteroot = "http://example.com",
                        user = new { id = 65321, name = "Fred", email = "fred.foo@bar.com" }
                    });

            Label2.Text = MessageBuilder.Current.Transform(
                "ForgotPassword",
                new
                    {
                        siteroot = "http://example.com",
                        user = new { name = "Fred", email = "fred.foo@bar.com" },
                        CryptoTimeLimitedServerVerifyingUserIdCipher = "qf2n98rcjq39r8qh4ogf8fhqo984fh892398fh7q4"
                    });


            Label3.Text = MessageBuilder.Current.Transform(
                "Promo42b",
                new
                    {
                        siteroot = "http://example.com",
                        user = new { id = 65321, name = "Fred", email = "fred.foo@bar.com" },
                        promo = new { size = 30000, msgnum = 24376, dist = "C3" },
                        product = new { id = 938, name = "Green Flashlight", price = 19.95, discount = 5 }
                    });
        }
    }
}
