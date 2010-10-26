using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;

namespace SparkSense.Parsing
{
    public class TypeResolver
    {
        private readonly ITypeDiscoveryService _typeDiscoveryService;

        public TypeResolver(ITypeDiscoveryService typeDiscoveryService)
        {
            _typeDiscoveryService = typeDiscoveryService;
        }

        public IList<Type> Resolve()
        {
            return _typeDiscoveryService.GetTypes(typeof (object), true).Cast<Type>().ToList();
        }
    }
}
