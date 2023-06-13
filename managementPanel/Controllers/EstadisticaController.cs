using managementPanel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;


namespace managementPanel.Controllers
{
    public class EstadisticaController : Controller
    {
        DB_ManagementEntities contexto = new DB_ManagementEntities();

        [NoDirectAccess]
        public ActionResult VerEstadisticas()
        {
            List<Usuarios> misUsuarios = new List<Usuarios>();

            List<Proyectos> misProyectos = new List<Proyectos>();
            List<FacturacionPorMes> facturacionPorMes = new List<FacturacionPorMes>();
            List<FacturacionPorMes> facturacionRPorMes = new List<FacturacionPorMes>();
            List<object> model = new List<object>();

            decimal facturacionT = 0;
            decimal facturacionTacabados = 0;

            DateTime fechaDesde = new DateTime(DateTime.Now.Year, 1, 1);
            DateTime fechaHoy = DateTime.Now;
            DateTime fechaHasta = new DateTime(DateTime.Now.Year, 12, 31);

            if (((Usuarios)Session["usuario"]).Consultor == true)
            {
                string consultor = ((Usuarios)Session["usuario"]).Usuario;
                foreach (Proyectos p in contexto.Proyectos)
                {
                    if (p.Consultor == consultor && p.FechaFin <= fechaHoy && p.FechaFin >= fechaDesde)
                    {
                        misProyectos.Add(p);
                    }
                }
            }

            int idBuscar = ((Usuarios)Session["usuario"]).IdUsuario;
            Usuarios sesion = contexto.Usuarios.FirstOrDefault(x => x.IdUsuario == idBuscar);
            if (((Usuarios)Session["usuario"]).Usuario.Equals("admin"))
            {
                misUsuarios = contexto.Usuarios.ToList();
            }
            else if (((Usuarios)Session["usuario"]).Admin == true && !((Usuarios)Session["usuario"]).Usuario.Equals("admin"))
            {
                foreach (Equipos e in sesion.Equipos)
                {
                    foreach (Usuarios u in e.Usuarios)
                    {
                        if (!misUsuarios.Contains(u))
                        {
                            misUsuarios.Add(u);
                        }
                    }
                }
            }
            else
            {
                foreach (Proyectos p in contexto.Proyectos)
                {
                    if (p.IdUsuario == idBuscar)
                    {
                        misProyectos.Add(p);
                    }
                }

                facturacionPorMes = misProyectos
                .Where(p => p.Fase.Equals("Ended") && p.FechaFin != null && p.FechaFin >= fechaDesde && p.FechaFin.Value <= fechaHoy)
                .Select(p => new { Fecha = new DateTime(p.FechaFin.Value.Year, p.FechaFin.Value.Month, 1), HorasFacturadas = p.HorasNormales + p.HorasEspeciales })
                .GroupBy(p => p.Fecha)
                .Select(g => new FacturacionPorMes { Mes = g.Key.Month, Año = g.Key.Year, HorasFacturadas = g.Sum(p => p.HorasFacturadas) })
                .OrderBy(g => g.Año)
                .ThenBy(g => g.Mes)
                .ToList();

                facturacionTacabados = misProyectos
                .Where(p => p.Fase.Equals("Ended") && p.FechaFin != null && p.FechaFin >= fechaDesde && p.FechaFin.Value <= fechaHoy)
                .Select(p => p.HorasNormales + p.HorasEspeciales)
                .Sum();

                facturacionRPorMes = misProyectos
                .Where(p => p.Fase != "Ended" && p.FechaFin != null && p.FechaFin >= fechaHoy && p.FechaFin.Value <= fechaHasta)
                .Select(p => new { Fecha = new DateTime(p.FechaFin.Value.Year, p.FechaFin.Value.Month, 1), HorasFacturadas = p.HorasNormales + p.HorasEspeciales })
                .GroupBy(p => p.Fecha)
                .Select(g => new FacturacionPorMes { Mes = g.Key.Month, Año = g.Key.Year, HorasFacturadas = g.Sum(p => p.HorasFacturadas) })
                .OrderBy(g => g.Año)
                .ThenBy(g => g.Mes)
                .ToList();

                facturacionT = misProyectos
                .Where(p => p.Fase != "Ended" && p.FechaFin != null && p.FechaFin >= fechaHoy && p.FechaFin.Value <= fechaHasta)
                .Select(p => p.HorasNormales + p.HorasEspeciales)
                .Sum();
            }


            List<Proyectos> proyectosFiltrados = new List<Proyectos>();

            foreach (Proyectos p in misProyectos)
            {
                if (p.FechaFin <= fechaDesde && p.FechaFin >= fechaHasta)
                {
                    proyectosFiltrados.Add(p);
                }
            }

            ViewBag.desde = fechaDesde;
            ViewBag.hoy = fechaHoy;
            ViewBag.hasta = fechaHasta;

            model.Add(proyectosFiltrados);
            model.Add(misUsuarios);
            model.Add(facturacionPorMes);
            model.Add(facturacionRPorMes);
            model.Add(facturacionTacabados);
            model.Add(facturacionT);
            return View(model);
        }

