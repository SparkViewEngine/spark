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
namespace Castle.MonoRail.Views.Spark
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Collections;

    public class ModelDictionary : Dictionary<string, object>
    {
        public ModelDictionary(object model)
            : base(StringComparer.InvariantCultureIgnoreCase)
        {

            foreach (var member in model.GetType().GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetField | BindingFlags.GetProperty))
            {
                object value;

                if (member is FieldInfo)
                    value = (member as FieldInfo).GetValue(model);
                else if (member is PropertyInfo)
                    value = (member as PropertyInfo).GetValue(model, null);
                else
                    value = null;

                if (string.Equals(member.Name, "querystring", StringComparison.InvariantCultureIgnoreCase) || 
                    string.Equals(member.Name, "params", StringComparison.InvariantCultureIgnoreCase) ||
                    string.Equals(member.Name, "routeparams", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (!(value is IDictionary) && !(value is string))
                    {
                        value = new ModelDictionary(value);
                    }
                }

                Add(member.Name, value);
            }

        }
    }
}
