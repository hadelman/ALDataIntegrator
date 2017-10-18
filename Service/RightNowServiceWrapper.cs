using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using ALDataIntegrator.Properties;
using ALDataIntegrator.RightNowService;
using ALDataIntegrator.Service.Messages;
using System.Configuration;

namespace ALDataIntegrator.Service
{
    internal class RightNowServiceWrapper
    {
        private readonly string _user = Settings.Default.RightNowUser;
        private readonly string _pass = Settings.Default.RightNowPass;

        private RightNowSyncPortClient _client;
        private ClientInfoHeader _clientInfoHeader;

        internal RightNowServiceWrapper()
        {
            SetupRightNowClient();
        }

        private void SetupRightNowClient()
        {
            var configBindings = ConfigurationManager.GetSection("system.serviceModel/bindings") as
                        System.ServiceModel.Configuration.BindingsSection;
            var configClient = ConfigurationManager.GetSection("system.serviceModel/client") as
                        System.ServiceModel.Configuration.ClientSection;

            var binding = new BasicHttpBinding(BasicHttpSecurityMode.TransportWithMessageCredential);
            binding.Security.Message.ClientCredentialType = BasicHttpMessageCredentialType.UserName;
            binding.MaxBufferSize = int.MaxValue;
            binding.MaxReceivedMessageSize = int.MaxValue;
            binding.MaxBufferPoolSize = int.MaxValue;

            var endPointAddr = new EndpointAddress(configClient.Endpoints[0].Address);
            _client = new RightNowSyncPortClient(binding, endPointAddr);
            if (_client.ClientCredentials != null)
            {
                _client.ClientCredentials.UserName.UserName = _user;
                _client.ClientCredentials.UserName.Password = _pass;
            }

            BindingElementCollection elements = _client.Endpoint.Binding.CreateBindingElements();
            elements.Find<SecurityBindingElement>().IncludeTimestamp = false;
            elements.Find<TransportBindingElement>().MaxReceivedMessageSize = int.MaxValue;
            elements.Find<TransportBindingElement>().MaxBufferPoolSize = int.MaxValue;

            _client.Endpoint.Binding = new CustomBinding(elements);
            _client.Endpoint.Binding.SendTimeout = configBindings.CustomBinding.ConfiguredBindings[0].SendTimeout;
            _client.Endpoint.Binding.ReceiveTimeout = configBindings.CustomBinding.ConfiguredBindings[0].ReceiveTimeout;
            _client.Endpoint.Binding.OpenTimeout = configBindings.CustomBinding.ConfiguredBindings[0].OpenTimeout;
            _client.Endpoint.Binding.CloseTimeout = configBindings.CustomBinding.ConfiguredBindings[0].CloseTimeout;

            _clientInfoHeader = new ClientInfoHeader { AppID = "AmberLeaf Data Integrator" };
        }

        internal CreateObjectsResponse CreateObjects(RNObject[] objects)
        {
            var result = new List<RNObject>();

            bool mustRetry = false;     // used to determine if we should retry a failed job
            int retryTimes = 0;         // we don't want to endlessly retry, so we only will retry 3 times
            do
            {
                try
                {
                    var soapResult = _client.Create(_clientInfoHeader, objects, new CreateProcessingOptions { SuppressExternalEvents = false, SuppressRules = false });
                    if (soapResult.Any())
                        result = soapResult.ToList();

                    return new CreateObjectsResponse
                    {
                        CreatedObjects = result,
                        Successful = true,
                        SuccessfulSet = true
                    };
                }
                catch (Exception ex)
                {
                    GlobalContext.Log(string.Format("Failed Create: Retry {0}: {1}", retryTimes, ex.Message), true);
                    GlobalContext.Log(string.Format("Failed Create: Retry {0}: {1}{2}{3}", retryTimes, ex.Message, Environment.NewLine, ex.StackTrace), false);

                    string innerExMsg = GetInnerExceptionMessage(ex);
                    if (!string.IsNullOrEmpty(innerExMsg))
                    {
                        GlobalContext.Log(string.Format("Failed Create: Retry {0}: InnerException Messages: {1}", retryTimes, innerExMsg), true);
                    }

                    if (retryTimes < 3)
                    {
                        // if we haven't retried 3 times then we retry the load again
                        mustRetry = true;
                        retryTimes++;
                    }
                    else
                    {
                        // don't retry for 3rd retry
                        return new CreateObjectsResponse
                        {
                            CreatedObjects = null,
                            Successful = false,
                            SuccessfulSet = true,
                            Details = ex.Message
                        };
                    }
                }
                GlobalContext.Log(string.Format("Create Must Retry {0}", mustRetry), false);
            } while (mustRetry);
            GlobalContext.Log("Create End: This code should never be hit.", false);
            return null;        // this code should never be hit
        }

