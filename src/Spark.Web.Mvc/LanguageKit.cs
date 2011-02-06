using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Spark.FileSystem;
using Spark.Web.Mvc.Descriptors;

namespace Spark.Web.Mvc
{
    public static class LanguageKit
    {
        public static void Install(ISparkServiceContainer services, Func<ControllerContext, string> selector)
        {
            services.AddFilter(new Filter(selector));
            services.GetService<ISparkViewEngine>().ViewFolder = new Folder(services.GetService<IViewFolder>());
        }

        public static void Install(SparkViewFactory factory, Func<ControllerContext, string> selector)
        {
            factory.AddFilter(new Filter(selector));
            factory.ViewFolder = new Folder(factory.ViewFolder);
        }

        public static void Install(IEnumerable<IViewEngine> engines, Func<ControllerContext, string> selector)
        {
            foreach (var factory in engines.OfType<SparkViewFactory>())
                Install(factory, selector);
        }

        public class Filter : DescriptorFilterBase
        {
            private readonly Func<ControllerContext, string> _selector;

            public Filter(Func<ControllerContext, string> selector)
            {
                _selector = selector;
            }

            public override void ExtraParameters(ControllerContext context, IDictionary<string, object> extra)
            {
                var theme = Convert.ToString(_selector(context));
                if (!string.IsNullOrEmpty(theme))
                    extra["language"] = theme;
            }

            public override IEnumerable<string> PotentialLocations(IEnumerable<string> locations, IDictionary<string, object> extra)
            {
                string languageName;
                if (!TryGetString(extra, "language", out languageName))
                    return locations;

                var prefix = Path.Combine("language", languageName);
                return locations.Select(path => Path.Combine(prefix, path));
            }
        }

        public class Folder : IViewFolder
        {
            private readonly IViewFolder _viewFolder;

            public Folder(IViewFolder viewFolder)
            {
                _viewFolder = viewFolder;
            }

            public bool HasView(string path)
            {
                var lang = ParseLanguagePath(path);
                if (lang == null)
                    return _viewFolder.HasView(path);

                return PathVariations(lang).Any(x => _viewFolder.HasView(x));
            }

            public IViewFile GetViewSource(string path)
            {
                var lang = ParseLanguagePath(path);
                if (lang == null)
                    return _viewFolder.GetViewSource(path);

                var detected = PathVariations(lang).First(x => _viewFolder.HasView(x));
                return _viewFolder.GetViewSource(detected ?? lang.Path);
            }

            public IList<string> ListViews(string path)
            {
                var lang = ParseLanguagePath(path);
                if (lang == null)
                    return _viewFolder.ListViews(path);

                var languageExtension = "." + lang.Language + ".spark";
                var shortLanguageExtension = lang.ShortLanguage == null ? null : "." + lang.ShortLanguage + ".spark";

                var actualViews = _viewFolder.ListViews(lang.Path);

                var adjustedViews = actualViews.Select(
                    actualPath =>
                    AlterPath(actualPath, languageExtension, lang.Prefix) ??
                    AlterPath(actualPath, shortLanguageExtension, lang.Prefix) ??
                    Path.Combine(lang.Prefix, actualPath)
                    );

                return adjustedViews.Distinct().ToList();
            }

            public string AlterPath(string path, string extension, string prefix)
            {
                if (extension == null ||
                    path.EndsWith(extension, StringComparison.InvariantCultureIgnoreCase) == false)
                {
                    return null;
                }
                return Path.Combine(prefix, path.Substring(0, path.Length - extension.Length) + ".spark");
            }


            public LanguagePath ParseLanguagePath(string path)
            {
                const string language = "language";

                if (path.Length < language.Length + 1 ||
                    path.StartsWith(language, StringComparison.InvariantCultureIgnoreCase) == false ||
                    (path[language.Length] != '/' && path[language.Length] != Path.DirectorySeparatorChar))
                {
                    return null;
                }

                var slashPos = path.IndexOfAny(new[] { '/', Path.DirectorySeparatorChar }, language.Length + 1);
                if (slashPos == -1)
                    return null;

                var lang = new LanguagePath
                               {
                                   Language = path.Substring(language.Length + 1, slashPos - language.Length - 1),
                                   Prefix = path.Substring(0, slashPos),
                                   Path = path.Substring(slashPos + 1)
                               };

                var dashPos = lang.Language.IndexOf('-');
                if (dashPos != -1)
                    lang.ShortLanguage = lang.Language.Substring(0, dashPos);

                return lang;
            }

            public IEnumerable<string> PathVariations(LanguagePath lang)
            {
                if (string.IsNullOrEmpty(lang.ShortLanguage))
                {
                    return new[]
                               {
                                   Path.ChangeExtension(lang.Path, lang.Language + ".spark"),
                                   lang.Path
                               };
                }

                return new[]
                           {
                               Path.ChangeExtension(lang.Path, lang.Language + ".spark"),
                               Path.ChangeExtension(lang.Path, lang.ShortLanguage + ".spark"),
                               lang.Path
                           };
            }
        }

        public class LanguagePath
        {
            public string Language { get; set; }
            public string ShortLanguage { get; set; }
            public string Prefix { get; set; }
            public string Path { get; set; }
        }
    }
}