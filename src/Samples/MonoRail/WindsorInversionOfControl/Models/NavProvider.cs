using System;
using Castle.Core.Logging;
using Castle.MonoRail.Framework;
using Castle.MonoRail.Framework.Services;

namespace WindsorInversionOfControl.Models
{
    public interface INavProvider
    {
        NavData GetNav(IEngineContext context, string section);
    }

    public class NavProvider : INavProvider
    {
        private ILogger _logger = NullLogger.Instance;
        public ILogger Logger
        {
            get { return _logger; }
            set { _logger = value; }
        }

        public NavData GetNav(IEngineContext context, string section)
        {
            Logger.Debug("Getting nav for {0}", section);

            if (section == "primary")
            {
                return new NavData
                           {
                               Items = new[]
                                           {
                                               Item(context, "Home", "home", "index"),
                                               Item(context, "About", "home", "about"),
                                               Item(context, "Login", "account", "login")
                                           }
                           };
            }

            if (section == "contents")
            {
                return new NavData
                           {
                               Items = new[]
                                           {
                                               Item(context, "Products", "products", "index"),
                                               Item(context, "About", "home", "about"),
                                               Item(context, "Contact Us", "home", "contact"),
                                               Item(context, "Register", "account", "register")
                                           }
                           };
            }

            throw new ApplicationException("Unknown nav section " + section);
        }

        static NavItem Item(IEngineContext context, string caption, string controller, string action)
        {
            var parameters = new UrlBuilderParameters(controller, action);
            return new NavItem
                       {
                           Caption = caption,
                           Url = context.Services.UrlBuilder.BuildUrl(context.UrlInfo, parameters)
                       };
        }
    }
}
