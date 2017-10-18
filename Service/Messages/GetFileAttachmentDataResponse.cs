using ALDataIntegrator.RightNowService;

namespace ALDataIntegrator.Service.Messages
{
    internal class GetFileAttachmentDataResponse : GenericServiceResponse
    {
        internal byte[] FileData { get; set; }
    }
}
