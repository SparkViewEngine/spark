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
using System.Linq;
using Castle.MonoRail.Framework;

namespace Castle.MonoRail.Views.Spark
{
    public class ViewComponentInfo
    {
        public ViewComponentInfo()
        {

        }
        public ViewComponentInfo(ViewComponent component)
        {
            Type = component.GetType();
            Details = Type.GetCustomAttributes(typeof(ViewComponentDetailsAttribute), false).OfType<ViewComponentDetailsAttribute>().FirstOrDefault();
            Instance = component;
        }

        public Type Type { get; set; }
        public ViewComponentDetailsAttribute Details { get; set; }
        public ViewComponent Instance { get; set; }

        public bool SupportsSection(string sectionName)
        {
            if (Details != null)
                return Details.SupportsSection(sectionName);
            
            if (Instance != null)
            {
                // if a component doesn't provide an implementation the default may throw an exception
                try { return Instance.SupportsSection(sectionName); }
                catch (NullReferenceException) { }
            }

            return false;
        }
    }
}