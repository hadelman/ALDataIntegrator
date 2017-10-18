using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ALDataIntegrator.Properties;

namespace ALDataIntegrator.Utility
{
    public static class DelimitedFileReader
    {
        public static DelimitedFileContent ReadFileLines(string path)
        {
            // get the readable data
            var fileData = File.ReadAllLines(path).ToList();

            // get the corresponding byte array from that
            var rawData = new List<byte>();
            foreach (var row in fileData)
            {
                rawData.AddRange(Encoding.UTF8.GetBytes(row));
                rawData.AddRange(Encoding.UTF8.GetBytes(Environment.NewLine));
            }

            var readableData = SplitLines(fileData);

            var result = new DelimitedFileContent()
                             {
                                 RawData = rawData.ToArray(),
                                 ReadableData = readableData
                             };

            return result;
        }

        private static List<string[]> SplitLines(List<string> file)
        {
            char delimiterChar = ',';
            if (!string.IsNullOrEmpty(Settings.Default.Delimiter))
            {
                char[] settingsDelimiter = Settings.Default.Delimiter.ToCharArray();
                if (settingsDelimiter != null && settingsDelimiter.Length > 0)
                    delimiterChar = settingsDelimiter[0];
            }

            var result = file.Select(row => row.Split(new[] { delimiterChar })).ToList();

            // if the file has headers, skip one row
            if (Settings.Default.FileHasHeaders)
                result = result.Skip(1).ToList();

            return result;
        }
               
    }
}
