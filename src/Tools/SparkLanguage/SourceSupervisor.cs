using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Spark.Compiler;
using Spark.Parser.Markup;
using SparkLanguage.VsAdapters;
using SparkLanguagePackageLib;
using Spark.Parser;
using Spark;

namespace SparkLanguage
{
    public class SourceSupervisor : ISourceSupervisor
    {
        readonly SparkViewEngine _engine;
        readonly MarkupGrammar _grammar;
        readonly ISparkSource _source;
        readonly string _path;

        uint _dwLastCookie;
        readonly IDictionary<uint, ISourceSupervisorEvents> _events = new Dictionary<uint, ISourceSupervisorEvents>();

        static SourceSupervisor()
        {
            // To enable Visual Studio to correlate errors, the location of the 
            // error must be allowed to come from the natural location 
            // in the generated file. This setting is changed for the entire 
            // AppDomain running inside the devenv process.

            SourceBuilder.AdjustDebugSymbolsDefault = false;
        }

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

            var settings = new VsProjectSparkSettings(hierarchy)
                               {
                                   PageBaseType = source.GetDefaultPageBaseType()
                               };

            var viewFolder = new VsProjectViewFolder(_source, hierarchy);

            _engine = new SparkViewEngine(settings)
                          {
                              ViewFolder = viewFolder
                          };

            _grammar = new MarkupGrammar(settings);
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

            if (Equals(docItem.Parent, rootItem))
                path = "$\\" + path;

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

        class PaintInfo
        {
            public int Count { get; set; }
            public _SOURCEPAINTING[] Paint { get; set; }
            public Exception ParseError { get; set; }
        }

        class MappingInfo
        {
            public string GeneratedCode { get; set; }
            public int Count { get; set; }
            public _SOURCEMAPPING[] Mapping { get; set; }
            public Exception GenerationError { get; set; }
        }

        public void PrimaryTextChanged(int processImmediately)
        {
            var primaryText = _source.GetPrimaryText();

            var paintInfo = GetPaintInfo(primaryText);

            var mappingInfo = GetMappingInfo();


            foreach (var events in _events.Values)
            {
                events.OnGenerated(
                    primaryText,
                    mappingInfo.GeneratedCode,
                    mappingInfo.Count,
                    ref mappingInfo.Mapping[0],
                    paintInfo.Count,
                    ref paintInfo.Paint[0]);
            }
        }

        private PaintInfo GetPaintInfo(string primaryText)
        {
            var paintInfo = new PaintInfo();
            try
            {
                var sourceContext = new SourceContext(primaryText, 0, _path);
                var result = _grammar.Nodes(new Position(sourceContext));

                paintInfo.Paint = result.Rest.GetPaint()
                    .OfType<Paint<SparkTokenType>>()
                    .Where(p => string.Equals(p.Begin.SourceContext.FileName, _path,
                                              StringComparison.InvariantCultureIgnoreCase))
                    .Select(p => new _SOURCEPAINTING
                                     {
                                         start = p.Begin.Offset,
                                         end = p.End.Offset,
                                         color = (int)p.Value
                                     })
                    .ToArray();

                paintInfo.Count = paintInfo.Paint.Length;
            }
            catch (Exception ex)
            {
                paintInfo.ParseError = ex;
            }

            if (paintInfo.Count == 0)
                paintInfo.Paint = new _SOURCEPAINTING[1];
            return paintInfo;
        }

        private MappingInfo GetMappingInfo()
        {
            var mappingInfo = new MappingInfo();
            try
            {
                var descriptor = new SparkViewDescriptor()
                    .AddTemplate(_path);

                var entry = _engine.CreateEntryInternal(descriptor, false);

                mappingInfo.GeneratedCode = entry.SourceCode;

                mappingInfo.Mapping = entry.SourceMappings
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

                mappingInfo.Count = mappingInfo.Mapping.Length;
            }
            catch (Exception ex)
            {
                mappingInfo.GenerationError = ex;
            }

            if (mappingInfo.Count == 0)
                mappingInfo.Mapping = new _SOURCEMAPPING[1];
            return mappingInfo;
        }

        public void OnTypeChar(IVsTextView pView, string ch)
        {
            if (ch == "{")
            {
                // add a closing "}" if it makes a complete expression or condition where one doesn't exist otherwise

                _TextSpan selection;
                pView.GetSelectionSpan(out selection);

                IVsTextLines buffer;
                pView.GetBuffer(out buffer);

                string before;
                buffer.GetLineText(0, 0, selection.iStartLine, selection.iStartIndex, out before);

                int iLastLine, iLastColumn;
                buffer.GetLastLineIndex(out iLastLine, out iLastColumn);

                string after;
                buffer.GetLineText(selection.iEndLine, selection.iEndIndex, iLastLine, iLastColumn, out after);

                var existingResult = _grammar.Nodes(new Position(new SourceContext(before + ch + after)));
                var expressionHits = existingResult.Rest.GetPaint()
                    .OfType<Paint<Node>>()
                    .Where(p => p.Begin.Offset <= before.Length && before.Length <= p.End.Offset && (p.Value is ExpressionNode || p.Value is ConditionNode));

                // if a node exists normally, do nothing
                if (expressionHits.Count() != 0)
                    return;

                var withCloseResult = _grammar.Nodes(new Position(new SourceContext(before + ch + "}" + after)));
                var withCloseHits = withCloseResult.Rest.GetPaint()
                    .OfType<Paint<Node>>()
                    .Where(p => p.Begin.Offset <= before.Length && before.Length <= p.End.Offset && (p.Value is ExpressionNode || p.Value is ConditionNode));

                // if a close brace doesn't cause a node to exist, do nothing
                if (withCloseHits.Count() == 0)
                    return;

                // add a closing } after the selection, then set the selection back to what it was
                int iAnchorLine, iAnchorCol, iEndLine, iEndCol;
                pView.GetSelection(out iAnchorLine, out iAnchorCol, out iEndLine, out iEndCol);
                _TextSpan inserted;
                buffer.ReplaceLines(selection.iEndLine, selection.iEndIndex, selection.iEndLine, selection.iEndIndex, "}", 1, out inserted);
                pView.SetSelection(iAnchorLine, iAnchorCol, iEndLine, iEndCol);
            }
        }

    }
}
