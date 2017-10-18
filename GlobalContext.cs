using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ALDataIntegrator.Properties;
using ALDataIntegrator.Service;
using ALDataIntegrator.Utility;

namespace ALDataIntegrator
{
    internal static class GlobalContext
    {
        internal static CommandLineOptions Options { get; set; }

        // logs that are written to the file system
        private static readonly StringBuilder ApplicationEventLog = new StringBuilder();

        // logs that are written to the RightNow IJ$Detail field
        private static readonly StringBuilder RightNowEventLog = new StringBuilder();

        private const string Line = "---------------------------------------------------------";

        internal static void Log(string message, bool logToRightNow)
        {
            if (Options != null && Options.Verbose)
            {
                Console.WriteLine(message);
            }

            ApplicationEventLog.AppendLine(Line);
            var timeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            ApplicationEventLog.AppendLine(string.Format("{0}:",  timeStamp));
            ApplicationEventLog.AppendLine(message);

            if (logToRightNow)
                RightNowEventLog.AppendLine(message);
        }

        internal static void ExitApplication(string exitReason, int exitCode)
        {
            var timeStamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            var filename = string.Format("{0}_{1}", timeStamp, "ALDataIntegrator_log.txt");

            ApplicationEventLog.AppendLine(exitReason);

            if (!Directory.Exists(Settings.Default.LoggingDirectory))
                Directory.CreateDirectory(Settings.Default.LoggingDirectory);

            using (var sw = File.CreateText(Path.Combine(Settings.Default.LoggingDirectory, filename)))
            {
                sw.Write(ApplicationEventLog.ToString());
            }

            Environment.Exit(exitCode);
        }

        internal static string GetEventLog()
        {
            return RightNowEventLog.ToString();
        }

    }
}