        internal GenericServiceResponse UpdateObjects(RNObject[] objects)
        {
            bool mustRetry = false;     // used to determine if we should retry a failed job
            int retryTimes = 0;         // we don't want to endlessly retry, so we only will retry 3 times
            do
            {
                try
                {
                    _client.Update(_clientInfoHeader, objects, new UpdateProcessingOptions { SuppressExternalEvents = false, SuppressRules = false });

                    return new GenericServiceResponse()
                    {
                        Successful = true
                    };
                }
                catch (Exception ex)
                {
                    GlobalContext.Log(string.Format("Failed Update: Retry {0}: {1}", retryTimes, ex.Message), true);
                    GlobalContext.Log(string.Format("Failed Update: Retry {0}: {1}{2}{3}", retryTimes, ex.Message, Environment.NewLine, ex.StackTrace), false);

                    string innerExMsg = GetInnerExceptionMessage(ex);
                    if (!string.IsNullOrEmpty(innerExMsg))
                    {
                        GlobalContext.Log(string.Format("Failed Update: Retry {0}: InnerException Messages: {1}", retryTimes, innerExMsg), true);
                    }

                    if (retryTimes < 3)
                    {
                        // if we haven't retried 3 times then we retry the load again
                        mustRetry = true;
                        retryTimes++;
                    }
                    else
                    {
                        // don't retry for 3rd retry
                        return new GenericServiceResponse()
                        {
                            SuccessfulSet = true,
                            Successful = false,
                            Details = ex.ToString()
                        };
                    }
                }
                GlobalContext.Log(string.Format("Update Must Retry {0}", mustRetry), false);
            } while (mustRetry);
            GlobalContext.Log("Update End: This code should never be hit.", false);
            return null;        // this code should never be hit
        }

        internal GenericServiceResponse DestroyObjects(RNObject[] objects)
        {
            bool mustRetry = false;     // used to determine if we should retry a failed job
            int retryTimes = 0;         // we don't want to endlessly retry, so we only will retry 3 times
            do
            {
                try
                {
                    _client.Destroy(_clientInfoHeader, objects, new DestroyProcessingOptions { SuppressExternalEvents = false, SuppressRules = false });

                    return new GenericServiceResponse()
                    {
                        Successful = true
                    };
                }
                catch (Exception ex)
                {
                    GlobalContext.Log(string.Format("Failed Destroy: Retry {0}: {1}", retryTimes, ex.Message), true);
                    GlobalContext.Log(string.Format("Failed Destroy: Retry {0}: {1}{2}{3}", retryTimes, ex.Message, Environment.NewLine, ex.StackTrace), false);

                    string innerExMsg = GetInnerExceptionMessage(ex);
                    if (!string.IsNullOrEmpty(innerExMsg))
                    {
                        GlobalContext.Log(string.Format("Failed Destroy: Retry {0}: InnerException Messages: {1}", retryTimes, innerExMsg), true);
                    }

                    if (retryTimes < 3)
                    {
                        // if we haven't retried 3 times then we retry the load again
                        mustRetry = true;
                        retryTimes++;
                    }
                    else
                    {
                        // don't retry for 3rd retry
                        return new GenericServiceResponse()
                        {
                            SuccessfulSet = true,
                            Successful = false,
                            Details = ex.ToString()
                        };
                    }
                }
                GlobalContext.Log(string.Format("Destroy Must Retry {0}", mustRetry), false);
            } while (mustRetry);
            GlobalContext.Log("Destroy End: This code should never be hit.", false);
            return null;        // this code should never be hit
        }

