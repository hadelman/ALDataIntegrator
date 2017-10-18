using System.Collections.Generic;

namespace ALDataIntegrator.Utility
{
    public class DelimitedFileContent
    {
        internal List<string[]> ReadableData { get; set; }
        internal byte[] RawData { get; set; }
    }
}
