using ALDataIntegrator.RightNowService;

namespace ALDataIntegrator.Service.Model
{
    internal class RightNowServiceGenericFieldDTO
    {
        internal string CustomFieldName { get; set; }
        internal string PackageName { get; set; }
        internal DataTypeEnum DataType { get; set; }
        internal object DataValue { get; set; }
    }
}
