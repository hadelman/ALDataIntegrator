using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ALDataIntegrator.Service.Messages
{
    internal class CreateContactResponse : GenericServiceResponse
    {
        internal long? ContactID { get; set; }
    }
}