        [NoDirectAccess]
        public ActionResult VerEstadisticasf(int? usuarioId, DateTime? fechaDesde, DateTime? fechaHoy, DateTime? fechaHasta, int? responseData)
        {
            List<Proyectos> misProyectos = new List<Proyectos>();
            List<Proyectos> misProyectosUsuario = new List<Proyectos>();
            List<Usuarios> misUsuarios = new List<Usuarios>();

            if (responseData == 1)
            {
                var datos = new
                {
                    UsuarioId = usuarioId,
                    FechaDesde = fechaDesde.ToString(),
                    FechaHoy = fechaHoy.ToString(),
                    FechaHasta = fechaHasta.ToString(),
                };

                return Json(datos, JsonRequestBehavior.AllowGet);

            }

            if (!fechaDesde.HasValue)
            {
                fechaDesde = new DateTime(DateTime.Now.Year, 1, 1);
            }

            if (!fechaHoy.HasValue)
            {
                fechaHoy = DateTime.Now;
            }

            if (!fechaHasta.HasValue)
            {
                fechaHasta = new DateTime(DateTime.Now.Year, 12, 31);
            }

            // Filtrar los proyectos por fecha
            misProyectos = contexto.Proyectos.ToList();

            int idBuscar = usuarioId ?? ((Usuarios)Session["usuario"]).IdUsuario;

            if (((Usuarios)Session["usuario"]).Consultor == true)
            {
                misProyectosUsuario = misProyectos.Where(p => p.Consultor == ((Usuarios)Session["usuario"]).Usuario).ToList();
            }

            Usuarios sesion = contexto.Usuarios.FirstOrDefault(x => x.IdUsuario == idBuscar);
            if (((Usuarios)Session["usuario"]).Usuario.Equals("admin"))
            {
                misUsuarios = contexto.Usuarios.ToList();
                misProyectos = misProyectos.Where(p => p.Consultor == ((Usuarios)Session["usuario"]).Usuario).ToList();
            }
            else if (((Usuarios)Session["usuario"]).Admin == true && !((Usuarios)Session["usuario"]).Usuario.Equals("admin"))
            {
                foreach (Equipos e in sesion.Equipos)
                {
                    foreach (Usuarios u in e.Usuarios)
                    {
                        if (!misUsuarios.Contains(u))
                        {
                            misUsuarios.Add(u);
                        }
                    }
                }
            }

            foreach (Proyectos p in contexto.Proyectos)
            {
                if (p.IdUsuario == idBuscar)
                {
                    misProyectosUsuario.Add(p);
                }
            }

            var facturacionPorMes = misProyectosUsuario
                .Where(p => p.Fase.Equals("Ended") && p.FechaFin != null && p.FechaFin >= fechaDesde && p.FechaFin.Value <= fechaHoy)
                .Select(p => new { Fecha = new DateTime(p.FechaFin.Value.Year, p.FechaFin.Value.Month, 1), HorasFacturadas = p.HorasNormales + p.HorasEspeciales })
                .GroupBy(p => p.Fecha)
                .Select(g => new FacturacionPorMes { Mes = g.Key.Month, Año = g.Key.Year, HorasFacturadas = g.Sum(p => p.HorasFacturadas) })
                .OrderBy(g => g.Año)
                .ThenBy(g => g.Mes)
                .ToList();

            decimal facturacionTacabados = misProyectosUsuario
                .Where(p => p.Fase.Equals("Ended") && p.FechaFin != null && p.FechaFin >= fechaDesde && p.FechaFin.Value <= fechaHoy)
                .Select(p => p.HorasNormales + p.HorasEspeciales)
                .Sum();

            var facturacionRPorMes = misProyectosUsuario
                .Where(p => p.Fase != "Ended" && p.FechaFin != null && p.FechaFin >= fechaHoy && p.FechaFin.Value <= fechaHasta)
                .Select(p => new { Fecha = new DateTime(p.FechaFin.Value.Year, p.FechaFin.Value.Month, 1), HorasFacturadas = p.HorasNormales + p.HorasEspeciales })
                .GroupBy(p => p.Fecha)
                .Select(g => new FacturacionPorMes { Mes = g.Key.Month, Año = g.Key.Year, HorasFacturadas = g.Sum(p => p.HorasFacturadas) })
                .OrderBy(g => g.Año)
                .ThenBy(g => g.Mes)
                .ToList();

            decimal facturacionT = misProyectosUsuario
                .Where(p => p.Fase != "Ended" && p.FechaFin != null && p.FechaFin >= fechaHoy && p.FechaFin.Value <= fechaHasta)
                .Select(p => p.HorasNormales + p.HorasEspeciales)
                .Sum();

            List<Proyectos> proyectosFiltrados = new List<Proyectos>();

            foreach (Proyectos p in misProyectosUsuario)
            {
                if (p.FechaFin >= fechaDesde && p.FechaFin <= fechaHasta)
                {
                    proyectosFiltrados.Add(p);
                }
            }

            ViewBag.desde = fechaDesde;
            ViewBag.hasta = fechaHasta;
            ViewBag.id = usuarioId;

            object[] modelos = new object[6];
            modelos[0] = proyectosFiltrados;
            modelos[1] = misUsuarios;
            modelos[2] = facturacionPorMes;
            modelos[3] = facturacionRPorMes;
            modelos[4] = facturacionTacabados;
            modelos[5] = facturacionT;

            if (responseData == 2)
            {

                return View("VerEstadisticas", modelos);
            }
            return View(modelos);
        }
        [NoDirectAccess]
        public ActionResult VerEstadisticasGrupo()
        {
            List<Equipos> misEquipos = new List<Equipos>();
            List<Proyectos> misProyectos = new List<Proyectos>();
            List<FacturacionPorMes> facturacionPorMes = new List<FacturacionPorMes>();
            List<FacturacionPorMes> misProyectosAcabados = new List<FacturacionPorMes>();
            List<Proyectos> misProyectosTotales = new List<Proyectos>();
            decimal facturacionT = 0;
            decimal facturacionTacabados = 0;

            List<object> model = new List<object>();

            if (((Usuarios)Session["usuario"]).Usuario.Equals("admin"))
            {
                foreach (Equipos e in contexto.Equipos)
                {
                    misEquipos.Add(e);
                }
            }
            else
            {
                foreach (Equipos e in ((Usuarios)Session["usuario"]).Equipos)
                {
                    misEquipos.Add(e);
                }
            }



            DateTime fechaHoy = DateTime.Now;

            DateTime fechaDesde = new DateTime(DateTime.Now.Year, 1, 1);//endedM
            DateTime fechaHasta = new DateTime(DateTime.Now.Year, 12, 31);//noEnded
            ViewBag.desde = fechaDesde;
            ViewBag.hasta = fechaHasta;


            model.Add(misEquipos);
            model.Add(facturacionPorMes);
            model.Add(facturacionT);
            model.Add(misProyectosAcabados);
            model.Add(facturacionTacabados);
            model.Add(misProyectosTotales);
            return View(model);
        }