        internal CSVQueryResponse QueryCSV(string queryString, string delimiter, int pageSize)
        {
            bool mustRetry = false;     // used to determine if we should retry a failed job
            int retryTimes = 0;         // we don't want to endlessly retry, so we only will retry 3 times
            do
            {
                try
                {
                    CSVTableSet result = null;

                    byte[] byteArray;
                    result = _client.QueryCSV(_clientInfoHeader, queryString, pageSize, delimiter, false, true, out byteArray);

                    return new CSVQueryResponse
                    {
                        TableSet = result,
                        Successful = true,
                        SuccessfulSet = true
                    };
                }
                catch (Exception ex)
                {
                    GlobalContext.Log(string.Format("Failed QueryCSV: Retry {0}: {1}", retryTimes, ex.Message), true);
                    GlobalContext.Log(string.Format("Failed QueryCSV: Retry {0}: {1}{2}{3}", retryTimes, ex.Message, Environment.NewLine, ex.StackTrace), false);

                    if (retryTimes < 3)
                    {
                        // if we haven't retried 3 times then we retry the load again
                        mustRetry = true;
                        retryTimes++;
                    }
                    else
                    {
                        // don't retry for 3rd retry
                        return new CSVQueryResponse
                        {
                            TableSet = null,
                            Successful = false,
                            SuccessfulSet = true,
                            Details = ex.Message
                        };
                    }
                }
                GlobalContext.Log(string.Format("QueryCSV Must Retry {0}", mustRetry), false);
            } while (mustRetry);
            GlobalContext.Log("QueryCSV End: This code should never be hit.", false);
            return null;        // this code should never be hit
        }

        internal ObjectQueryResponse QueryObjects(string queryString, RNObject[] objectTemplates, int pageSize)
        {
            bool mustRetry = false;     // used to determine if we should retry a failed job
            int retryTimes = 0;         // we don't want to endlessly retry, so we only will retry 3 times
            do
            {
                try
                {
                    QueryResultData[] queryObjects = null;
                    queryObjects = _client.QueryObjects(_clientInfoHeader, queryString, objectTemplates, pageSize);

                    return new ObjectQueryResponse
                    {
                        QueryObjects = queryObjects,
                        Successful = true,
                        SuccessfulSet = true
                    };
                }
                catch (Exception ex)
                {
                    GlobalContext.Log(string.Format("Failed QueryObjects: Retry {0}: {1}", retryTimes, ex.Message), true);
                    GlobalContext.Log(string.Format("Failed QueryObjects: Retry {0}: {1}{2}{3}", retryTimes, ex.Message, Environment.NewLine, ex.StackTrace), false);

                    if (retryTimes < 3)
                    {
                        // if we haven't retried 3 times then we retry the load again
                        mustRetry = true;
                        retryTimes++;
                    }
                    else
                    {
                        // don't retry for 3rd retry
                        return new ObjectQueryResponse
                        {
                            QueryObjects = null,
                            Successful = false,
                            SuccessfulSet = true,
                            Details = ex.Message
                        };
                    }
                }
                GlobalContext.Log(string.Format("QueryObjects Must Retry {0}", mustRetry), false);
            } while (mustRetry);
            GlobalContext.Log("QueryObjects End: This code should never be hit.", false);
            return null;        // this code should never be hit
        }

        internal ServiceBatchResponse Batch(BatchRequestItem[] batchItems)
        {
            bool mustRetry = false;     // used to determine if we should retry a failed job
            int retryTimes = 0;         // we don't want to endlessly retry, so we only will retry 3 times
            do
            {
                try
                {
                    BatchResponseItem[] batchReponseItems = _client.Batch(_clientInfoHeader, batchItems);

                    return new ServiceBatchResponse()
                    {
                        BatchResponseItems = batchReponseItems,
                        Successful = true
                    };
                }
                catch (Exception ex)
                {
                    GlobalContext.Log(string.Format("Failed Batch: Retry {0}: {1}", retryTimes, ex.Message), true);
                    GlobalContext.Log(string.Format("Failed Batch: Retry {0}: {1}{2}{3}", retryTimes, ex.Message, Environment.NewLine, ex.StackTrace), false);

                    if (retryTimes < 3)
                    {
                        // if we haven't retried 3 times then we retry the load again
                        mustRetry = true;
                        retryTimes++;
                    }
                    else
                    {
                        // don't retry for 3rd retry
                        return new ServiceBatchResponse()
                        {
                            SuccessfulSet = true,
                            Successful = false,
                            Details = ex.ToString()
                        };
                    }
                }
                GlobalContext.Log(string.Format("Batch Must Retry {0}", mustRetry), false);
            } while (mustRetry);
            GlobalContext.Log("Batch End: This code should never be hit.", false);
            return null;        // this code should never be hit
        }

