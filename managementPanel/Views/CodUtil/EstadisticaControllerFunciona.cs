using managementPanel.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace managementPanel.Controllers
{
    public class EstadisticaControllerFunciona : Controller
    {
        DB_ManagementEntities contexto = new DB_ManagementEntities();
        public ActionResult VerEstadisticas(string usuarioId)
        {
            List<Proyectos> misProyectos = new List<Proyectos>();
            List<Usuarios> usuarios = new List<Usuarios>();

            foreach (Proyectos proy in contexto.Proyectos.ToList())
            {
                if (((Usuarios)Session["usuario"]).Usuario.Equals("admin"))
                {
                    usuarios = contexto.Usuarios.ToList();
                }
                else if ((!proy.IdUsuario.ToString().Equals("null") && proy.IdUsuario.ToString().Equals(((Usuarios)Session["usuario"]).IdUsuario.ToString())))
                {
                    misProyectos.Add(proy);
                }

                if ((!proy.IdUsuario.ToString().Equals("null") && usuarioId != null))
                {
                    ViewBag.UsuSelect = int.Parse(usuarioId);

                    if (proy.IdUsuario.ToString().Equals(usuarioId))
                    {
                        misProyectos.Add(proy);
                    }
                }
            }

            object[] modelos = new object[2];
            modelos[0] = misProyectos;
            modelos[1] = usuarios;

            return View(modelos);
        }

        public JsonResult VerFacturacion(string usu, string fechaInicio, string fechaFin)
        {
            Usuarios usuSelect = new Usuarios();
            string[] meses = { "Enero","Febrero","Marzo","Abril","Mayo","Junio","Julio","Agosto","Septiembre","Octubre","Noviembre","Diciembre" };
            Dictionary<string, int> fechaHoras = new Dictionary<string, int>();

            if (usu != null)
            {
                int usuId = int.Parse(usu);
                usuSelect = contexto.Usuarios.FirstOrDefault(x => x.IdUsuario == usuId);
            }

            int fechaInicioForm = int.Parse(fechaInicio.Replace("-", ""));
            int fechaFinForm = int.Parse(fechaFin.Replace("-", ""));

            foreach (Proyectos p in contexto.Proyectos)
            {
                if (p.JefeProyecto != null)
                {
                    if (p.JefeProyecto.Equals(usuSelect.Usuario) && p.Fase.Equals("Ended"))
                    {
                        int fechaInicioP= int.Parse(Convert.ToDateTime(p.FechaInicio).ToString("yyyyMMdd"));
                        int fechaFinP = int.Parse(Convert.ToDateTime(p.FechaFin).ToString("yyyyMMdd"));

                        if ( (fechaInicioP >= fechaInicioForm && fechaInicioP<=fechaFinForm) && (fechaFinP<=fechaFinForm && fechaFinP >= fechaInicioForm))
                        {
                            int anioFinalizacion = int.Parse(fechaFinP.ToString().Substring(0,4));
                            int mesFinalizacion = int.Parse(fechaFinP.ToString().Substring(4,2));

                            int horasTotales = Convert.ToInt32(p.HorasNormales + p.HorasEspeciales);

                            string claveFecha = meses[mesFinalizacion - 1] + " del " + anioFinalizacion;

                            if (!fechaHoras.ContainsKey(claveFecha))
                            {
                                fechaHoras.Add(claveFecha, horasTotales);
                            }
                            else
                            {
                                int valorAnterior = fechaHoras[claveFecha];
                                fechaHoras[claveFecha]= valorAnterior + horasTotales;
                            }
                        }
                    }
                }
            }

            /*object[] modelos = new object[fechaHoras.Count];
            int i = 0;
            foreach (KeyValuePair<string, int> v in fechaHoras)
            {
                var modelo = new[] { v.Key, v.Value.ToString() };
                modelos[i] = modelo;
                i++;
            }
            return Json(modelos, JsonRequestBehavior.AllowGet);*/

            return Json(fechaHoras, JsonRequestBehavior.AllowGet);
        }
    }
}
