using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using Castle.MicroKernel;

namespace WindsorInversionOfControl
{
    public class WindsorControllerFactory : IControllerFactory
    {
        private readonly IKernel _kernel;

        public WindsorControllerFactory(IKernel kernel)
        {
            _kernel = kernel;
        }

        public IController CreateController(RequestContext requestContext, string controllerName)
        {            
            return (IController)_kernel.Resolve(controllerName + "controller", typeof(IController));
        }

        public void DisposeController(IController controller)
        {
            _kernel.ReleaseComponent(controller);
        }
    }
}
