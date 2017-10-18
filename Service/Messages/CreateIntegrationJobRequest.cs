using ALDataIntegrator.Service.Model;

namespace ALDataIntegrator.Service.Messages
{
    internal class CreateIntegrationJobRequest
    {
        internal string FileName { get; set; }
        internal string HostName { get; set; }
        internal IntegrationJobType JobType { get; set; }
    }
}
