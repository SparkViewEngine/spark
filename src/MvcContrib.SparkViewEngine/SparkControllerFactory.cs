using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Web;
using System.Web.Mvc;
using DefaultControllerFactory = MvcContrib.ControllerFactories.DefaultControllerFactory;

namespace MvcContrib.SparkViewEngine
{
	[AspNetHostingPermission(SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public class SparkControllerFactory : DefaultControllerFactory
	{
		protected override IController CreateController(System.Web.Routing.RequestContext requestContext, string controllerName)
		{
			var controller = base.CreateController(requestContext, controllerName);
			var c = controller as Controller;
			if (c != null)
				c.ViewEngine = new SparkViewFactory();
			return controller;
		}
	}
}
