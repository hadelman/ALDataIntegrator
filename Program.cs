using System;
using System.IO;
using ALDataIntegrator.Business;
using ALDataIntegrator.Utility;
using ALDataIntegrator.Service.Model;

namespace ALDataIntegrator
{
    class Program
    {
        static void Main(string[] args)
        {
            // get all the default options
            var options = new CommandLineOptions();

            // parse out all of the arguments and exit if there are any bad ones
            if (!CommandLine.Parser.Default.ParseArguments(args, options))
            {
                GlobalContext.Log(string.Format("Arguments: {0}", string.Join(" ", args)), true);
                GlobalContext.ExitApplication(string.Format("Error parsing application arguments.\r\n\r\n{0}", options.ToString()), 1);
            }

            IntegrationJobType intJobType = IntegrationJobType.All;
            if (options.ProcessType == 1)
                intJobType = IntegrationJobType.ReportExport;

            //// set options globally
            GlobalContext.Options = options;

            GlobalContext.Log(options.ToString(), true);

            var controller = new ProcessingController();
            try
            {
                if (intJobType == IntegrationJobType.All || intJobType == IntegrationJobType.ReportExport)
                {
                    String rIDstr = options.ReportIDs;
                    if (string.IsNullOrEmpty(rIDstr))
                        rIDstr = "";
                    
                    controller.ExecuteIntegrationJob(IntegrationJobType.ReportExport, rIDstr.Split(','));
                }
            }
            catch (Exception ex)
            {
                GlobalContext.ExitApplication(string.Format("Application Error.  Message: {0}\nException: {1}", ex.Message, ex.StackTrace), 1);
            }

            // this writes all logging to the logging file
            GlobalContext.ExitApplication("", 0);
        }
    }
}
