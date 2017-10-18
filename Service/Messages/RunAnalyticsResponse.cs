using ALDataIntegrator.RightNowService;

namespace ALDataIntegrator.Service.Messages
{
    internal class RunAnalyticsResponse : GenericServiceResponse
    {
        internal CSVTableSet RunAnalyticsTableSet { get; set; }
    }
}
