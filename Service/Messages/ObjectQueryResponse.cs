using ALDataIntegrator.RightNowService;

namespace ALDataIntegrator.Service.Messages
{
    internal class ObjectQueryResponse : GenericServiceResponse
    {
        internal QueryResultData[] QueryObjects { get; set; }
    }
}
