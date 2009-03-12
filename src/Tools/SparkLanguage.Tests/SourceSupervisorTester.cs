using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.Mocks.Interfaces;
using Rhino.Mocks.MethodRecorders;
using SparkLanguage.VsAdapters;
using SparkLanguagePackageLib;
using IServiceProvider = SparkLanguagePackageLib.IServiceProvider;

namespace SparkLanguage.Tests
{
    [TestFixture]
    public class SourceSupervisorTester
    {
        private MockRepository _repos;

        [SetUp]
        public void Init()
        {
            _repos = new MockRepository();
        }

        [Test]
        public void PrimaryTextChangedCausesEventToFire()
        {
            var hier = _repos.PartialMock<StubHierarchy>();
            var buffer = _repos.StrictMock<IVsTextLines>();

            var source = _repos.PartialMock<StubSource>(hier, (uint)3, buffer);
            var events = _repos.StrictMock<ISourceSupervisorEvents>();

            var primaryText = "Hello World";

            const string name = @"c:\hello.txt";

            string outname;
            source.Expect(x => x.GetDefaultPageBaseType())
                .Return("FakeBaseType");
            source.Expect(x => x.GetPrimaryText())
                .Return(primaryText);
            source.Expect(x => x.GetRunningDocumentText(name))
                .Return(primaryText).Repeat.AtLeastOnce();
            hier.Expect(x => x.GetCanonicalName(3, out outname))
                .OutRef(name).Repeat.AtLeastOnce();

            var paints = new _SOURCEPAINTING();
            var spans = new _SOURCEMAPPING();
            events.Expect(x => x.OnGenerated(null, null, 0, ref spans, 0, ref paints))
                .IgnoreArguments();

            _repos.ReplayAll();

            hier.Add(0xfffffffe, new Item { { __VSHPROPID.VSHPROPID_FirstChild, 1 } });
            hier.Add(1, new Item
                            {
                                {__VSHPROPID.VSHPROPID_Parent, -2},
                                {__VSHPROPID.VSHPROPID_FirstChild, 2},
                                {__VSHPROPID.VSHPROPID_NextSibling, -1},
                                {__VSHPROPID.VSHPROPID_Name, "Views"},
                            });
            hier.Add(2, new Item
                            {
                                {__VSHPROPID.VSHPROPID_Parent, 1},
                                {__VSHPROPID.VSHPROPID_FirstChild, 3},
                                {__VSHPROPID.VSHPROPID_NextSibling, -1},
                                {__VSHPROPID.VSHPROPID_Name, "Home"},
                            });
            hier.Add(3, new Item
                             {
                                 {__VSHPROPID.VSHPROPID_Parent, 2},
                                 {__VSHPROPID.VSHPROPID_FirstChild, -1},
                                 {__VSHPROPID.VSHPROPID_NextSibling, -1},
                                 {__VSHPROPID.VSHPROPID_Name, "Index.spark"},
                             });

            uint dwCookie;
            var sourceSupervisor = new SourceSupervisor(source);
            sourceSupervisor.Advise(events, out dwCookie);
            sourceSupervisor.PrimaryTextChanged(1);

            _repos.VerifyAll();
        }

        private _SOURCEMAPPING[] GenerateMaps(string text)
        {
            var hier = _repos.PartialMock<StubHierarchy>();
            var buffer = _repos.StrictMock<IVsTextLines>();

            var source = _repos.PartialMock<StubSource>(hier, (uint)3, buffer);
            var events = _repos.PartialMock<StubSourceSupervisorEvents>();

            const string name = @"c:\hello.txt";

            string outname;
            source.Expect(x => x.GetDefaultPageBaseType())
                .Return("FakeBaseType");
            source.Expect(x => x.GetPrimaryText())
                .Return(text);
            source.Expect(x => x.GetRunningDocumentText(name))
                .Return(text).Repeat.AtLeastOnce();
            hier.Expect(x => x.GetCanonicalName(3, out outname))
                .OutRef(name).Repeat.AtLeastOnce();

            //var paints = new _SOURCEPAINTING();
            //var spans = new _SOURCEMAPPING();
            //events.Expect(x => x.OnGenerated(null, null, 0, ref spans, 0, ref paints))
            //    .IgnoreArguments()
            //    .CallOriginalMethod(OriginalCallOptions.CreateExpectation);

            _repos.ReplayAll();

            hier.Add(0xfffffffe, new Item { { __VSHPROPID.VSHPROPID_FirstChild, 1 } });
            hier.Add(1, new Item
                            {
                                {__VSHPROPID.VSHPROPID_Parent, -2},
                                {__VSHPROPID.VSHPROPID_FirstChild, 2},
                                {__VSHPROPID.VSHPROPID_NextSibling, -1},
                                {__VSHPROPID.VSHPROPID_Name, "Views"},
                            });
            hier.Add(2, new Item
                            {
                                {__VSHPROPID.VSHPROPID_Parent, 1},
                                {__VSHPROPID.VSHPROPID_FirstChild, 3},
                                {__VSHPROPID.VSHPROPID_NextSibling, -1},
                                {__VSHPROPID.VSHPROPID_Name, "Home"},
                            });
            hier.Add(3, new Item
                             {
                                 {__VSHPROPID.VSHPROPID_Parent, 2},
                                 {__VSHPROPID.VSHPROPID_FirstChild, -1},
                                 {__VSHPROPID.VSHPROPID_NextSibling, -1},
                                 {__VSHPROPID.VSHPROPID_Name, "Index.spark"},
                             });

            uint dwCookie;
            var sourceSupervisor = new SourceSupervisor(source);
            sourceSupervisor.Advise(events, out dwCookie);
            sourceSupervisor.PrimaryTextChanged(1);

            _repos.VerifyAll();

            return events.Mapping;
        }

