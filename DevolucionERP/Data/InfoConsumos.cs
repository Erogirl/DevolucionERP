using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DevolucionERP.Data
{
    public class InfoConsumos
    {
         public string codigo { get; set; }
         public string udsConsumidas { get; set; }
         public string totalLote { get; set; }
         public string mermas { get; set; }
         public Boolean esAceite { get; set; }
    }
}
