using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ALDataIntegrator.Service.Model;
using ALDataIntegrator.RightNowService;
using ALDataIntegrator.Service;
using System.IO;
using Ionic.Zip;
using ALDataIntegrator.Service.Messages;
using ALDataIntegrator.Utility;

namespace ALDataIntegrator.Business
{
    internal class ReportExportProcessor : FileExportProcessorTemplate
    {
        private string JOB_NAME = "ReportExport";
        private string DATE_FORMAT = Properties.Settings.Default.ExportFileDateFormat;
        private List<string> _reportIDList = new List<string>();
        private string _activeReportID = string.Empty;

        RightNowServiceWrapper _serviceWrapper = new RightNowServiceWrapper();

        internal ReportExportProcessor(string[] reportIDs)
        {
            if (reportIDs != null && reportIDs.Count() > 0)
            {
                _reportIDList.AddRange(reportIDs);
            }
        }

        internal override IntegrationJobType IntegrationJobType
        {
            get { return IntegrationJobType.ReportExport; }
        }

        internal override string GetJobName()
        {
            return JOB_NAME + "_RID-" + _activeReportID + "_" + _runTime.ToString("s");
        }

        internal override void ProcessData()
        {
            foreach(string sReportID in _reportIDList)
            {
                try
                {
                    _activeReportID = sReportID;
                    System.Net.ServicePointManager.SecurityProtocol = (System.Net.SecurityProtocolType)3072;
                    AnalyticsReport rpt = _serviceWrapper.getAnalyticsReport(ActiveReportID());
                    if (rpt != null && !string.IsNullOrEmpty(rpt.Name))
                    {
                        string cleanJobName = rpt.Name;
                        foreach (char c in System.IO.Path.GetInvalidFileNameChars())
                        {
                            cleanJobName = cleanJobName.Replace(c, '_');
                        }
                        cleanJobName = cleanJobName.Replace(' ', '_');
                        JOB_NAME = cleanJobName;
                    }
                    base.ProcessData();
                }
                catch (Exception ex)
                {
                    GlobalContext.Log(string.Format("Error processing: {0} - {1}\r\n{2}", GetJobName(), ex.Message, ex.StackTrace), true);
                }
            }
        }

        internal override string ExportFileName
        {
            get { return Properties.Settings.Default.StagingDirectory + "\\" + JOB_NAME + "_RID-" + _activeReportID + "_" + String.Format("{0:" + DATE_FORMAT + "}", _runTime) + ".csv"; }
        }

        internal string ExportFileNameWithoutPath
        {
            get { return JOB_NAME + "_RID-" + _activeReportID + "_" + String.Format("{0:" + DATE_FORMAT + "}", _runTime) + ".csv"; }
        }

        internal override long ActiveReportID()
        {
            return Convert.ToInt64(_activeReportID);
        }

        internal override List<CSVTable> RetrieveData()
        {
            GlobalContext.Log("Retreiving Pending Service Ticket Export Requests...", false);
            List<CSVTable> results = _serviceWrapper.GetAnalyticsReport(ActiveReportID(), null, Properties.Settings.Default.Delimiter);
            return results;
        }

        internal override int ProcessExport(List<CSVTable> csvTables)
        {
            if (csvTables == null || csvTables.Count() <= 0 || csvTables[0].Rows.Length <= 0)
            {
                GlobalContext.Log(string.Format("No Records to process for Report ID: {0}.", _activeReportID), true);
            }

            GlobalContext.Log(string.Format("Creating Report Export CSV File for Report ID: {0}", _activeReportID), true);

            // Generate CSV File
            int processedExportRecordNbr = CreateCSVFile(csvTables);

            return processedExportRecordNbr;
        }

        internal override void PostProcess()
        {
            SFTPUtility sftpUtil = null;

            if (!string.IsNullOrEmpty(Properties.Settings.Default.SFTPHost) &&
                !string.IsNullOrEmpty(Properties.Settings.Default.SFTPUser) &&
                !string.IsNullOrEmpty(Properties.Settings.Default.SFTPPass))
            {
                try
                {
                    // Send CSV file to SFTP location
                    GlobalContext.Log(string.Format("Attempting to connect to remote host: {0}", Properties.Settings.Default.SFTPHost), true);

                    sftpUtil = new SFTPUtility(Properties.Settings.Default.SFTPRemotePath);
                    sftpUtil.PutFile(ExportFileNameWithoutPath);

                    GlobalContext.Log(string.Format("Successfully uploaded file {0} to remote host: {1}", ExportFileName, Properties.Settings.Default.SFTPHost), true);

                    if (Properties.Settings.Default.RemoveExportFileAfterUpload)
                    {
                        DeleteFile(ExportFileName);
                        GlobalContext.Log(string.Format("Removed local copy of file {0}", ExportFileName), true);
                    }
                }
                catch(Exception ex)
                {
                    GlobalContext.Log(string.Format("Error SFTPing file {0} - {1}\r\n{2}", ExportFileNameWithoutPath, ex.Message, ex.StackTrace), true);
                }
            }
        }

