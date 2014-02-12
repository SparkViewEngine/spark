// Copyright 2008-2009 Louis DeJardin - http://whereslou.com
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
using System.Collections;
using System.Globalization;
using System.Security.Permissions;
using System.Security.Principal;
using System.Web;
using System.Web.Caching;
using System.Web.Instrumentation;
using System.Web.Profile;

namespace Spark.Web.Mvc.Wrappers
{
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal),
     AspNetHostingPermission(SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    public class HttpContextWrapper : HttpContextBase
    {
        // Fields
        private readonly HttpContextBase _context;
        private readonly ITextWriterContainer _textWriterContainer;

        // Methods
        public HttpContextWrapper(HttpContextBase httpContext, ITextWriterContainer textWriterContainer)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException("httpContext");
            }
            if (textWriterContainer == null)
            {
                throw new ArgumentNullException("textWriterContainer");
            }
            _context = httpContext;
            _textWriterContainer = textWriterContainer;
        }

        // Properties
        public override Exception[] AllErrors
        {
            get { return _context.AllErrors; }
        }

        public override HttpApplicationStateBase Application
        {
            get { return _context.Application; }
        }

        public override HttpApplication ApplicationInstance
        {
            get { return _context.ApplicationInstance; }
            set { _context.ApplicationInstance = value; }
        }

        public override Cache Cache
        {
            get { return _context.Cache; }
        }

        public override IHttpHandler CurrentHandler
        {
            get { return _context.CurrentHandler; }
        }

        public override RequestNotification CurrentNotification
        {
            get { return _context.CurrentNotification; }
        }

        public override Exception Error
        {
            get { return _context.Error; }
        }

        public override IHttpHandler Handler
        {
            get { return _context.Handler; }
            set { _context.Handler = value; }
        }

        public override bool IsCustomErrorEnabled
        {
            get { return _context.IsCustomErrorEnabled; }
        }

        public override bool IsDebuggingEnabled
        {
            get { return _context.IsDebuggingEnabled; }
        }

        public override bool IsPostNotification
        {
            get { return _context.IsDebuggingEnabled; }
        }

        public override IDictionary Items
        {
            get { return _context.Items; }
        }

        public override IHttpHandler PreviousHandler
        {
            get { return _context.PreviousHandler; }
        }

        public override ProfileBase Profile
        {
            get { return _context.Profile; }
        }

        public override HttpRequestBase Request
        {
            get { return _context.Request; }
        }

        public override HttpResponseBase Response
        {
            get { return new HttpResponseWrapper(_context.Response, _textWriterContainer); }
        }

        public override HttpServerUtilityBase Server
        {
            get { return _context.Server; }
        }

        public override HttpSessionStateBase Session
        {
            get { return _context.Session; }
        }

        public override bool SkipAuthorization
        {
            get { return _context.SkipAuthorization; }
            set { _context.SkipAuthorization = value; }
        }

        public override DateTime Timestamp
        {
            get { return _context.Timestamp; }
        }

        public override TraceContext Trace
        {
            get { return _context.Trace; }
        }

        public override IPrincipal User
        {
            get { return _context.User; }
            set { _context.User = value; }
        }

        public override PageInstrumentationService PageInstrumentation
        {
            get { return _context.PageInstrumentation; }
        }

        public override void AddError(Exception errorInfo)
        {
            _context.AddError(errorInfo);
        }

        public override void ClearError()
        {
            _context.ClearError();
        }

        public override object GetGlobalResourceObject(string classKey, string resourceKey)
        {
            return _context.GetGlobalResourceObject(classKey, resourceKey);
        }

        public override object GetGlobalResourceObject(string classKey, string resourceKey, CultureInfo culture)
        {
            return _context.GetGlobalResourceObject(classKey, resourceKey, culture);
        }

        public override object GetLocalResourceObject(string virtualPath, string resourceKey)
        {
            return _context.GetLocalResourceObject(virtualPath, resourceKey);
        }

        public override object GetLocalResourceObject(string virtualPath, string resourceKey, CultureInfo culture)
        {
            return _context.GetLocalResourceObject(virtualPath, resourceKey, culture);
        }

        public override object GetSection(string sectionName)
        {
            return _context.GetSection(sectionName);
        }

        public override object GetService(Type serviceType)
        {
            return ((IServiceProvider) _context).GetService(serviceType);
        }

        public override void RewritePath(string path)
        {
            _context.RewritePath(path);
        }

        public override void RewritePath(string path, bool rebaseClientPath)
        {
            _context.RewritePath(path, rebaseClientPath);
        }

        public override void RewritePath(string filePath, string pathInfo, string queryString)
        {
            _context.RewritePath(filePath, pathInfo, queryString);
        }

        public override void RewritePath(string filePath, string pathInfo, string queryString, bool setClientFilePath)
        {
            _context.RewritePath(filePath, pathInfo, queryString, setClientFilePath);
        }
    }
}