        internal GetFileAttachmentDataResponse GetFileData(RNObject rnObject, long fileID, bool disableMTOM)
        {
            var result = new List<RNObject>();

            bool mustRetry = false;     // used to determine if we should retry a failed job
            int retryTimes = 0;         // we don't want to endlessly retry, so we only will retry 3 times
            do
            {
                try
                {
                    ID idObj = new ID
                    {
                        id = fileID,
                        idSpecified = true
                    };

                    byte[] fileData = _client.GetFileData(_clientInfoHeader, rnObject, idObj, disableMTOM);

                    return new GetFileAttachmentDataResponse
                    {
                        FileData = fileData,
                        Successful = true,
                        SuccessfulSet = true
                    };
                }
                catch (Exception ex)
                {
                    GlobalContext.Log(string.Format("Failed GetFileData: Retry {0}: {1}", retryTimes, ex.Message), true);
                    GlobalContext.Log(string.Format("Failed GetFileData: Retry {0}: {1}{2}{3}", retryTimes, ex.Message, Environment.NewLine, ex.StackTrace), false);

                    if (retryTimes < 3)
                    {
                        // if we haven't retried 3 times then we retry the load again
                        mustRetry = true;
                        retryTimes++;
                    }
                    else
                    {
                        // don't retry for 3rd retry
                        return new GetFileAttachmentDataResponse
                        {
                            FileData = null,
                            Successful = false,
                            SuccessfulSet = true,
                            Details = ex.Message
                        };
                    }
                }
                GlobalContext.Log(string.Format("GetFileData Must Retry {0}", mustRetry), false);
            } while (mustRetry);
            GlobalContext.Log("GetFileData End: This code should never be hit.", false);
            return null;        // this code should never be hit
        }

        internal RunAnalyticsResponse GetAnalyticsReportResults(long reportId,
            int limit = 100, int start = 0, string delimiter = ",")
        {
            //Create a filter and specify the filter name
            //Assigning the filter created in desktop agent to the new analytics filter 
            AnalyticsReportFilter filter = new AnalyticsReportFilter();

            //Apply the filter to the AnalyticsReport object
            return GetAnalyticsReportResults(reportId,
                new AnalyticsReportFilter[] { filter }, limit, start, delimiter);
        }

        internal string GetInnerExceptionMessage(Exception ex)
        {
            if (ex == null)
                return "";
            
            List<string> listInnerMsgs = new List<string>();
            Exception exInner = ex;
            if (exInner.InnerException != null)
            {
                while (exInner.InnerException != null)
                {
                    exInner = exInner.InnerException;
                    listInnerMsgs.Add(exInner.Message);
                }
            }
            return string.Join(Environment.NewLine, listInnerMsgs.ToArray());
        }

        #region RunAnalytics Operations

