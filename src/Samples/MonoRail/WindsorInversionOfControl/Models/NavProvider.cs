// Copyright 2008 Louis DeJardin - http://whereslou.com
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// 
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

        #region INavProvider Members

        public NavData GetNav(IEngineContext context, string section)
        {
            Logger.Debug("Getting nav for {0}", section);

            if (section == "navigation")
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

        #endregion

        private static NavItem Item(IEngineContext context, string caption, string controller, string action)
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