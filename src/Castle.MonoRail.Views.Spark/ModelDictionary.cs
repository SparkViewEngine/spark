// Copyright 2004-2008 Castle Project - http://www.castleproject.org/
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

namespace Castle.MonoRail.Views.Spark
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public class ModelDictionary : Dictionary<string, object>
    {
        public ModelDictionary(object model)
            : base(StringComparer.InvariantCultureIgnoreCase)
        {
            foreach (var member in model.GetType().GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetField | BindingFlags.GetProperty))
            {
                if (member is FieldInfo)
                    Add(member.Name, (member as FieldInfo).GetValue(model));

                if (member is PropertyInfo)
                    Add(member.Name, (member as PropertyInfo).GetValue(model, null));
            }
        }
    }
}