        internal RunAnalyticsResponse GetAnalyticsReportResults(long reportId, AnalyticsReportFilter[] filters,
            int limit = 100, int start = 0, string delimiter = ",")
        {
            //Create new AnalyticsReport Object
            AnalyticsReport analyticsReport = new AnalyticsReport();
            //Specify a report ID of Public Reports>Common>Data integration>Opportunities
            ID reportID = new ID();
            reportID.id = reportId;
            reportID.idSpecified = true;
            analyticsReport.ID = reportID;

            analyticsReport.Filters = filters;

            GetProcessingOptions processingOptions = new GetProcessingOptions();
            processingOptions.FetchAllNames = true;

            RNObject[] getAnalyticsObjects = new RNObject[] { analyticsReport };
            CSVTableSet thisSet = new CSVTableSet();
            byte[] ignore;

            bool mustRetry = false;     // used to determine if we should retry a failed job
            int retryTimes = 0;         // we don't want to endlessly retry, so we only will retry 3 times
            do
            {
                try
                {
                    thisSet = _client.RunAnalyticsReport(_clientInfoHeader, analyticsReport, limit, start, delimiter, false, true, out ignore);

                    return new RunAnalyticsResponse
                    {
                        RunAnalyticsTableSet = thisSet,
                        Successful = true,
                        SuccessfulSet = true
                    };
                }
                catch (Exception ex)
                {
                    GlobalContext.Log(string.Format("Failed RunAnalyticsReport: Retry {0}: {1}", retryTimes, ex.Message), true);
                    GlobalContext.Log(string.Format("Failed RunAnalyticsReport: Retry {0}: {1}{2}{3}", retryTimes, ex.Message, Environment.NewLine, ex.StackTrace), false);

                    if (retryTimes < 3)
                    {
                        // if we haven't retried 3 times then we retry the load again
                        mustRetry = true;
                        retryTimes++;
                    }
                    else
                    {
                        // don't retry for 3rd retry
                        return new RunAnalyticsResponse
                        {
                            Successful = false,
                            SuccessfulSet = false,
                            Details = ex.Message
                        };
                    }
                }
                GlobalContext.Log(string.Format("RunAnalyticsReport Must Retry {0}", mustRetry), false);
            } while (mustRetry);
            GlobalContext.Log("RunAnalyticsReport End: This code should never be hit.", false);
            return null;        // this code should never be hit
        }

        /// <summary>
        /// Generic helper method to create a lookup table
        /// This is used for performance reasons, so that main lookups all occur in memory instead of database lookups
        /// </summary>
        /// <param name="query"></param>
        /// <param name="limit"></param>
        /// <param name="useCache"></param>
        /// <returns></returns>
        public List<CSVTable> GetAnalyticsReport(long reportId, AnalyticsReportFilter[] filters, string delimiter = ",")
        {
            bool mustRetry = false;     // used to determine if we should retry a failed job
            int retryTimes = 0;         // we don't want to endlessly retry, so we only will retry 3 times
            
            if (filters == null)
            {
                filters = new AnalyticsReportFilter[] { };
            }
            do
            {
                try
                {
                    List<CSVTable> resultTableList = new List<CSVTable>();

                    int limit = 10000;
                    long rowsReturned = Int64.MaxValue;
                    long totalRowsReturned = 0;

                    int offset = 0;
                    while (rowsReturned >= limit)
                    {
                        var queryResult = GetAnalyticsReportResults(reportId, filters.ToArray(), limit, offset, delimiter); // "|");
                        if (!queryResult.Successful)
                        {
                            throw new Exception(queryResult.Details);
                        }

                        CSVTableSet thisset = queryResult.RunAnalyticsTableSet;
                        foreach (CSVTable table in thisset.CSVTables)
                        {
                            resultTableList.Add(table);
                            rowsReturned = table.Rows.Length;
                            totalRowsReturned += rowsReturned;
                            //break;
                        }
                        offset = offset + limit;
                    }

                    GlobalContext.Log(string.Format("GetAnalyticsReport Added {0} tables with a total of {1} records for ReportID {2}",
                        resultTableList.Count, totalRowsReturned, reportId), false);
                    return resultTableList;
                }
                catch (Exception ex)
                {
                    GlobalContext.Log(string.Format("GetAnalyticsReport Error calling ReportID {0}: Retry {1}: {2}\n{3}",
                        reportId, retryTimes, ex.Message, ex.StackTrace), false);
                    if (retryTimes < 4)
                    {
                        // if we haven't retried 4 times then we retry the load again
                        mustRetry = true;
                        retryTimes++;
                    }
                    else
                    {
                        throw ex;      // don't retry for 4th retry
                    }
                }
                GlobalContext.Log(string.Format("GetAnalyticsReport Must Retry {0}", mustRetry), false);
            } while (mustRetry);
            GlobalContext.Log("GetAnalyticsReport End: This code should never be hit.", false);
            return null;        // this code should never be hit
        }

