using System;
using CommandLine;

namespace ALDataIntegrator
{
    public class CommandLineOptions
    {
        [Option('v', "verbose", Required = false, DefaultValue = false, HelpText = "This option sets if application events should be shown in the console")]
        public  bool Verbose { get; set; }

        [Option('t', "type", Required = false, HelpText = "This option sets the type of files that should be processed where:\r\n\t0: All Files\r\n\t1: Organizations\r\n\t2: Service Tickets\r\n\t3: Images\r\n\t4: Service Ticket Export Requests\r\n\t5: Purge Test Images")]
        public int ProcessType { get; set; }

        [Option('r', "reportid", Required = false, HelpText = "This option sets the Report ID(s) that should be processed where Report ID is one or more OSvC Report IDs separated by a comma.")]
        public string ReportIDs { get; set; }

        public  override string ToString()
        {
            const string line = "---------------------------------------------------------";

            var result = string.Join(Environment.NewLine,
                line,
                "Passed Command Line Options",
                line,
                string.Format("Verbose Output: -v\t{0}", Verbose),
                string.Format("Process Type: -t\t{0}", ProcessType),
                string.Format("Report ID: -r\t{0}", ReportIDs),
                line,
                "Available Command Line Options",
                line,
                "Verbose Output: -v or --verbose",
                "Report ID(s): -r or --reportid",
                //"Process Type: -t or --type",
                //"\t0 - All Files",
                //"\t1 - Reports Export",
                line);
            return result;
        }

        public string GetUsage()
        {
            return "this screen tells you how to get help";
        }
    }
}