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

namespace Castle.MonoRail.Views.Spark.Tests
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;

    using Castle.MonoRail.Framework.Helpers;

    using NUnit.Framework;

    [TestFixture]
    public class HelperExtensionMethodsTests
    {
        [Test]
        public void VerifyAppropriateMethodsPresent()
        {
            var propInfos = typeof(SparkView).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.DeclaredOnly);

            var helperDescriptors = new List<HelperDescriptor>();
            foreach (var propInfo in propInfos.Where(info => typeof(AbstractHelper).IsAssignableFrom(info.PropertyType)))
            {
                helperDescriptors.Add(new HelperDescriptor(propInfo.PropertyType));
            }

            var neededMethods = new List<MethodDescriptor>();
            foreach (var helper in helperDescriptors)
            {
                foreach (var method in helper.MethodDescriptors)
                {
                    neededMethods.Add(method);
                }
            }

            var methodInfos = typeof(HelperExtensions).GetMethods(BindingFlags.Public | BindingFlags.Static);

            var existingMethods = new List<MethodDescriptor>();
            foreach (var methodInfo in methodInfos.Where(MethodDescriptor.IsHelperExtension))
            {
                existingMethods.Add(new MethodDescriptor(methodInfo));
            }

            var missingMethods = new List<MethodDescriptor>();
            foreach (var neededMethod in neededMethods)
            {
                bool located = false;
                foreach (var existingMethod in existingMethods)
                {
                    if (MethodDescriptor.IsExtensionForMethod(existingMethod, neededMethod))
                    {
                        located = true;
                        break;
                    }
                }
                if (!located)
                    missingMethods.Add(neededMethod);
            }

            Assert.IsEmpty(string.Concat(missingMethods.ToArray()), "{0} methods not represented", missingMethods.Count);
        }

        class HelperDescriptor
        {
            private readonly Type _helperType;
            private readonly IList<MethodDescriptor> _methodDescriptors;

            public HelperDescriptor(Type helperType)
            {
                _helperType = helperType;
                _methodDescriptors = new List<MethodDescriptor>();

                var methodInfos =
                    _helperType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

                foreach (var methodInfo in methodInfos.Where(MethodDescriptor.IsSuitableHelperMethod))
                {
                    MethodDescriptors.Add(new MethodDescriptor(methodInfo));
                }
            }

            public IList<MethodDescriptor> MethodDescriptors
            {
                get { return _methodDescriptors; }
            }
        }

        class MethodDescriptor
        {
            private readonly MethodInfo info;

            public MethodDescriptor(MethodInfo info)
            {
                this.info = info;
            }

            public static bool IsSuitableHelperMethod(MethodInfo info)
            {
                bool hasDictionary = false;
                foreach (var parameter in info.GetParameters())
                {
                    if (parameter.ParameterType == typeof(IDictionary))
                        hasDictionary = true;
                }
                if (!hasDictionary)
                    return false;

                if (info.ReturnType != typeof(string))
                    return false;

                return true;
            }

            public static bool IsHelperExtension(MethodInfo info)
            {
                var parameters = info.GetParameters();
                if (parameters == null || parameters.Length == 0)
                    return false;

                var firstParam = parameters[0];
                if (!typeof(AbstractHelper).IsAssignableFrom(firstParam.ParameterType))
                    return false;

                return true;
            }

            public static bool IsExtensionForMethod(MethodDescriptor extension, MethodDescriptor method)
            {
                // methods must be same name
                if (extension.info.Name != method.info.Name)
                    return false;

                // first argument must be for target type
                var extensionParameter = extension.info.GetParameters()[0];
                if (extensionParameter.ParameterType != method.info.DeclaringType)
                    return false;

                // remainging argument count must match
                int parameterCount = method.info.GetParameters().Length;
                if (extension.info.GetParameters().Length != parameterCount + 1)
                    return false;

                return true;
            }

            public override string ToString()
            {
                //public static string FormTag(this FormHelper helper, string url, object parameters)
                //{ return helper.FormTag(url, new ModelDictionary(parameters)); }

                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("public static string {0} (this {1} helper", info.Name, info.DeclaringType.Name);
                foreach (var parameter in info.GetParameters())
                {
                    var type = parameter.ParameterType;
                    if (type == typeof(IDictionary))
                        type = typeof(object);

                    sb.AppendFormat(", {0} {1}", type.FullName, parameter.Name);
                }
                sb.AppendLine(")");
                sb.AppendLine("{");
                sb.AppendFormat("\treturn helper.{0}(", info.Name);
                string delimiter = "";
                foreach (var parameter in info.GetParameters())
                {
                    var type = parameter.ParameterType;
                    if (type == typeof(IDictionary))
                    {
                        sb.AppendFormat("{0}new ModelDictionary({1})", delimiter, parameter.Name);
                    }
                    else
                    {
                        sb.AppendFormat("{0}{1}", delimiter, parameter.Name);
                    }
                    delimiter = ", ";
                }
                sb.AppendLine(");");
                sb.AppendLine("}");
                return sb.ToString();
            }
        }
    }
}