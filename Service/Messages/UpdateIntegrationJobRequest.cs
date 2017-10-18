using ALDataIntegrator.Service.Model;

namespace ALDataIntegrator.Service.Messages
{
    internal class UpdateIntegrationJobRequest
    {
        internal long ID { get; set; }
        internal int RecordsTotal { get; set; }
        internal int RecordsTotalUnique { get; set; }
        internal int RecordsSuccessful { get; set; }
        internal int RecordsFailed { get; set; }
        internal string Detail { get; set; }
        
        // Attachment
        internal AttachmentData AttachmentData { get; set; }
    }
}
