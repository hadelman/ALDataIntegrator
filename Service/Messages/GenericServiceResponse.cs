using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ALDataIntegrator.Service.Messages
{
    internal class GenericServiceResponse
    {
        internal bool Successful { get; set; }
        internal bool SuccessfulSet { get; set; }

        internal string Details { get; set; }
    }
}
