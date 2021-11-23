using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DevolucionERP.Data;

namespace DevolucionERP.Interfaces
{
        public interface IDevolucionERP
        {
            string DevolucionERPFuncion(DevolucionERPRequest jsonOBJ);

        }
}