        private int CreateCSVFile(List<CSVTable> csvTables)
        {
            int processedExportRecordNbr = 0;
            GlobalContext.Log(string.Format("Creating CSV File: {0}...", ExportFileName), false);

            foreach (CSVTable csvTable in csvTables)
            {
                string strTemp;
                if (csvTable.Rows.Length <= 0)
                {
                    // Don't output empty extract file
                    GlobalContext.Log(string.Format("No Record Returned for CSV File: {0}.", ExportFileName), true);
                    //break;
                }

                // Add header record to extract file if needed
                if (processedExportRecordNbr == 0 && Properties.Settings.Default.FileHasHeaders == true)
                {
                    File.AppendAllText(ExportFileName, csvTable.Columns + "\r\n");
                }

                processedExportRecordNbr += csvTable.Rows.Length;

                foreach (string row in csvTable.Rows)
                {
                    // Are we doing Intra Day Data Report?
                    if (ExportFileName.Contains("Aspect_Intra_Day_Data"))
                    {
                        // Yes - Replace spaces in row with underscores
                        strTemp = row.Replace(" ", "_");
                        // Is file Pipe delimited?
                        if (Properties.Settings.Default.Delimiter == "|")
                        {
                            // Pipe delimited - Replace nulls with 0's
                            strTemp = strTemp.Replace("||||||||", "|0|0|0|0|0|0|0|0");
                            strTemp = strTemp.Replace("||||0|||0|", "|0|0|0|0|0|0|0|0");
                            strTemp = strTemp.Replace("||", "|0|");
                            // Make delimiter into a space
                            strTemp = strTemp.Replace("|", " ");
                            
                        }
                        else
                        {
                            // Not Pipe Delimited (Assume it's comma delimited) - Replace nulls with 0's
                            strTemp = strTemp.Replace(",,,,,,,,", ",0,0,0,0,0,0,0,0");
                            strTemp = strTemp.Replace(",,,,0,,,0,", ",0,0,0,0,0,0,0,0");
                            strTemp = strTemp.Replace(",,", ",0,");
                            // Make delimiter into a space
                            strTemp = strTemp.Replace(",", " ");                           
                        }
                        File.AppendAllText(ExportFileName, strTemp + "\r\n");
                    }
                    else
                    {
                        // Not Intra Day Data Report - Leave code as it was
                        File.AppendAllText(ExportFileName, row + "\r\n");
                    }

                }  // end foreach (string row in csvTable.Rows)

            }
            return processedExportRecordNbr;
        }

        private static string Trim(string data)
        {
            string newData = data;
            if (newData != null)
            {
                if (newData.StartsWith("\""))
                    newData = newData.Substring(1);
                if (newData.EndsWith("\""))
                    newData = newData.Substring(0, newData.Length - 1);
                if (newData.StartsWith("\'"))
                    newData = newData.Substring(1);
                if (newData.EndsWith("\'"))
                    newData = newData.Substring(0, newData.Length - 1);
                newData = newData.Trim();
            }
            return newData;
        }

        static long GetDirectorySize(string directoryPath)
        {
            string[] fileNames = Directory.GetFiles(directoryPath, "*.*");
            long directorySize = 0;
            foreach (string name in fileNames)
            {
                FileInfo info = new FileInfo(name);
                directorySize += info.Length;
            }
            return directorySize;
        }

        static void DeleteFile(string fileName)
        {
            try
            {
                File.Delete(fileName);
            }
            catch (IOException ex)
            {
                GlobalContext.Log(string.Format("Error removing file: {0} - {1}\r\n{2}", fileName, ex.Message, ex.StackTrace), true);
            }
        }

        static void DeleteFolder(string folderName, bool recursive)
        {
            try
            {
                Directory.Delete(folderName, recursive);
            }
            catch (IOException ex)
            {
                GlobalContext.Log(string.Format("Erorr removing file: {0} - {1}\r\n{2}", folderName, ex.Message, ex.StackTrace), true);
            }
        }
    }
}
