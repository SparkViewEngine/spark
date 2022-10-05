using NUnit.Framework;
using Rhino.Mocks;

namespace Spark
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
            this._viewPath = "fake/path";
            this._partialProvider = MockRepository.GenerateMock<IPartialProvider>();
            this._engine = new SparkViewEngine(new SparkSettings())
            {
                PartialProvider = this._partialProvider
            };
            this._result = new[] {"output"};
        }

        [Test]
        public void DefaultPartialReferenceProviderWrapsPartialProvider()
        {
            this._partialProvider.Expect(x => x.GetPaths(this._viewPath)).Return(this._result);
            var output = this._engine.PartialReferenceProvider.GetPaths(this._viewPath, true);
            this._partialProvider.VerifyAllExpectations();
            Assert.AreEqual(this._result, output);
        }

        [Test]
        public void SettingNewPartialProviderPropogatesToDefaultPartialProvider()
        {
            var differentPartialProvider = MockRepository.GenerateMock<IPartialProvider>();
            this._engine.PartialProvider = differentPartialProvider;

            //should not call the original
            this._partialProvider.Expect(x => x.GetPaths(this._viewPath)).Repeat.Never();

            //should call the newly provided instance
            differentPartialProvider.Expect(x => x.GetPaths(this._viewPath)).Return(this._result);

            var output = this._engine.PartialReferenceProvider.GetPaths(this._viewPath, true);
            this._partialProvider.VerifyAllExpectations();
            differentPartialProvider.VerifyAllExpectations();
            Assert.AreEqual(this._result, output);
        }
    }
}