using NUnit.Framework;
using Rhino.Mocks;

namespace Spark.Tests
{
    [TestFixture]
    public class PartialProviderTester
    {
        private SparkViewEngine _engine;
        private IPartialProvider _partialProvider;
        private string _viewPath;
        private string[] _result;

        [SetUp]
        public void Init()
        {
            _viewPath = "fake/path";
            _partialProvider = MockRepository.GenerateMock<IPartialProvider>();
            _engine = new SparkViewEngine(new SparkSettings())
            {
                PartialProvider = _partialProvider
            };
            _result = new[] {"output"};
        }

        [Test]
        public void DefaultPartialReferenceProviderWrapsPartialProvider()
        {
            _partialProvider.Expect(x => x.GetPaths(_viewPath)).Return(_result);
            var output = _engine.PartialReferenceProvider.GetPaths(_viewPath, true);
            _partialProvider.VerifyAllExpectations();
            Assert.AreEqual(_result, output);
        }

        [Test]
        public void SettingNewPartialProviderPropogatesToDefaultPartialProvider()
        {
            var differentPartialProvider = MockRepository.GenerateMock<IPartialProvider>();
            _engine.PartialProvider = differentPartialProvider;

            //should not call the original
            _partialProvider.Expect(x => x.GetPaths(_viewPath)).Repeat.Never();

            //should call the newly provided instance
            differentPartialProvider.Expect(x => x.GetPaths(_viewPath)).Return(_result);

            var output = _engine.PartialReferenceProvider.GetPaths(_viewPath, true);
            _partialProvider.VerifyAllExpectations();
            differentPartialProvider.VerifyAllExpectations();
            Assert.AreEqual(_result, output);
        }
    }
}