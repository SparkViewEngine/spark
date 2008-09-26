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
using Castle.Core;
using Castle.MicroKernel;
using Spark;

namespace WindsorInversionOfControl
{
    public class ViewActivator : IViewActivatorFactory, IViewActivator
    {
        private readonly IKernel kernel;

        public ViewActivator(IKernel kernel)
        {
            this.kernel = kernel;
        }

        #region IViewActivator Members

        public ISparkView Activate(Type type)
        {
            return kernel.Resolve<ISparkView>(type.FullName);
        }

        public void Release(Type type, ISparkView view)
        {
            kernel.ReleaseComponent(view);
        }

        #endregion

        #region IViewActivatorFactory Members

        public IViewActivator Register(Type type)
        {
            kernel.AddComponent(type.FullName, typeof (ISparkView), type, LifestyleType.Transient);
            return this;
        }

        public void Unregister(Type type, IViewActivator activator)
        {
            kernel.RemoveComponent(type.FullName);
        }

        #endregion
    }
}