        #endregion

        #region Lookup Data Operations

        /// <summary>
        /// Generic helper method to create a lookup table
        /// This is used for performance reasons, so that main lookups all occur in memory instead of database lookups
        /// </summary>
        /// <param name="query"></param>
        /// <param name="limit"></param>
        /// <param name="useCache"></param>
        /// <returns></returns>
        public Dictionary<string, long> GetLookupTable(string baseQueryString)
        {
            bool mustRetry = false;     // used to determine if we should retry a failed job
            int retryTimes = 0;         // we don't want to endlessly retry, so we only will retry 3 times
            do
            {
                try
                {
                    Dictionary<string, long> lookupTable = new Dictionary<string, long>();

                    int limit = 10000;
                    long rowsReturned = Int64.MaxValue;
                    long totalRowsReturned = 0;

                    int offset = 0;
                    while (rowsReturned >= limit)
                    {
                        string queryString = baseQueryString + " LIMIT " + limit + " OFFSET " + offset;
                        var queryResult = QueryCSV(queryString, "|", limit);
                        if (!queryResult.Successful)
                        {
                            throw new Exception(queryResult.Details);
                        }

                        CSVTableSet thisset = queryResult.TableSet;
                        foreach (CSVTable table in thisset.CSVTables)
                        {
                            rowsReturned = table.Rows.Length;
                            totalRowsReturned += rowsReturned;
                            foreach (string row in table.Rows)
                            {
                                String[] columns = row.Split(new string[] { "|" }, StringSplitOptions.None);
                                if (!String.IsNullOrEmpty(columns[0]) && !String.IsNullOrEmpty(columns[1]) && (columns[0] != "0")
                                    && !lookupTable.ContainsKey(columns[0]))
                                {
                                    lookupTable.Add(columns[0], Convert.ToInt64(columns[1]));
                                }
                                else
                                {
                                    GlobalContext.Log(string.Format("GetLookupTable: Ignoring Duplicate ID Found: {0} : {1} : Query String: {2}",
                                        columns[0], columns[1], queryString), true);
                                }
                            }
                            break;
                        }
                        offset = offset + limit;
                    }

                    GlobalContext.Log("GetLookupTable Added " + lookupTable.Count + " Query Results: " + baseQueryString, false);
                    return lookupTable;
                }
                catch (Exception ex)
                {
                    GlobalContext.Log(string.Format("GetLookupTable Error: Retry {0}: {1}\n{2}", retryTimes, ex.Message, ex.StackTrace), false);
                    if (retryTimes < 4)
                    {
                        // if we haven't retried 4 times then we retry the load again
                        mustRetry = true;
                        retryTimes++;
                    }
                    else
                    {
                        throw ex;      // don't retry for 4th retry
                    }
                }
                GlobalContext.Log(string.Format("GetLookupTable Must Retry {0}", mustRetry), false);
            } while (mustRetry);
            GlobalContext.Log("GetLookupTable End: This code should never be hit.", false);
            return null;        // this code should never be hit
        }

