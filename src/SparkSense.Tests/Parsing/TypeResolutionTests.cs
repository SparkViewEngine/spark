using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Rhino.Mocks;
using Fasterflect;
using SparkSense.Parsing;

namespace SparkSense.Tests.Parsing
{
    [TestFixture]
    public class TypeResolutionTests
    {
        [Test]
        public void Should()
        {
            //When starting a new request for type resolution
            //And no input is given on where to start

            //A list of all referenced assembly types should be returned

            var typeDiscoveryService = MockRepository.GenerateStub<ITypeDiscoveryService>();

            ICollection referencedTypes = new List<Type> {typeof (SomeType)};
            typeDiscoveryService.Stub(x => x.GetTypes(typeof(object), true)).Return(referencedTypes);

            TypeResolver typeResolver = new TypeResolver(typeDiscoveryService);

            IList<Type> types = typeResolver.Resolve();


            //var mems = someType.GetType().Members(MemberTypes.Property);

            Assert.AreEqual(1, types.Count);


        }

        public class SomeType
        {
        }
    }
}