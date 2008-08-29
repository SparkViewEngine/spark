using System;
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
using Castle.MonoRail.Framework;

namespace WindsorInversionOfControl.Controllers
{
    [Layout("account")]
    public class AccountController : SmartDispatcherController
    {
        public void Login()
        {
            
        }

        public void Register()
        {

        }
    }
}
