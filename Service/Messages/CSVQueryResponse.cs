using ALDataIntegrator.RightNowService;

namespace ALDataIntegrator.Service.Messages
{
    internal class CSVQueryResponse : GenericServiceResponse
    {
        internal CSVTableSet TableSet { get; set; }
    }
}