        [HttpPost]
        [NoDirectAccess]
        public ActionResult VerEstadisticasGrupo(int? equipoId, DateTime? fechaDesde, DateTime? fechaHasta)
        {
            List<Equipos> misEquipos = new List<Equipos>();
            List<Proyectos> misProyectos = new List<Proyectos>();
            List<Proyectos> misProyectosAcabados = new List<Proyectos>();

            if (!fechaDesde.HasValue)
            {
                fechaDesde = new DateTime(DateTime.Now.Year, 1, 1);//ended
            }

            if (!fechaHasta.HasValue)
            {
                fechaHasta = new DateTime(DateTime.Now.Year, 12, 31);//noEnded
            }

            if (((Usuarios)Session["usuario"]).Usuario.Equals("admin"))
            {
                foreach (Equipos e in contexto.Equipos)
                {
                    misEquipos.Add(e);
                }
            }
            else
            {
                foreach (Equipos e in ((Usuarios)Session["usuario"]).Equipos)
                {
                    misEquipos.Add(e);
                }
            }

            var usuariosDelEquipo = contexto.Usuarios.Where(u => u.Equipos.Any(e => e.IdEquipo == equipoId)).ToList();

            foreach (var usuario in usuariosDelEquipo)
            {
                var proyectosDelUsuario = usuario.Proyectos
                    .Where(p => p.Fase != "Ended" && p.JefeProyecto == usuario.Usuario)
                    .ToList();

                misProyectos.AddRange(proyectosDelUsuario);
            }

            //proyectos acabados
            foreach (var usuario in usuariosDelEquipo)
            {
                var proyectosAcabadosUsu = usuario.Proyectos
                    .Where(p => p.Fase != null && p.Fase.Equals("Ended") && p.JefeProyecto == usuario.Usuario)
                    .ToList();

                misProyectosAcabados.AddRange(proyectosAcabadosUsu);
            }

            List<Proyectos> misProyectosTotales = misProyectos.Concat(misProyectosAcabados).ToList();

            var misProyectosTotalesF = misProyectosTotales
                .Where(p => p.FechaFin != null && p.FechaFin.Value >= fechaDesde && p.FechaFin.Value <= fechaHasta)
                .ToList();

            var facturacionRPorMes = misProyectos
                .Where(p => p.Fase != "Ended" && p.FechaFin != null && p.FechaFin.Value >= DateTime.Now && p.FechaFin.Value <= fechaHasta)
                .Select(p => new { Fecha = new DateTime(p.FechaFin.Value.Year, p.FechaFin.Value.Month, 1), HorasFacturadas = p.HorasNormales + p.HorasEspeciales })
                .GroupBy(p => p.Fecha)
                .Select(g => new FacturacionPorMes { Mes = g.Key.Month, Año = g.Key.Year, HorasFacturadas = g.Sum(p => p.HorasFacturadas) })
                .OrderBy(g => g.Año)
                .ThenBy(g => g.Mes)
                .ToList();

            decimal facturacionT = misProyectos
                .Where(p => p.Fase != "Ended" && p.FechaFin != null && p.FechaFin.Value >= DateTime.Now && p.FechaFin.Value <= fechaHasta)
                .Select(p => p.HorasNormales + p.HorasEspeciales)
                .Sum();


            //Facturación total de los proyectos acabados
            var facturacionPorMesAcabados = misProyectosAcabados
                .Where(p => p.Fase != null && p.Fase.Equals("Ended") && p.FechaFin != null && p.FechaFin.Value >= fechaDesde && p.FechaFin.Value <= DateTime.Now)
                .Select(p => new { Fecha = new DateTime(p.FechaFin.Value.Year, p.FechaFin.Value.Month, 1), HorasFacturadas = p.HorasNormales + p.HorasEspeciales })
                .GroupBy(p => p.Fecha)
                .Select(g => new FacturacionPorMes { Mes = g.Key.Month, Año = g.Key.Year, HorasFacturadas = g.Sum(p => p.HorasFacturadas) })
                .OrderBy(g => g.Año)
                .ThenBy(g => g.Mes)
                .ToList();

            decimal facturacionTacabados = misProyectosAcabados
                .Where(p => p.Fase != null && p.Fase.Equals("Ended") && p.FechaFin != null && p.FechaFin.Value >= fechaDesde && p.FechaFin.Value <= DateTime.Now)
                .Select(p => p.HorasNormales + p.HorasEspeciales)
                .Sum();

            ViewBag.desde = fechaDesde;
            ViewBag.hasta = fechaHasta;
            ViewBag.id = equipoId;

            object[] modelos = new object[6];
            modelos[0] = misEquipos;
            modelos[1] = facturacionRPorMes;
            modelos[2] = facturacionT;
            modelos[3] = facturacionPorMesAcabados;
            modelos[4] = facturacionTacabados;
            modelos[5] = misProyectosTotalesF;
            return View(modelos);
        }
    }
}