        /// <summary>
        /// Generic helper method to create a lookup table
        /// This is used for performance reasons, so that main lookups all occur in memory instead of database lookups
        /// </summary>
        /// <param name="query"></param>
        /// <param name="limit"></param>
        /// <param name="useCache"></param>
        /// <returns></returns>
        public Dictionary<long, long> GetLookupTable2(string baseQueryString)
        {
            bool mustRetry = false;     // used to determine if we should retry a failed job
            int retryTimes = 0;         // we don't want to endlessly retry, so we only will retry 3 times
            do
            {
                try
                {
                    Dictionary<long, long> lookupTable = new Dictionary<long, long>();

                    int limit = 10000;
                    long rowsReturned = Int64.MaxValue;
                    long totalRowsReturned = 0;

                    int offset = 0;
                    while (rowsReturned >= limit)
                    {
                        string queryString = baseQueryString + " LIMIT " + limit + " OFFSET " + offset;
                        var queryResult = QueryCSV(queryString, "|", limit);
                        if (!queryResult.Successful)
                        {
                            throw new Exception(queryResult.Details);
                        }

                        CSVTableSet thisset = queryResult.TableSet;
                        foreach (CSVTable table in thisset.CSVTables)
                        {
                            rowsReturned = table.Rows.Length;
                            totalRowsReturned += rowsReturned;
                            foreach (string row in table.Rows)
                            {
                                String[] columns = row.Split(new string[] { "|" }, StringSplitOptions.None);
                                if (!String.IsNullOrEmpty(columns[0]) && !String.IsNullOrEmpty(columns[1]) && (columns[0] != "0"))
                                {
                                    long key = Convert.ToInt64(columns[0]);
                                    if (!lookupTable.ContainsKey(key))
                                    {
                                        lookupTable.Add(key, Convert.ToInt64(columns[1]));
                                    }
                                    else
                                    {
                                        GlobalContext.Log(string.Format("GetLookupTable2: Ignoring Duplicate ID Found: {0} : {1} : Query String: {2}",
                                            columns[0], columns[1], queryString), true);
                                    }
                                }
                            }
                            break;
                        }
                        offset = offset + limit;
                    }

                    GlobalContext.Log("GetLookupTable2 Added " + lookupTable.Count + " Query Results: " + baseQueryString, false);
                    return lookupTable;
                }
                catch (Exception ex)
                {
                    GlobalContext.Log(string.Format("GetLookupTable2 Error: Retry {0}: {1}\n{2}", retryTimes, ex.Message, ex.StackTrace), false);
                    if (retryTimes < 4)
                    {
                        // if we haven't retried 4 times then we retry the load again
                        mustRetry = true;
                        retryTimes++;
                    }
                    else
                    {
                        throw ex;      // don't retry for 4th retry
                    }
                }
                GlobalContext.Log(string.Format("GetLookupTable2 Must Retry {0}", mustRetry), false);
            } while (mustRetry);
            GlobalContext.Log("GetLookupTable2 End: This code should never be hit.", false);
            return null;        // this code should never be hit
        }

        /// <summary>
        /// Generic helper method to create a lookup table
        /// This is used for performance reasons, so that main lookups all occur in memory instead of database lookups
        /// </summary>
        /// <param name="query"></param>
        /// <param name="limit"></param>
        /// <param name="useCache"></param>
        /// <returns></returns>
        public Dictionary<string, string> GetLookupTableByReportID(int reportID)
        {
            bool mustRetry = false;     // used to determine if we should retry a failed job
            int retryTimes = 0;         // we don't want to endlessly retry, so we only will retry 3 times
            do
            {
                try
                {
                    Dictionary<string, string> lookupTable = new Dictionary<string, string>();

                    int limit = 10000;
                    long rowsReturned = Int64.MaxValue;
                    long totalRowsReturned = 0;

                    int offset = 0;
                    while (rowsReturned >= limit)
                    {
                        GlobalContext.Log(string.Format("Retrieving {0} records from offset {1}", limit, offset), true);
                        var queryResult = GetAnalyticsReportResults(reportID, null, limit, offset, "|");
                        if (!queryResult.Successful)
                        {
                            GlobalContext.Log(string.Format("Error occurred retrieving records - {0}", queryResult.Details), true);
                            throw new Exception(queryResult.Details);
                        }

                        foreach (CSVTable table in queryResult.RunAnalyticsTableSet.CSVTables)
                        {
                            rowsReturned = table.Rows.Length;
                            totalRowsReturned += rowsReturned;
                            foreach (string row in table.Rows)
                            {
                                String[] columns = row.Split(new string[] { "|" }, StringSplitOptions.None);
                                if (!String.IsNullOrEmpty(columns[0]) && !String.IsNullOrEmpty(columns[1]) && (columns[0] != "0")
                                    && !lookupTable.ContainsKey(columns[0]))
                                {
                                    lookupTable.Add(columns[0], columns[1]);
                                }
                                else
                                {
                                    GlobalContext.Log(string.Format("GetLookupTable: Ignoring Duplicate ID Found: {0} : {1} : ReportID: {2}",
                                        columns[0], columns[1], reportID), true);
                                }
                            }
                            break;
                        }
                        offset = offset + limit;
                    }

                    GlobalContext.Log("GetLookupTable Added " + lookupTable.Count + " ReportID: " + reportID, false);
                    return lookupTable;
                }
                catch (Exception ex)
                {
                    GlobalContext.Log(string.Format("GetLookupTable Error: Retry {0}: {1}\n{2}", retryTimes, ex.Message, ex.StackTrace), false);
                    if (retryTimes < 4)
                    {
                        // if we haven't retried 4 times then we retry the load again
                        mustRetry = true;
                        retryTimes++;
                    }
                    else
                    {
                        throw ex;      // don't retry for 4th retry
                    }
                }
                GlobalContext.Log(string.Format("GetLookupTable Must Retry {0}", mustRetry), false);
            } while (mustRetry);
            GlobalContext.Log("GetLookupTable End: This code should never be hit.", false);
            return null;        // this code should never be hit
        }

