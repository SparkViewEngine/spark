using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Rhino.Mocks;
using Spark.Extensions;

namespace Spark
{
    [TestFixture]
    public class PartialProviderTester
    {
        private IPartialProvider _partialProvider;
        private IPartialReferenceProvider _partialReferenceProvider;
        private string _viewPath;
        private string[] _result;

        [SetUp]
        public void Init()
        {
            this._viewPath = "fake/path";

            this._partialProvider = MockRepository.GenerateMock<IPartialProvider>();

            var sp = new ServiceCollection()
                .AddSpark(new SparkSettings())
                .AddSingleton<IPartialProvider>(this._partialProvider)
                .BuildServiceProvider();

            _partialReferenceProvider = sp.GetService<IPartialReferenceProvider>();
            
            this._result = new[] {"output"};
        }

        [Test]
        public void DefaultPartialReferenceProviderWrapsPartialProvider()
        {
            this._partialProvider.Expect(x => x.GetPaths(this._viewPath)).Return(this._result);
            
            var output = _partialReferenceProvider.GetPaths(this._viewPath, true);
            
            this._partialProvider.VerifyAllExpectations();
            
            Assert.AreEqual(this._result, output);
        }
    }
}