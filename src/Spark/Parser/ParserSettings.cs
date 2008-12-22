namespace Spark.Parser
{
    public interface IParserSettings
    {
        bool AutomaticEncoding { get; }
    }

    public class ParserSettings : IParserSettings
    {
        /// <summary>
        /// For now the default is to have ${expr} and !{expr} both output raw html.
        /// This could change very soon, so developers are encouraged to provide an explicit setting.
        /// </summary>
        public const bool DefaultAutomaticEncoding = false;

        public bool AutomaticEncoding { get; set; }

        static public IParserSettings LegacyBehavior
        {
            get { return new ParserSettings { AutomaticEncoding = false }; }
        }

        static public IParserSettings DefaultBehavior
        {
            get { return new ParserSettings { AutomaticEncoding = DefaultAutomaticEncoding }; }
        }
    }
}
