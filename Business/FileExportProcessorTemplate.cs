using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ALDataIntegrator.Service.Messages;
using ALDataIntegrator.Service.Model;
using ALDataIntegrator.Service;
using ALDataIntegrator.RightNowService;

namespace ALDataIntegrator.Business
{
    internal abstract class FileExportProcessorTemplate : BaseProcessorTemplate
    {
        internal abstract List<CSVTable> RetrieveData();
        internal abstract string ExportFileName { get; }
        internal abstract int ProcessExport(List<CSVTable> csvTables);
        internal abstract void PostProcess();

        internal abstract long ActiveReportID();

        internal override void ProcessData()
        {
            _runTime = DateTime.Now;
            GlobalContext.Log(string.Format("Processing Job: {0}", GetJobName()), true);

            try
            {
                SetSuccess(0);
                SetFail(0);
                SetTotalRecords(0);
                SetTotalUniqueRecords(0);

                // Get list of ExportRequest objects
                var reportCSVTables = RetrieveData();
                if (reportCSVTables != null && reportCSVTables.Count > 0)
                {
                    // create OSvC integration job for tracking purposes
                    CreateJob();

                    try
                    {
                        int processExportRecords = ProcessExport(reportCSVTables);

                        SetTotalRecords(processExportRecords);
                        IncreaseTotalUniqueRecords(processExportRecords);
                        if (processExportRecords < 0)
                            IncreaseFail(1);
                        else
                            IncreaseSuccess(processExportRecords);

                        UpdateJob();
                    }
                    catch (Exception ex)
                    {
                        GlobalContext.Log(string.Format("Error processing ExportRequestID: {0} - {1}\r\n{2}", ActiveReportID(), ex.Message, ex.StackTrace), true);
                    }

                    // Complete Integration Job status
                    CompleteJob();
                }

                PostProcess();
            }
            catch (Exception ex)
            {
                GlobalContext.Log(string.Format("Error processing: {0} - {1}\r\n{2}", GetJobName(), ex.Message, ex.StackTrace), true);
            }

        }

        internal string GetFileName(string file)
        {
            string fileName = file.Substring(Properties.Settings.Default.StagingDirectory.Length);
            if (fileName.StartsWith("\\"))
                fileName = fileName.Substring(1);
            return fileName;
        }
    }
}