        /// <summary>
        /// Generic helper method to create a lookup table
        /// This is used for performance reasons, so that main lookups all occur in memory instead of database lookups
        /// </summary>
        public List<RNObject> GetObjects(string baseQueryString, RNObject[] objectTemplates)
        {
            bool mustRetry = false;     // used to determine if we should retry a failed job
            int retryTimes = 0;         // we don't want to endlessly retry, so we only will retry 3 times
            do
            {
                try
                {
                    List<RNObject> rnObjectList = new List<RNObject>();

                    int limit = 10000;
                    long rowsReturned = Int64.MaxValue;
                    long totalRowsReturned = 0;

                    int offset = 0;
                    while (rowsReturned >= limit)
                    {
                        string queryString = baseQueryString + " LIMIT " + limit + " OFFSET " + offset;
                        var queryResult = QueryObjects(queryString, objectTemplates, limit);
                        if (!queryResult.Successful)
                        {
                            throw new Exception(queryResult.Details);
                        }

                        RNObject[] rnObjects = queryResult.QueryObjects[0].RNObjectsResult;
                        if (rnObjects != null)
                        {
                            rowsReturned = rnObjects.Length;
                            totalRowsReturned += rowsReturned;
                            rnObjectList.AddRange(new List<RNObject>(rnObjects));
                        }

                        offset = offset + limit;
                    }

                    GlobalContext.Log("GetObjects Added " + rnObjectList.Count + " Query Results: " + baseQueryString, false);
                    return rnObjectList;
                }
                catch (Exception ex)
                {
                    GlobalContext.Log(string.Format("GetObjects Error: Retry {0}: {1}\n{2}", retryTimes, ex.Message, ex.StackTrace), false);
                    if (retryTimes < 4)
                    {
                        // if we haven't retried 4 times then we retry the load again
                        mustRetry = true;
                        retryTimes++;
                    }
                    else
                    {
                        throw ex;      // don't retry for 4th retry
                    }
                }
                GlobalContext.Log(string.Format("GetObjects Must Retry {0}", mustRetry), false);
            } while (mustRetry);
            GlobalContext.Log("GetObjects End: This code should never be hit.", false);
            return null;        // this code should never be hit
        }

        public AnalyticsReport getAnalyticsReport(long reportID)
        {
            AnalyticsReport analyticsReport = new AnalyticsReport();
            ID analyticsReportID = new ID();
            analyticsReportID.id = reportID;
            analyticsReportID.idSpecified = true;
            analyticsReport.ID = analyticsReportID;

            AnalyticsReportFilter filter = new AnalyticsReportFilter();
            analyticsReport.Filters = (new AnalyticsReportFilter[] { filter });

            GetProcessingOptions processingOptions = new GetProcessingOptions();
            processingOptions.FetchAllNames = true;

            RNObject[] getAnalyticsObjects = new RNObject[] { analyticsReport };
            RNObject[] rnObjects = _client.Get(_clientInfoHeader, getAnalyticsObjects, processingOptions);
            AnalyticsReport report = (AnalyticsReport)rnObjects[0];
            return report;
        }

        #endregion
    }
}
