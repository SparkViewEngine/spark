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
            _path = path;


            //Spark.Web.Mvc.SparkView
            //MyBaseView

            var settings = new SparkSettings()
                .SetPageBaseType("Spark.Web.Mvc.SparkView");
            _engine = new SparkViewEngine(settings);
            var host = (IVsContainedLanguageHost)source;
            host.GetVSHierarchy(out _hierarchy);
            _engine.ViewFolder = new VsProjectViewFolder(_source, _hierarchy);
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
            var result = _grammar.Nodes(new Position(new SourceContext(primaryText)));

            var descriptor = new SparkViewDescriptor()
                .AddTemplate(_path);

            var entry = _engine.CreateEntry(_engine.CreateKey(descriptor), false);
            _generatedCode = entry.SourceCode;


            var mappings = entry.SourceMappings.Select(
                m => new _SOURCEMAPPING
                         {
                             start1 = m.Source.Begin.Offset,
                             end1 = m.Source.End.Offset,
                             start2 = m.OutputBegin,
                             end2 = m.OutputEnd
                         }).ToArray();

            var paints = result.Rest.GetPaint().OfType<Paint<SparkTokenType>>().Select(
                p => new _SOURCEPAINTING
                    {
                        start = p.Begin.Offset,
                        end = p.End.Offset,
                        color = (_SPARKPAINT)p.Value
                    }).ToArray();

            foreach (var events in _events.Values)
            {
                events.OnGenerated(
                    primaryText, 
                    entry.SourceCode, 
                    mappings.Length, 
                    ref mappings[0],
                    paints.Length,
                    ref paints[0]);
            }
        }
    }
}
