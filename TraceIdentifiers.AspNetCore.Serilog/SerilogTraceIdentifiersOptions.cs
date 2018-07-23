namespace TraceIdentifiers.AspNetCore.Serilog
{
    public class SerilogTraceIdentifiersOptions
    {
        internal string TraceIdentifiersCurrentPropertyName { get; set; } = "TraceIdentifier";
        internal string TraceIdentifiersAllPropertyName { get; set; } = "TraceIdentifiersAll";
        internal string TraceIdentifiersSinglePropertyName { get; set; } = "TraceIdentifiers";

        internal bool SingleProperty { get; set; } = false;

        public SerilogTraceIdentifiersOptions AsSingleProperty(string name = null)
        {
            this.TraceIdentifiersSinglePropertyName = name ?? this.TraceIdentifiersSinglePropertyName;
            this.SingleProperty = true;

            return this;
        }

        public SerilogTraceIdentifiersOptions AsSeparateProperties(string currentPropertyName = null, string allPropertyName = null)
        {
            this.TraceIdentifiersCurrentPropertyName = currentPropertyName ?? this.TraceIdentifiersCurrentPropertyName;
            this.TraceIdentifiersAllPropertyName = allPropertyName ?? this.TraceIdentifiersAllPropertyName;
            this.SingleProperty = false;

            return this;
        }
    }
}