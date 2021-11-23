using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DevolucionERP.Data;
using DevolucionERP.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using Microsoft.AspNetCore.Cors;

namespace DevolucionERP.Controllers
{
    [Route("[controller]/[action]")]
    [EnableCors("AllowAll")]
    [ApiController]
    public class DevolucionERPController : ControllerBase
    {
        public IDevolucionERP _devolucionERP { get; set; }

        public DevolucionERPController(IDevolucionERP devolucionERP)
        {
            _devolucionERP = devolucionERP;
        }


        [HttpGet]
        public void PruebaCosa() { Console.WriteLine("¡Funciona!"); }

        [HttpPost]
        public IActionResult DevolucionERPFuncion([FromBody] DevolucionERPRequest jsonOBJ)
        {

            var res = _devolucionERP.DevolucionERPFuncion(jsonOBJ);

            if (res == "error" || res == null || res == "")
            {
                return StatusCode(500, "{Error en la orden: " + jsonOBJ.orden +"}");
            }
            else if (res == "ok")
            {

                return StatusCode(200, "{Generado documento .txt de la orden :" + jsonOBJ.orden + "}");
            }
            else { return StatusCode(500, "{Error en la orden: " + jsonOBJ.orden +  "Error: " + res + "}"); }
            /* var idDisparador = jsonOBJ.idDisparador;
            var xmlDisparador = jsonOBJ.xmlDisparador;
            var codigoTrazabilidad = jsonOBJ.codigoTrazabilidad;*/
        }


    }
}



