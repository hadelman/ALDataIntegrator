using ALDataIntegrator.RightNowService;

namespace ALDataIntegrator.Service.Messages
{
    internal class ServiceBatchResponse : GenericServiceResponse
    {
        internal BatchResponseItem[] BatchResponseItems { get; set; }
    }
}
