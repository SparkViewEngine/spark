using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Web;
using System.Web.Mvc;
using MvcContrib.ViewFactories;
using Spark;
using DefaultControllerFactory = MvcContrib.ControllerFactories.DefaultControllerFactory;

namespace MvcContrib.SparkViewEngine
{
	[AspNetHostingPermission(SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public class SparkControllerFactory : DefaultControllerFactory
	{
        public SparkControllerFactory()
        {
            ViewSourceLoader = new FileSystemViewSourceLoader();
        }

        public ISparkSettings Settings { get; set; }

        public IViewSourceLoader ViewSourceLoader { get; set; }

		protected override IController CreateController(System.Web.Routing.RequestContext requestContext, string controllerName)
		{
			var controller = base.CreateController(requestContext, controllerName);
			var c = controller as Controller;
			if (c != null)
                c.ViewEngine = new SparkViewFactory(Settings) { ViewSourceLoader = ViewSourceLoader};
			return controller;
		}
	}
}
