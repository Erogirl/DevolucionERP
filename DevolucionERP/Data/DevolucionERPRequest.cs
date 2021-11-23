using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DevolucionERP.Data
{
    public class DevolucionERPRequest
    {
        [Required] public string orden { get; set; }

    }
}
