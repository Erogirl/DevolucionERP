using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DevolucionERP.Data
{
    public class InfoMaquinas
    {
         public string equipoOperadorMaquina { get; set; }
         public string tiempoParo { get; set; }
         public string tiempoMarchaReal { get; set; }
    }
}
