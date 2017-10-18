using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ALDataIntegrator.Properties;
using ALDataIntegrator.Service;
using ALDataIntegrator.Service.Messages;
using ALDataIntegrator.Service.Model;
using ALDataIntegrator.Utility;
using System.Data;

namespace ALDataIntegrator.Business
{
    internal abstract class BaseProcessorTemplate
    {
        internal abstract IntegrationJobType IntegrationJobType { get; }
        internal abstract string GetJobName();
        internal abstract void ProcessData();

        internal readonly ALRightNowServiceFacade _serviceFacade = new ALRightNowServiceFacade();
        internal DateTime _runTime;
        private static byte[] _fileContent = null;
        private long? _integrationJobID;
        private int _totalRecords = 0;
        private int _totalUniqueRecords = 0;
        private int _success = 0;
        private int _fail = 0;

        internal void SetTotalRecords(int totalRecords)
        {
            _totalRecords = totalRecords;
        }
        internal void SetTotalUniqueRecords(int totalUniqueRecords)
        {
            _totalUniqueRecords = totalUniqueRecords;
        }
        internal void IncreaseTotalUniqueRecords(int totalUniqueRecords)
        {
            _totalUniqueRecords += totalUniqueRecords;
        }
        internal void SetSuccess(int successRecords)
        {
            _success = successRecords;
        }
        internal void IncreaseSuccess(int value)
        {
            _success += value;
        }
        internal void SetFail(int failRecords)
        {
            _fail = failRecords;
        }
        internal void IncreaseFail(int value)
        {
            _fail += value;
        }

        internal void CreateJob()
        {
            string hostName = System.Net.Dns.GetHostEntry("").HostName;
            _integrationJobID = _serviceFacade.CreateIntegrationJob(
                new CreateIntegrationJobRequest
                {
                    FileName = GetJobName(),
                    JobType = IntegrationJobType,
                    HostName = hostName
                });
        }

        internal void UpdateJob()
        {
            if (_integrationJobID == null)
                return;

            var request = new UpdateIntegrationJobRequest()
            {
                ID = (long)_integrationJobID,
                RecordsTotal = _totalRecords,
                RecordsTotalUnique = _totalUniqueRecords,
                RecordsSuccessful = _success,
                RecordsFailed = _fail,
                Detail = GlobalContext.GetEventLog(),
            };

            // update the job to show that it is complete
            _serviceFacade.UpdateIntegrationJob(request, false);
        }

        internal void CompleteJob()
        {
            if (_integrationJobID == null)
                return;

            var request = new UpdateIntegrationJobRequest()
            {
                ID = (long)_integrationJobID,
                RecordsTotal = _totalRecords,
                RecordsTotalUnique = _totalUniqueRecords,
                RecordsSuccessful = _success,
                RecordsFailed = _fail,
                Detail = GlobalContext.GetEventLog(),
                //AttachmentData = new AttachmentData
                //{
                //    AttachmentContent = _fileContent,
                //    AttachmentName = GetFileName(file)
                //}
            };

            // update the job to show that it is complete
            _serviceFacade.UpdateIntegrationJob(request, true);
        }
    }
}