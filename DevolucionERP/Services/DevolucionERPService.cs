using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using DevolucionERP.Data;
using DevolucionERP.Interfaces;
using System.IO;

namespace DevolucionERP.Services
{
    public class DevolucionERPService : IDevolucionERP
    {
        private string _connectionString; 
        public DevolucionERPService(AppSettings settings)
        {
            _connectionString = settings.ConnectionString;
        }
        public string DevolucionERPFuncion(DevolucionERPRequest jsonOBJ)
        {
            System.Net.ServicePointManager.ServerCertificateValidationCallback = (object se, System.Security.Cryptography.X509Certificates.X509Certificate cert, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslerror) => true;
            var udsBuenas = "";
            var palet = "";
            string res = "error";
            string[] error;
            decimal mermasCaja = 0;
            List<InfoMaquinas> infoMaquinas;
            List<InfoConsumos> infoConsumos;
            List<InfoCalculosMermas> infoMermas;
            List<InfoSinCalculosMermas> infoMermasAcPlCaj;
            SaveLog("holi");
            string orden = jsonOBJ.orden;
 

            udsBuenas = ObtenerUds(orden);
            palet = ObtenerPalets(orden);
            infoMaquinas = ObtenerMaquinas(orden);
            infoConsumos = ObtenerConsumos(orden);
            mermasCaja = ObtenerMermasCaja(orden);
            if (mermasCaja == 0) //no hay cajas
            {
                infoMermas = ObtenerMermasConLote(orden);
                infoMermasAcPlCaj = new List<InfoSinCalculosMermas>();
               // infoMermasAcPlCaj.Add(new InfoSinCalculosMermas() { codigo = "", udsConsumo = "", mermasConsumo ="", loteConsumo = "" });


            }
            else { infoMermas = ObtenerMermasSinLote(orden); infoMermasAcPlCaj = ObtenerMermasSinLote2(orden); } //hay cajas



            //---COMPROBACIÓN ERRORES
            error = udsBuenas.Split(':');
            if (error[0] == "error" || udsBuenas == "" || udsBuenas == null) { res = udsBuenas; return res; }
            error = palet.Split(':');
            if (error[0] == "error" || palet == "" || palet == null) { res = palet; return res; }
            if (infoMaquinas.Count > 0) { if (infoMaquinas[0].equipoOperadorMaquina == "error") { res = infoMaquinas[0].tiempoMarchaReal; return res; } }
            if (infoConsumos.Count > 0)
            {
                if (infoConsumos[0].codigo == "error") { res = infoConsumos[0].udsConsumidas; return res; }
            }
            if (infoMermas.Count > 0)
            {
                if (infoMermas[0].udsConsumo == "error") { res = infoMermas[0].mermasConsumo; return res; }
            }
            if (mermasCaja < 0) { return res; }

                res = MontarTxt(orden, udsBuenas, palet, infoMaquinas, infoConsumos, infoMermas, mermasCaja, infoMermasAcPlCaj);
            return res;
        }
        public string ObtenerUds(string orden)
        {
            var uds = "";
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    SqlCommand command = new SqlCommand($@" SELECT [UnidadesProducidasBuenas] FROM [OrdenesDeProduccion] where [OrdenProduccion] = '{orden}';", connection);
                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.HasRows && reader.Read())
                    {
                        uds = reader["UnidadesProducidasBuenas"].ToString();
                    }
                    reader.Close();
                }
            }
            catch (Exception e)
            {
                
                uds = "error: " + e.Message.ToString();
            }
            return uds;
        }
        public string ObtenerPalets(string orden)
        {
            var palets = "";
            try
            {
                // Obtener datos del contenedor
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    SqlCommand command = new SqlCommand($@" SELECT  COUNT(*) FROM [OrdenesDeProduccionEncajado] where [Orden]= '{orden}';", connection);
                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.HasRows && reader.Read())
                    {
                        palets = reader[0].ToString();
                    }
                    reader.Close();
                }
            }
            catch (Exception e)
            {

                palets = "error: " + e.Message.ToString();
            }
            return palets;
        }
        public List<InfoMaquinas> ObtenerMaquinas(string orden)
        {
           
            List<InfoMaquinas> infoMaquinas = new List <InfoMaquinas>();
            try
            {
                // Obtener datos del contenedor
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    //  sum(PHReal), (quitado tiempo paro)
                    SqlCommand command = new SqlCommand($@" SELECT EquipoOperadorMaquina, SUM(mhreal) FROM registroActividadMaquinas where ordenproduccion= '{orden}' group by  EquipoOperadorMaquina;", connection);
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        infoMaquinas.Add(new InfoMaquinas() { equipoOperadorMaquina = reader[0].ToString().Split("-")[1], tiempoMarchaReal = reader[1].ToString() });
                       // infoMaquinas[i].equipoOperadorMaquina = reader.GetString(0);
                        //infoMaquinas[i].tiempoParo = reader.GetString(1);
                        //infoMaquinas[i].tiempoMarchaReal = reader.GetString(2);
                    }
                    reader.Close();
                }
                // Obtener tiempo del contenedor
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    //  sum(PHReal), (quitado tiempo paro)
                    SqlCommand command = new SqlCommand($@" SELECT EquipoOperadorMaquina, SUM(mhreal) FROM registroActividadMaquinas where ordenproduccion= '{orden}' group by  EquipoOperadorMaquina;", connection);
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        infoMaquinas.Add(new InfoMaquinas() { equipoOperadorMaquina = reader[0].ToString().Split("-")[1], tiempoMarchaReal = reader[1].ToString() });
                        // infoMaquinas[i].equipoOperadorMaquina = reader.GetString(0);
                        //infoMaquinas[i].tiempoParo = reader.GetString(1);
                        //infoMaquinas[i].tiempoMarchaReal = reader.GetString(2);
                    }
                    reader.Close();
                }
            }
            catch (Exception e)
            {
                if (infoMaquinas.Count > 0)
                {
                    infoMaquinas[0].equipoOperadorMaquina = "error";
                    infoMaquinas[0].tiempoParo = "error: " + e.Message.ToString();
                    infoMaquinas[0].tiempoMarchaReal = "error: " + e.Message.ToString();
                } else { infoMaquinas.Add(new InfoMaquinas() { equipoOperadorMaquina = "error", tiempoParo = "error: " + e.Message.ToString(), tiempoMarchaReal = "error: " + e.Message.ToString() }); }
               
               
            }
            return infoMaquinas;
        }
        public List<InfoConsumos> ObtenerConsumos(string orden)
        {
            
            List<InfoConsumos> infoConsumos = new List<InfoConsumos>();
            try
            {
                // Obtener datos del contenedor
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    SqlCommand command = new SqlCommand($@" SELECT Codigo, SUM(UDS_Consumidas), Lote, sum(UDS_Merma), Descripcion FROM ordenesdeproduccionconsumos where orden='{orden}' and UDS_Consumidas IS NOT NULL and LoteCerrado = '1' GROUP BY Codigo,Lote, Descripcion;", connection);
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        Boolean aceite = false;
                        if (Convert.ToDecimal(reader[1].ToString()) > 0) {
                            if (reader[4].ToString() == "ACEITE" || reader[4].ToString().Split(' ')[0] == "ACEITE") { aceite = true; } else { aceite = false; }
                            infoConsumos.Add(new InfoConsumos() { codigo = reader[0].ToString(), udsConsumidas = reader[1].ToString(), totalLote = reader[2].ToString(), mermas = reader[3].ToString(), esAceite = aceite });
                        }
                        //infoConsumos[i].codigo = reader.GetString(0);
                        //infoConsumos[i].udsConsumidas = reader.GetString(1);
                        //infoConsumos[i].totalLote = reader.GetString(2);
                        //infoConsumos[i].mermas = reader.GetString(3);
                    }
                    reader.Close();
                }
            }
            catch (Exception e)
            {
                if (infoConsumos.Count > 0)
                {
                    infoConsumos[0].codigo = "error";
                    infoConsumos[0].udsConsumidas = "error: " + e.Message.ToString();
                    infoConsumos[0].totalLote = "error: " + e.Message.ToString();
                    infoConsumos[0].mermas = "error: " + e.Message.ToString();
                }
                else
                {
                    infoConsumos.Add(new InfoConsumos() { codigo = "error", udsConsumidas = e.Message.ToString(), totalLote = e.Message.ToString(), mermas = e.Message.ToString() });
                }



            }
            return infoConsumos;
        }

        public decimal ObtenerMermasCaja(string orden)
        {
            decimal udsConsumidas = 0;
            decimal factor =0;
            decimal res =0;
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    SqlCommand command = new SqlCommand($@" SELECT SUM(O.[UDS_Consumidas]), [dec3] FROM [OrdenesDeProduccionConsumos] O INNER JOIN [OrdenesDeProduccion] P ON P.[OrdenProduccion] = O.[orden] where O.[orden] = '{orden}' AND O.[Unidad_expresada_en] = 'CJ' GROUP BY [dec3];", connection);
                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.HasRows && reader.Read())
                    {
                        udsConsumidas =  Convert.ToDecimal(reader[0].ToString());
                        factor = Convert.ToDecimal(reader[1].ToString());
                    }
                    reader.Close();
                }
            }
            catch (Exception e)
            {

                res = -1;
            }
            if (res > -1) { res = udsConsumidas * factor; }
           
            return res;
        }

        public List<InfoCalculosMermas> ObtenerMermasConLote(string orden)
        {
          
            List<InfoCalculosMermas> infoMermas = new List<InfoCalculosMermas>();
            try
            {
                // Obtener datos del contenedor
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    SqlCommand command = new SqlCommand($@" SELECT Codigo, SUM(UDS_Consumidas), sum(UDS_Merma), Lote FROM ordenesdeproduccionconsumos where orden='{orden}' and UDS_Consumidas IS NOT NULL and LoteCerrado = '1' GROUP BY Codigo,Lote;", connection);
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        if (Convert.ToDecimal(reader[1].ToString()) > 0)
                        {
                            infoMermas.Add(new InfoCalculosMermas() { codigo = reader[0].ToString(), udsConsumo = reader[1].ToString(), mermasConsumo = reader[2].ToString(), loteConsumo = reader[3].ToString()});

                        }


                    }
                    reader.Close();
                }
            }
            catch (Exception e)
            {
                if (infoMermas.Count > 0)
                {
                    infoMermas[0].codigo = "error";
                    infoMermas[0].udsConsumo = "error";
                    infoMermas[0].mermasConsumo = "error: " + e.Message.ToString();
                    infoMermas[0].loteConsumo = "error: " + e.Message.ToString();
                }
                else
                {
                    infoMermas.Add(new InfoCalculosMermas() { codigo = "error",  udsConsumo = "error", mermasConsumo = e.Message.ToString(), loteConsumo = e.Message.ToString() });
                }



            }
            return infoMermas;
        }

        public List<InfoCalculosMermas> ObtenerMermasSinLote(string orden)
        {
            var i = 0;
            List<InfoCalculosMermas> infoMermas = new List<InfoCalculosMermas>();
            try
            {
                // Obtener datos del contenedor
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    SqlCommand command = new SqlCommand($@" Select Codigo, SUM(UDS_Consumidas), SUM(UDS_Merma) FROM [OrdenesDeProduccionConsumos] where orden = '{orden}'AND [ReferenciaFiltro] IS NOT NULL AND  [ReferenciaFiltro] <> 'PL' AND [ReferenciaFiltro] <> 'CJ' and UDS_Consumidas IS NOT NULL and LoteCerrado = '1'  group by Unidad_expresada_en, Codigo ;", connection);
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        if (Convert.ToDecimal(reader[1].ToString()) > 0)
                        {
                            infoMermas.Add(new InfoCalculosMermas() { codigo = reader[0].ToString(), udsConsumo = reader[1].ToString(), mermasConsumo = reader[2].ToString() });
                        }
                    }
                    reader.Close();
                }
            }
            catch (Exception e)
            {
                if (infoMermas.Count > 0)
                {
                    infoMermas[0].codigo = "error";
                    infoMermas[0].udsConsumo = "error";
                    infoMermas[0].mermasConsumo = "error: " + e.Message.ToString();
                    infoMermas[0].loteConsumo = "error: " + e.Message.ToString();
                }
                else
                {
                    infoMermas.Add(new InfoCalculosMermas() { codigo = "error", udsConsumo = "error", mermasConsumo = e.Message.ToString(), loteConsumo = e.Message.ToString() });
                }



            }
            return infoMermas;
        }

        public List<InfoSinCalculosMermas> ObtenerMermasSinLote2(string orden)
        {
            var i = 0;
            List<InfoSinCalculosMermas> infoMermas = new List<InfoSinCalculosMermas>();
            try
            {
                // Obtener datos del contenedor
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    SqlCommand command = new SqlCommand($@" Select Codigo, SUM(UDS_Consumidas), SUM(UDS_Merma), Lote FROM [OrdenesDeProduccionConsumos] where orden = '{orden}' AND ([ReferenciaFiltro] is NULL or  [ReferenciaFiltro] = 'PL' OR [ReferenciaFiltro] = 'CJ'  ) and UDS_Consumidas IS NOT NULL and LoteCerrado = '1'  group by Unidad_expresada_en, Codigo, Lote ;", connection);
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        if (Convert.ToDecimal(reader[1].ToString()) > 0)
                        {
                            infoMermas.Add(new InfoSinCalculosMermas() { codigo = reader[0].ToString(), udsConsumo = reader[1].ToString(), mermasConsumo = reader[2].ToString(), loteConsumo = reader[3].ToString() });
                        }
                    }
                    reader.Close();
                }
            }
            catch (Exception e)
            {
                if (infoMermas.Count > 0)
                {
                    infoMermas[0].codigo = "error";
                    infoMermas[0].udsConsumo = "error";
                    infoMermas[0].mermasConsumo = "error: " + e.Message.ToString();
                    infoMermas[0].loteConsumo = "error: " + e.Message.ToString();
                }
                else
                {
                    infoMermas.Add(new InfoSinCalculosMermas() { codigo = "error", udsConsumo = "error", mermasConsumo = e.Message.ToString(), loteConsumo = e.Message.ToString() });
                }



            }
            return infoMermas;
        }


        public string MontarTxt(string orden, string udsBuenas, string palet, List<InfoMaquinas> infoMaquinas, List<InfoConsumos> infoConsumos, List<InfoCalculosMermas> infoMermas, decimal mermasCaja, List<InfoSinCalculosMermas> infoMermasAcPlCaj)
        {
            
            
            string res = orden + ";";// + udsBuenas + ";" + palet + ";";
            int contadorAceite = 0;
            int contadorAceite1 = 0;
            int contadorAceite2 = 0;
            decimal totalAceite = 0;
            decimal udsTotales = 0;
            decimal palets = Convert.ToDecimal(palet);

            int contadorMaq = 0;
            int contadorCons = 0;
            int contadorMer = 0;
            int contadorMer1 = 0;
            decimal tiempos;
            decimal totalMermas = 0;
            
            string[] fecha = DateTime.Now.ToString("MM/dd/yyyy").Split('/');
            string nombre = fecha[2] + fecha[1] + fecha[0] + "_" + orden;
            string path = @"C:\SALIDA\" + nombre+".txt";
            try
            {
                //TOTAL Uds
                foreach (InfoConsumos e in infoConsumos)
                {
                    if (infoConsumos[contadorAceite].esAceite) { totalAceite = totalAceite + Convert.ToDecimal(infoConsumos[contadorAceite].udsConsumidas); }
                    udsTotales = udsTotales + Convert.ToDecimal(infoConsumos[contadorAceite].udsConsumidas);
                    contadorAceite++;
                }

                foreach (InfoConsumos e in infoConsumos)
                {

                    if (infoConsumos[contadorAceite1].esAceite)
                    {
                        if (contadorAceite2 > 0) { res += "\t"; }
                        decimal udsParte = udsTotales * Convert.ToDecimal(infoConsumos[contadorAceite1].udsConsumidas) / totalAceite;
                        decimal paletsParte = Convert.ToDecimal(infoConsumos[contadorAceite1].udsConsumidas) * palets / totalAceite;
                        res += infoConsumos[contadorAceite1].totalLote + "|" + Decimal.Round(udsParte, 3).ToString().Replace(',', '.') + "|" + Math.Round(paletsParte, 3).ToString().Replace(',', '.');
                        contadorAceite2++;
                    }

                    contadorAceite1++;
                }
                res += ";";
                foreach (InfoMaquinas e in infoMaquinas)
                {

                   
                        if (contadorMaq > 0) { res += "\t"; }
                       // tiempos = Convert.ToDecimal(e.tiempoMarchaReal) + Convert.ToDecimal(e.tiempoParo);
                        res += e.equipoOperadorMaquina.Trim() + "|" + Math.Round(Convert.ToDecimal(e.tiempoMarchaReal), 3).ToString().Replace(',', '.');
                        contadorMaq++;
                     
                }
                res += ";";
                foreach (InfoConsumos e in infoConsumos)
                {
                   // if (contadorCons > 0) { res += "\t"; }
                    res += e.codigo + "|" + e.totalLote + "|" + Math.Round(Convert.ToDecimal(e.udsConsumidas), 3).ToString().Replace(',', '.');
                    contadorCons++;
                    if (infoConsumos.Count > contadorCons) { res += "\t"; }
                }
                res += ";";

                //Parte mermas
                if (mermasCaja == 0) {

                    foreach (InfoCalculosMermas e in infoMermas)
                    {
                        //totalMermas = Convert.ToDecimal(infoMermas[contadorMer].udsConsumo) - mermasCaja + Convert.ToDecimal(infoMermas[contadorMer].mermasConsumo);
                        if (contadorMer > 0) { res += "\t"; }
                        res += e.codigo + "|" + e.loteConsumo + "|" + Math.Round(Convert.ToDecimal(e.mermasConsumo), 3).ToString().Replace(',', '.');
                        SaveLog("1 \n");
                        SaveLog(res + "\n");
                        contadorMer++;
                    }
                } else {

                    //aceites, cajas, palets
                    foreach (InfoSinCalculosMermas e in infoMermasAcPlCaj)
                    {
                       // totalMermas = Convert.ToDecimal(e.udsConsumo) - mermasCaja + Convert.ToDecimal(infoMermas[contadorMer].mermasConsumo);
                        if (contadorMer1 > 0) { res += "\t"; }
                        res += e.codigo + "|" + e.loteConsumo + "|" + Math.Round(Convert.ToDecimal(e.mermasConsumo), 3).ToString().Replace(',', '.');
                        SaveLog("2 \n");
                        SaveLog(res + "\n");
                        contadorMer1++;
                    }
                    res += "\t";
                    //resto
                    foreach (InfoCalculosMermas e in infoMermas)
                    {
                        totalMermas = 0;
                        totalMermas = Convert.ToDecimal(e.udsConsumo) - mermasCaja + Convert.ToDecimal(e.mermasConsumo);
                        if (totalMermas < 0) { SaveLog("e.udsConsumo - " + e.udsConsumo + " mermasCaja + "+ mermasCaja + " mermasConsumo "+e.mermasConsumo+ "\n"); }
                        if (contadorMer > 0) { res += "\t"; }
                        res += e.codigo + "|"  + Math.Round(totalMermas, 3).ToString().Replace(',', '.');
                        SaveLog("3 \n");
                        SaveLog(res + "\n");
                        contadorMer++;
                    }
                }
                
                
                File.WriteAllText(path, res);
            }
            catch (Exception e) { return e.Message.ToString(); }
            string res2 = "ok";
            return res2;
        }
        public static void SaveLog(string data)
        {
            // Set a variable to the Documents path.
            using (StreamWriter writer = new StreamWriter(@"C:\DOEET\DevolucionDebug\LOG.txt", true)) //// true to append data to the file
            {
                writer.WriteLine(data);
            }
        }

    }
}