        void AssertMapping(string before, string mapped, string after)
        {
            var code = before + mapped + after;
            var maps = GenerateMaps(code);
            Assert.That(
                maps.Any(x => x.start1 == before.Length && x.end1 == before.Length + mapped.Length),
                "Section {1} must be mapped in {0}\r\nmapping was:{2}",
                code, mapped,
                string.Join(", ", maps.Select(x => "`" + code.Substring(x.start1, x.end1 - x.start1) + "`").ToArray()));
        }

        [Test]
        public void MapExpression()
        {
            var maps = GenerateMaps("a${b}c");
            Assert.AreEqual(1, maps.Length);
            Assert.AreEqual(3, maps[0].start1);
            Assert.AreEqual(4, maps[0].end1);
        }

        [Test]
        public void MapExpressionRaw()
        {
            AssertMapping("a!{", "b", "}c");
            AssertMapping("a!{", "Html.ActionLink(\"foo\")", "}c");
        }

        [Test]
        public void MapConditionAttributes()
        {
            AssertMapping("<if condition='", "x", "'>hello</if>");
            AssertMapping("<test if='", "x", "'>hello</test>");
            AssertMapping("<p if='", "x", "'>hello</p>");
        }
        [Test]
        public void MapElseIfAttributes()
        {
            AssertMapping("<test if='x'>hello<else if='", "y", "'/>world</test>");
            AssertMapping("<if condition='x'>hello</if><else if='", "y", "'>world</else>");
        }

        [Test]
        public void MapVarValueAndType()
        {
            AssertMapping("<var x='", "5", "'/>");
            AssertMapping("<var x='5' type='", "int", "'/>");
            AssertMapping("<var x='", "Convert.ToString(\"43\")", "'/>");
        }

        [Test]
        public void MapEachAttribute()
        {
            AssertMapping("<for each='", "var x in y", "'>hello</for>");
            AssertMapping("<p each='", "var x in y", "'>hello</p>");
        }


        [Test]
        public void MapViewDataModel()
        {
            AssertMapping("<viewdata model='", "string", "'/>");
        }

        [Test]
        public void MapNamespace()
        {
            AssertMapping("<use namespace='", "System", "'/>");
        }

        public class Item : Dictionary<__VSHPROPID, object>
        {
        }

        public abstract class StubHierarchy : Dictionary<uint, Item>, IVsHierarchy
        {
            public virtual void GetProperty(uint itemid, int propid, out object pvar)
            {
                if (!ContainsKey(itemid))
                    Assert.Fail("itemid {0} not present", itemid);
                if (!this[itemid].ContainsKey((__VSHPROPID)propid))
                    Assert.Fail("itemid {0} does not have {1}", itemid, (__VSHPROPID)propid);
                pvar = this[itemid][(__VSHPROPID)propid];
            }

            public abstract void SetSite(IServiceProvider pSP);
            public abstract void GetSite(out IServiceProvider ppSP);
            public abstract void QueryClose(out int pfCanClose);
            public abstract void Close();
            public abstract void GetGuidProperty(uint itemid, int propid, out Guid pguid);
            public abstract void SetGuidProperty(uint itemid, int propid, ref Guid rguid);
            public abstract void SetProperty(uint itemid, int propid, object var);
            public abstract void GetNestedHierarchy(uint itemid, ref Guid iidHierarchyNested, out IntPtr ppHierarchyNested, out uint pitemidNested);
            public abstract void GetCanonicalName(uint itemid, out string pbstrName);
            public abstract void ParseCanonicalName(string pszName, out uint pitemid);
            public abstract void Unused0();
            public abstract void AdviseHierarchyEvents(IVsHierarchyEvents pEventSink, out uint pdwCookie);
            public abstract void UnadviseHierarchyEvents(uint dwCookie);
            public abstract void Unused1();
            public abstract void Unused2();
            public abstract void Unused3();
            public abstract void Unused4();
        }

        public abstract class StubSource : ISparkSource
        {
            private readonly IVsHierarchy _hier;
            private readonly uint _itemid;
            private readonly IVsTextLines _buffer;

            protected StubSource(IVsHierarchy hier, uint itemid, IVsTextLines buffer)
            {
                _hier = hier;
                _itemid = itemid;
                _buffer = buffer;
            }

            public virtual void GetDocument(out IVsHierarchy ppHierarchy, out uint pitemid, out IVsTextLines pBuffer)
            {
                ppHierarchy = _hier;
                pitemid = _itemid;
                pBuffer = _buffer;
            }

            public abstract void SetSupervisor(ISourceSupervisor pSupervisor);
            public abstract ISourceSupervisor GetSupervisor();
            public abstract IVsIntellisenseProjectManager GetIntellisenseProjectManager();
            public abstract IVsContainedLanguage GetContainedLanguage();
            public abstract IVsTextBufferCoordinator GetTextBufferCoordinator();
            public abstract string GetPrimaryText();
            public abstract string GetRunningDocumentText(string CanonicalName);
            public abstract void GetPaint(out int cPaint, IntPtr prgPaint);
            public abstract string GetDefaultPageBaseType();
        }

        public class StubSourceSupervisorEvents : ISourceSupervisorEvents
        {
            public _SOURCEMAPPING[] Mapping { get; set; }

            unsafe public void OnGenerated(string primaryText, string secondaryText, int cMappings, ref _SOURCEMAPPING rgSpans, int cPaints, ref _SOURCEPAINTING rgPaints)
            {
                Mapping = new _SOURCEMAPPING[cMappings];

                fixed (_SOURCEMAPPING* sourcemapping = &rgSpans)
                {
                    for (var index = 0; index != cMappings; ++index)
                        Mapping[index] = sourcemapping[index];
                }
            }
        }
    }
}
