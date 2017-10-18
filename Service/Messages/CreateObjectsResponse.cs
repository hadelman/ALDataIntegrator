using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ALDataIntegrator.RightNowService;

namespace ALDataIntegrator.Service.Messages
{
    internal class CreateObjectsResponse : GenericServiceResponse
    {
        internal List<RNObject> CreatedObjects { get; set; }
    }
}
