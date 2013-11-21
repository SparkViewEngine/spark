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
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Hosting;
using Spark.Bindings;
using Spark.FileSystem;
using Spark.Parser.Syntax;

namespace Spark
{
    public class SparkServiceContainer : ISparkServiceContainer
    {
        public SparkServiceContainer()
        {

        }

        public SparkServiceContainer(ISparkSettings settings)
        {
            _services[typeof(ISparkSettings)] = settings;
        }


        readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

        readonly Dictionary<Type, Func<ISparkServiceContainer, object>> _defaults =
            new Dictionary<Type, Func<ISparkServiceContainer, object>>
                {
                    {typeof (ISparkSettings), c => ConfigurationManager.GetSection("spark") ?? new SparkSettings()},
                    {typeof (ISparkViewEngine), c => new SparkViewEngine(c.GetService<ISparkSettings>())},
                    {typeof (ISparkLanguageFactory), c => new DefaultLanguageFactory()},
                    {typeof (ISparkSyntaxProvider), c => new DefaultSyntaxProvider(c.GetService<ISparkSettings>())},
                    {typeof (IViewActivatorFactory), c => new DefaultViewActivator()},
                    {typeof (IResourcePathManager), c => new DefaultResourcePathManager(c.GetService<ISparkSettings>())},
                    {typeof (ITemplateLocator), c => new DefaultTemplateLocator()},
                    {typeof (IBindingProvider), c => new DefaultBindingProvider()},
                    {typeof (IViewFolder), CreateDefaultViewFolder},
                    {typeof (ICompiledViewHolder), c => new CompiledViewHolder()},
                    {typeof (IPartialProvider), c => new DefaultPartialProvider()},
                    {typeof (IPartialReferenceProvider), c => new DefaultPartialReferenceProvider(c.GetService<IPartialProvider>())},
                };


        public T GetService<T>()
        {
            return (T)GetService(typeof(T));
        }

        public object GetService(Type serviceType)
        {
            lock (_services)
            {
                object service;
                if (_services.TryGetValue(serviceType, out service))
                    return service;

                Func<ISparkServiceContainer, object> serviceBuilder;
                if (_defaults.TryGetValue(serviceType, out serviceBuilder))
                {
                    service = serviceBuilder(this);
                    _services.Add(serviceType, service);
                    if (service is ISparkServiceInitialize)
                        ((ISparkServiceInitialize)service).Initialize(this);
                    return service;
                }
                return null;
            }
        }


        public void SetService<TService>(TService service)
        {
            SetService(typeof(TService), service);
        }

        public void SetService(Type serviceType, object service)
        {
            if (_services.ContainsKey(serviceType))
                throw new ApplicationException(string.Format("A service of type {0} has already been created", serviceType));
            if (!serviceType.IsInterface)
                throw new ApplicationException(string.Format("Only an interface may be used as service type. {0}", serviceType));

            lock (_services)
            {
                _services[serviceType] = service;
                if (service is ISparkServiceInitialize)
                    ((ISparkServiceInitialize)service).Initialize(this);
            }
        }

        public void SetServiceBuilder<TService>(Func<ISparkServiceContainer, object> serviceBuilder)
        {
            SetServiceBuilder(typeof(TService), serviceBuilder);
        }

        public void SetServiceBuilder(Type serviceType, Func<ISparkServiceContainer, object> serviceBuilder)
        {
            if (_services.ContainsKey(serviceType))
                throw new ApplicationException(string.Format("A service of type {0} has already been created", serviceType));
            if (!serviceType.IsInterface)
                throw new ApplicationException(string.Format("Only an interface may be used as service type. {0}", serviceType));

            lock (_services)
            {
                _defaults[serviceType] = serviceBuilder;
            }
        }

        private static object CreateDefaultViewFolder(ISparkServiceContainer arg)
        {
            if (HostingEnvironment.IsHosted && HostingEnvironment.VirtualPathProvider != null)
                return new VirtualPathProviderViewFolder("~/Views");
            var appBase = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            return new FileSystemViewFolder(Path.Combine(appBase, "Views"));
        }
    }
}
