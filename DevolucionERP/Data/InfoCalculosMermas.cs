using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DevolucionERP.Data
{
    public class InfoCalculosMermas
    {
        public string codigo { get; set; }
        public string udsConsumo { get; set; }
         public string mermasConsumo { get; set; }
        public string loteConsumo { get; set; }
    }
}
