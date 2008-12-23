using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Spark.Parser.Markup;
using SparkLanguage.VsAdapters;
using SparkLanguagePackageLib;
using Spark.Parser;
using Spark;

namespace SparkLanguage
{
    public class SourceSupervisor : ISourceSupervisor
    {
        SparkViewEngine _engine;
        ISparkSource _source;
        IVsHierarchy _hierarchy;
        private string _path;
        string _generatedCode;

        private uint _dwLastCookie;
        private IDictionary<uint, ISourceSupervisorEvents> _events = new Dictionary<uint, ISourceSupervisorEvents>();

        public SourceSupervisor(ISparkSource source)
        {
            _source = source;

            IVsHierarchy hierarchy;
            uint itemid;
            IVsTextLines buffer;
            _source.GetDocument(out hierarchy, out itemid, out buffer);

            _path = GetDocumentPath(hierarchy, itemid);


            //Spark.Web.Mvc.SparkView
            //MyBaseView

            var settings = new SparkSettings()
                .SetPageBaseType("Spark.Web.Mvc.SparkView");
            _engine = new SparkViewEngine(settings);
            var host = (IVsContainedLanguageHost)source;
            host.GetVSHierarchy(out _hierarchy);
            _engine.ViewFolder = new VsProjectViewFolder(_source, _hierarchy);
        }

        private static string GetDocumentPath(IVsHierarchy hierarchy, uint itemid)
        {
            var rootid = -2;
            var rootItem = new HierarchyItem(hierarchy, (uint)rootid);
            var viewItem = rootItem.FirstOrDefault(child => child.Name == "Views");

            var docItem = new HierarchyItem(hierarchy, itemid);
            var path = docItem.Name;
            while (!Equals(docItem.Parent, viewItem) &&
                   !Equals(docItem.Parent, rootItem))
            {
                docItem = docItem.Parent;
                path = docItem.Name + "\\" + path;
            }
            return path;
        }

        public void Advise(ISourceSupervisorEvents pEvents, out uint pdwCookie)
        {
            pdwCookie = ++_dwLastCookie;
            _events[_dwLastCookie] = pEvents;
        }

        public void Unadvise(uint dwCookie)
        {
            if (_events.ContainsKey(dwCookie))
                _events.Remove(dwCookie);
        }

        static readonly MarkupGrammar _grammar = new MarkupGrammar();

        public void PrimaryTextChanged(int processImmediately)
        {
            var primaryText = _source.GetPrimaryText();
            var sourceContext = new SourceContext(primaryText, 0, _path);
            var result = _grammar.Nodes(new Position(sourceContext));

            var descriptor = new SparkViewDescriptor()
                .AddTemplate(_path);

            var entry = _engine.CreateEntry(_engine.CreateKey(descriptor), false);
            _generatedCode = entry.SourceCode;


            var mappings = entry.SourceMappings
                .Where(m => string.Equals(m.Source.Begin.SourceContext.FileName, _path,
                                          StringComparison.InvariantCultureIgnoreCase))
                .Select(m => new _SOURCEMAPPING
                                 {
                                     start1 = m.Source.Begin.Offset,
                                     end1 = m.Source.End.Offset,
                                     start2 = m.OutputBegin,
                                     end2 = m.OutputEnd
                                 })
                .ToArray();

            var paints = result.Rest.GetPaint()
                .OfType<Paint<SparkTokenType>>()
                .Where(p => string.Equals(p.Begin.SourceContext.FileName, _path,
                                          StringComparison.InvariantCultureIgnoreCase))
                .Select(p => new _SOURCEPAINTING
                                 {
                                     start = p.Begin.Offset,
                                     end = p.End.Offset,
                                     color = (int) p.Value
                                 })
                .ToArray();

            int cMappings = mappings.Length;
            if (cMappings == 0)
                mappings = new _SOURCEMAPPING[1];

            int cPaints = paints.Length;
            if (cPaints == 0)
                paints = new _SOURCEPAINTING[1];

            foreach (var events in _events.Values)
            {
                events.OnGenerated(
                    primaryText,
                    entry.SourceCode,
                    cMappings,
                    ref mappings[0],
                    cPaints,
                    ref paints[0]);
            }
        }
    }
}
