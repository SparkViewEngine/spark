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
using System.Collections.Specialized;
using System.IO;
using System.Security.Principal;
using System.Web;
using Rhino.Mocks;

namespace Spark.Web.Mvc.Tests
{
    public static class MvcMockHelpers
    {
        public static HttpContextBase DynamicHttpContextBase(this MockRepository mocks)
        {
            return mocks.DynamicHttpContextBase
                (mocks.DynamicHttpBrowserCapabilitiesBase(),
                 mocks.DynamicHttpRequestBase(),
                 mocks.DynamicHttpResponseBase(),
                 mocks.DynamicHttpSessionStateBase(),
                 mocks.DynamicHttpServerUtilityBase(),
                 mocks.DynamicIPrincipal());
        }

        public static HttpContextBase DynamicHttpContextBase(this MockRepository mocks, IPrincipal principal,
                                                             HttpBrowserCapabilitiesBase browser)
        {
            return mocks.DynamicHttpContextBase
                (browser,
                 mocks.DynamicHttpRequestBase(),
                 mocks.DynamicHttpResponseBase(),
                 mocks.DynamicHttpSessionStateBase(),
                 mocks.DynamicHttpServerUtilityBase(),
                 principal);
        }

        public static HttpContextBase DynamicHttpContextBase(this MockRepository mocks,
                                                             HttpBrowserCapabilitiesBase browser,
                                                             HttpRequestBase request,
                                                             HttpResponseBase response,
                                                             HttpSessionStateBase session,
                                                             HttpServerUtilityBase server,
                                                             IPrincipal user)
        {
            var context = mocks.DynamicMock<HttpContextBase>();
            SetupResult.For(context.User).Return(user);
            SetupResult.For(request.Browser).Return(browser);
            SetupResult.For(context.Request).Return(request);
            SetupResult.For(context.Response).Return(response);
            SetupResult.For(context.Session).Return(session);
            SetupResult.For(context.Server).Return(server);
            mocks.Replay(context);
            return context;
        }

        public static HttpBrowserCapabilitiesBase DynamicHttpBrowserCapabilitiesBase(this MockRepository mocks)
        {
            var browser = mocks.DynamicMock<HttpBrowserCapabilitiesBase>();
            return browser;
        }

        public static HttpRequestBase DynamicHttpRequestBase(this MockRepository mocks)
        {
            var request = mocks.DynamicMock<HttpRequestBase>();
            SetupResult.For(request.Form).Return(new NameValueCollection());
            SetupResult.For(request.QueryString).Return(new NameValueCollection());
            return request;
        }

        public static HttpResponseBase DynamicHttpResponseBase(this MockRepository mocks)
        {
            var response = mocks.DynamicMock<HttpResponseBase>();
            SetupResult.For(response.OutputStream).Return(new MemoryStream());
            SetupResult.For(response.Output).Return(new StringWriter());
            SetupResult.For(response.ContentType).PropertyBehavior();
            return response;
        }

        public static HttpSessionStateBase DynamicHttpSessionStateBase(this MockRepository mocks)
        {
            var session = mocks.DynamicMock<HttpSessionStateBase>();
            return session;
        }

        public static HttpServerUtilityBase DynamicHttpServerUtilityBase(this MockRepository mocks)
        {
            var server = mocks.DynamicMock<HttpServerUtilityBase>();
            return server;
        }

        public static IPrincipal DynamicIPrincipal(this MockRepository mocks)
        {
            var principal = mocks.DynamicMock<IPrincipal>();
            return principal;
        }
    }
}