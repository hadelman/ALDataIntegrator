using System.Collections.Generic;
using System.Linq;
using ALDataIntegrator.RightNowService;
using ALDataIntegrator.Service.Messages;
using ALDataIntegrator.Service.Model;
using System;

namespace ALDataIntegrator.Service
{
    internal class ALRightNowServiceFacade
    {
        private readonly RightNowServiceWrapper _serviceWrapper = new RightNowServiceWrapper();

        #region IntegrationJob Operations

        internal long? CreateIntegrationJob(CreateIntegrationJobRequest request)
        {
            var gfs = new List<GenericField>
            {
                // always default status to in-progress when creating a job
                RightNowServiceBaseObjectBuilder.CreateGenericField("JobStatus", DataTypeEnum.NAMED_ID, RightNowServiceBaseObjectBuilder.CreateNamedID ((int)IntegrationJobStatus.InProgress)),
                RightNowServiceBaseObjectBuilder.CreateGenericField("JobType", DataTypeEnum.NAMED_ID, RightNowServiceBaseObjectBuilder.CreateNamedID ((int)request.JobType)),
                RightNowServiceBaseObjectBuilder.CreateGenericField("FileName", DataTypeEnum.STRING, request.FileName),
                RightNowServiceBaseObjectBuilder.CreateGenericField("HostName", DataTypeEnum.STRING, request.HostName),
            };

            var package = new RNObject[]
            {
                new GenericObject
                {
                    ObjectType = new RNObjectType
                    {
                        Namespace = "IJ",
                        TypeName = "Job"
                    },
                    GenericFields = gfs.ToArray()
                }
            };

            GlobalContext.Log("Creating an Integration Job in RightNow for tracking purposes", true);
            var result = _serviceWrapper.CreateObjects(package);

            if (result.Successful && result.CreatedObjects.Any())
                return result.CreatedObjects.First().ID.id;

            return null;
        }

        internal void UpdateIntegrationJob(UpdateIntegrationJobRequest request, bool updateJobStatus)
        {
            IntegrationJobStatus jobStatus = request.RecordsFailed > 0
                                                 ? IntegrationJobStatus.CompletedWithErrors
                                                 : IntegrationJobStatus.Completed;

            var gfs = new List<GenericField>
            {
                RightNowServiceBaseObjectBuilder.CreateGenericField("CountOfRowsInFile", DataTypeEnum.INTEGER, request.RecordsTotal),
                RightNowServiceBaseObjectBuilder.CreateGenericField("CountOfUniqueRowsInFile", DataTypeEnum.INTEGER, request.RecordsTotalUnique),
                RightNowServiceBaseObjectBuilder.CreateGenericField("CountOfSuccess", DataTypeEnum.INTEGER, request.RecordsSuccessful),
                RightNowServiceBaseObjectBuilder.CreateGenericField("CountOfFailure", DataTypeEnum.INTEGER, request.RecordsFailed)
            };

            if (updateJobStatus)
                gfs.Add(RightNowServiceBaseObjectBuilder.CreateGenericField("JobStatus", DataTypeEnum.NAMED_ID, RightNowServiceBaseObjectBuilder.CreateNamedID((int)jobStatus)));

            if (request.AttachmentData != null)
                gfs.Add(RightNowServiceBaseObjectBuilder.CreateAttachmentText(request.AttachmentData));

            if (!string.IsNullOrEmpty(request.Detail))
                gfs.Add(RightNowServiceBaseObjectBuilder.CreateGenericField("Detail", DataTypeEnum.STRING, request.Detail));

            var package = new RNObject[]
            {
                new GenericObject
                {
                    ObjectType = new RNObjectType
                    {
                        Namespace = "IJ",
                        TypeName = "Job"
                    },
                    GenericFields = gfs.ToArray(),
                    ID = new ID
                    {
                        id = request.ID, 
                        idSpecified = true
                    },

                }
            };

            GlobalContext.Log("Updating the Integration Job with results", true);
            _serviceWrapper.UpdateObjects(package);
        }

        internal void CreateNoFilesToProcessIntegrationJob(IntegrationJobType jobType)
        {
            var gfs = new List<GenericField>
            {
                RightNowServiceBaseObjectBuilder.CreateGenericField("JobStatus", DataTypeEnum.NAMED_ID, RightNowServiceBaseObjectBuilder.CreateNamedID ((int)IntegrationJobStatus.Completed)),
                RightNowServiceBaseObjectBuilder.CreateGenericField("JobType", DataTypeEnum.NAMED_ID, RightNowServiceBaseObjectBuilder.CreateNamedID ((int)jobType)),
                RightNowServiceBaseObjectBuilder.CreateGenericField("Detail", DataTypeEnum.STRING, "No files were available to process")
            };

            var package = new RNObject[]
            {
                new GenericObject
                {
                    ObjectType = new RNObjectType
                    {
                        Namespace = "IJ",
                        TypeName = "Job"
                    },
                    GenericFields = gfs.ToArray(),
                }
            };

            GlobalContext.Log("Creating an Integration Job showing that no files were available to process", true);
            _serviceWrapper.CreateObjects(package);
        }

        #endregion

    }
}
