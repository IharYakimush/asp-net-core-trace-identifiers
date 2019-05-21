namespace TraceIdentifiers
{
    public class SendIdentifiersOptions
    {
        public static readonly SendIdentifiersOptions Default = new SendIdentifiersOptions();

        public string HeaderName { get; set; } = TraceIdentifiersDefaults.DefaultSharedHeaderName;
        public string Separator { get; set; } = TraceIdentifiersDefaults.DefaultSeparator.ToString();
        public bool UseSeparator { get; set; } = false;
    }
}