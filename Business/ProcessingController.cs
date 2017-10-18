using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ALDataIntegrator.Properties;
using ALDataIntegrator.Service;
using ALDataIntegrator.Service.Model;
using ALDataIntegrator.Utility;

namespace ALDataIntegrator.Business
{
    internal class ProcessingController
    {
        internal ProcessingController()
        {
        }

        internal void ExecuteIntegrationJob(IntegrationJobType jobType, string[] reportIDs) 
        {
            BaseProcessorTemplate processor = null;

            switch (jobType)
            {
                case IntegrationJobType.ReportExport:
                    processor = new ReportExportProcessor(reportIDs);
                    processor.ProcessData();
                    break;
            }
        }
    }
}
