namespace TraceIdentifiers.Abstractions
{
    public class TraceIdentifiersSendOptions
    {
        public static TraceIdentifiersSendOptions Default { get; } = new TraceIdentifiersSendOptions();

        public const string DefaultHeaderName = "X-TraceIdentifier";

        public const char DefaultSeparator = '|';

        public const int DefaultIdentifiersMaxCount = 10;

        public const int DefaultIdentifierMaxLength = 32;

        public bool AllwoMultipleHeaders { get; set; }

        public string HeaderHame { get; set; } = DefaultHeaderName;

        public int IdentifiersMaxCount { get; set; } = DefaultIdentifiersMaxCount;

        public int IdentifierMaxLength { get; set; } = DefaultIdentifierMaxLength;

        public string SingleHeaderSeparator { get; set; } = new string(DefaultSeparator, 1);
    }
}