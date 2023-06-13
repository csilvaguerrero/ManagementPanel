using managementPanel.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using PagedList;
using System.Text;

namespace managementPanel.Controllers
{
    public class ProyectoController : Controller
    {
        DB_ManagementEntities contexto = new DB_ManagementEntities();

        [NoDirectAccess]
        public ActionResult CrearProyecto()
        {
            List<Usuarios> usuarios = contexto.Usuarios.ToList();
            Proyectos proyectos = new Proyectos();

            object[] modelos = new object[2];

            modelos[0] = usuarios;
            modelos[1] = proyectos;

            return View(modelos);
        }


        [HttpPost]
        [NoDirectAccess]
        public ActionResult CrearProyecto(Proyectos proyecto, string IdUsuario)
        {
            if (!IdUsuario.Equals("null"))
            {
                Usuarios u = contexto.Usuarios.Find(Int32.Parse(IdUsuario));
                proyecto.JefeProyecto = u.Usuario;
                proyecto.IdUsuario = Convert.ToInt16(IdUsuario);
            }

            proyecto.Desviacion = CalcularDesviacion(proyecto.HorasEspeciales, proyecto.HorasNormales, proyecto.HorasReales);

            contexto.Proyectos.Add(proyecto);
            contexto.SaveChanges();

            return RedirectToAction("ListarProyectos", "Proyecto");
        }
        [NoDirectAccess]
        public ActionResult verProyecto(string codigo, int? page, int? size, string projectS, string campoOrden, string buscar)
        {
            Proyectos proyecto = contexto.Proyectos.FirstOrDefault(x => x.Codigo == codigo);

            proyecto.HorasDesarrollo = proyecto.HorasReales - proyecto.HorasAnalisis;

            proyecto.Desviacion = CalcularDesviacion(proyecto.HorasEspeciales, proyecto.HorasNormales, proyecto.HorasReales);

            proyecto.DesviacionAnalisis = CalcularDesviacionAnalisis(proyecto.HorasEspeciales, proyecto.HorasNormales, proyecto.HorasAnalisis);

            proyecto.DesviacionDesarrollo = CalcularDesviacionDesarrollo(proyecto.HorasEspeciales, proyecto.HorasNormales, proyecto.HorasAnalisis, proyecto.HorasReales);

            ViewBag.pageNumber = page;
            ViewBag.pageSize = size;
            ViewBag.projectStatus = projectS;
            ViewBag.campo = campoOrden;
            ViewBag.buscarC = buscar;

            return View("VerProyecto", proyecto);
        }
        [NoDirectAccess]
        public ActionResult EditarProyecto(string codigo, int? page, int? size, string projectS, string campoOrden, string buscar)
        {
            List<Usuarios> usuarios = contexto.Usuarios.ToList();
            Proyectos modificarProyecto = contexto.Proyectos.FirstOrDefault(x => x.Codigo.Equals(codigo));

            object[] modelos = new object[7];

            modelos[0] = usuarios;
            modelos[1] = modificarProyecto;
            modelos[2] = page;
            modelos[3] = size;
            modelos[4] = projectS;
            modelos[5] = campoOrden;
            modelos[6] = buscar;
            return View(modelos);
        }

        [HttpPost]
        [NoDirectAccess]
        public ActionResult EditarProyecto(Proyectos proyecto, string IdUsuario, string Consultor, int? page, int? size, string projectS, string campoOrden, string buscar)
        {
            if (proyecto.Cliente == null)
            {
                TempData["msjEditar"] = "ERROR - Proyecto no editado";
                return RedirectToAction("ListarProyectos", "Proyecto");
            }
            else
            {
                Proyectos temp = contexto.Proyectos.FirstOrDefault(x => x.Codigo == proyecto.Codigo);

                temp.Nombre = proyecto.Nombre;
                temp.Completado = proyecto.Completado;
                temp.FechaComienzo = proyecto.FechaComienzo;
                temp.Fase = proyecto.Fase;
                temp.HorasNormales = proyecto.HorasNormales;
                temp.HorasEspeciales = proyecto.HorasEspeciales;
                temp.HorasReales = proyecto.HorasReales;
                temp.HorasAnalisis = proyecto.HorasAnalisis;
                temp.FechaInicio = proyecto.FechaInicio;
                temp.FechaDisenio = proyecto.FechaDisenio;
                temp.FechaValidacion = proyecto.FechaValidacion;
                temp.FechaEnVivo = proyecto.FechaEnVivo;
                temp.FechaRecepcion = proyecto.FechaRecepcion;
                temp.FechaFin = proyecto.FechaFin;
                temp.Cliente = proyecto.Cliente;

                if (!IdUsuario.Equals("null"))
                {
                    Usuarios u = contexto.Usuarios.Find(Int32.Parse(IdUsuario));
                    temp.IdUsuario = u.IdUsuario;
                    temp.JefeProyecto = u.Usuario;
                }
                else
                {
                    //Para registra el valor de sin técnico
                    temp.IdUsuario = null;
                    temp.JefeProyecto = null;
                }

                if (!Consultor.Equals("null"))
                {
                    temp.Consultor = Consultor;
                }
                else
                {
                    //Para registra el valor de sin consultor
                    temp.Consultor = null;
                }
                temp.CodigoOferta = proyecto.CodigoOferta;
                temp.Replanificaciones = proyecto.Replanificaciones;
                temp.Incidencias = proyecto.Incidencias;
                temp.HorasDesarrollo = proyecto.HorasReales - proyecto.HorasAnalisis;
                temp.Desviacion = CalcularDesviacion(proyecto.HorasEspeciales, proyecto.HorasNormales, proyecto.HorasReales);
                temp.DesviacionAnalisis = CalcularDesviacionAnalisis(proyecto.HorasEspeciales, proyecto.HorasNormales, proyecto.HorasAnalisis);
                temp.DesviacionDesarrollo = CalcularDesviacionDesarrollo(proyecto.HorasEspeciales, proyecto.HorasNormales, proyecto.HorasAnalisis, proyecto.HorasReales);
                contexto.SaveChanges();

                return RedirectToAction("verProyecto", new { proyecto.Codigo, page, size, projectS, campoOrden, buscar });
            }
        }
        [NoDirectAccess]
        public ActionResult ListarProyectos(int? page, int? size, string projectS, string campoOrden, string buscar, int? idUsu, string sentido)
        {
            int pageNumber = page ?? 1;
            int pageSize = size ?? 10;
            string projectStatus = projectS ?? "No finalizados";
            string pageOrden = campoOrden ?? "FechaFin";
            string buscarC = buscar ?? "";
            int idUsuarioSelect = idUsu ?? ((Usuarios)Session["usuario"]).IdUsuario;
            string sentidoL = sentido ?? "asc";

            List<Proyectos> misProyectos = new List<Proyectos>();
            List<Proyectos> misProyectosExportar = new List<Proyectos>();
            List<Proyectos> buscado = new List<Proyectos>();

            //Usu seleccionado
            Usuarios usu = contexto.Usuarios.FirstOrDefault(x => x.IdUsuario == idUsuarioSelect);

            foreach (Proyectos proy in contexto.Proyectos.ToList())
            {
                if (projectStatus == "Todos")
                {
                    misProyectos.Add(proy);
                }
                else if (projectStatus == "Finalizados" && proy.Fase == "Ended")
                {
                    misProyectos.Add(proy);
                }
                else if (projectStatus == "No finalizados" && proy.Fase != "Ended")
                {
                    misProyectos.Add(proy);
                }
            }

            ViewBag.pageNumber = pageNumber;
            ViewBag.pageSize = pageSize;
            ViewBag.projectStatus = projectStatus;
            ViewBag.campo = pageOrden;
            ViewBag.buscarC = buscarC;
            ViewBag.idUsuarioSelect = idUsuarioSelect;
            ViewBag.sentido = sentidoL;

            List<Proyectos> misProyectosFiltrados = new List<Proyectos>();
            if (usu.Usuario.Equals("admin"))
            {
                misProyectosFiltrados = misProyectos;
            }
            else if (!usu.Admin == false)
            {
                if (usu.Equipos != null)
                {
                    foreach (Equipos e in usu.Equipos)
                    {
                        foreach (Usuarios u in e.Usuarios)
                        {
                            foreach (Proyectos p in misProyectos)
                                if (p.JefeProyecto != null && !misProyectosFiltrados.Contains(p) && p.JefeProyecto.Equals(u.Usuario))
                                {
                                    misProyectosFiltrados.Add(p);
                                }
                                else if (p.Consultor != null && !misProyectosFiltrados.Contains(p) && p.Consultor.Equals(u.Usuario))
                                {
                                    misProyectosFiltrados.Add(p);

                                }
                        }
                    }
                }
            }
            else
            {
                if (misProyectos.Count > 0)
                {
                    foreach (Proyectos p in misProyectos)
                    {
                        if (p.JefeProyecto != null && p.JefeProyecto.Equals(usu.Usuario))
                        {
                            misProyectosFiltrados.Add(p);
                        }
                        else if (p.Consultor != null && p.Consultor.Equals(usu.Usuario))
                        {
                            misProyectosFiltrados.Add(p);
                        }
                    }
                }
            }



            //Para que se ordenen en fución del campo elegido
            var campo = typeof(Proyectos).GetProperty(pageOrden);
            if (sentidoL == "asc")
            {
                misProyectosFiltrados = misProyectosFiltrados.OrderBy(r => campo.GetValue(r, null)).ToList();
            }
            else
            {
                misProyectosFiltrados = misProyectosFiltrados.OrderByDescending(r => campo.GetValue(r, null)).ToList();
            }


            if (!string.IsNullOrWhiteSpace(buscar))
            {
                buscado = misProyectosFiltrados.Where(x => x.Nombre.ToLower().Contains(buscar.ToLower()) || x.Codigo.ToLower().Contains(buscar.ToLower()) || x.Cliente.ToLower().Contains(buscar.ToLower())).ToList();

                Session["Proyectos"] = buscado;
                var pagedList = buscado.ToPagedList(pageNumber, pageSize);
                //return View(pagedList);
                if (pagedList.Count == 0)
                {
                    while (pagedList.Count == 0 && pageNumber > 1)
                    {
                        pageNumber--;
                        pagedList = buscado.ToPagedList(pageNumber, pageSize);
                    }
                    ViewBag.pageNumber = pageNumber + 1;
                }
                return View(pagedList);
            }
            else
            {
                Session["Proyectos"] = misProyectosFiltrados;
                var pagedList = misProyectosFiltrados.ToPagedList(pageNumber, pageSize);
                //return View(pagedList);
                if (pagedList.Count == 0)
                {
                    while (pagedList.Count == 0 && pageNumber > 1)
                    {
                        pageNumber--;
                        pagedList = misProyectosFiltrados.ToPagedList(pageNumber, pageSize);
                    }
                    ViewBag.pageNumber = pageNumber + 1;
                }
                return View(pagedList);
            }
        }

        [HttpPost]
        [NoDirectAccess]
        public ActionResult EliminarProyecto(string codigo, int? page, int? size, string projectS, string campoOrden, string buscar, string borrarProyecto)
        {
            Thread.Sleep(2000);
            if (borrarProyecto == "Borrar")
            {
                contexto.Proyectos.Remove(contexto.Proyectos.FirstOrDefault(x => x.Codigo == codigo));
                contexto.SaveChanges();

                return RedirectToAction("ListarProyectos", new { page, size, projectS, campoOrden, buscar });
            }
            else
            {
                return RedirectToAction("verProyecto", new { codigo, page, size, projectS, campoOrden, buscar });
            }
        }
        public JsonResult CheckUnique(string value)
        {
            bool isUnique = !contexto.Proyectos.Any(x => x.Codigo == value);
            return Json(isUnique, JsonRequestBehavior.AllowGet);
        }

        [NoDirectAccess]
        public ActionResult ImportarProyecto()
        {
            return View("ImportarProyecto");
        }

        [HttpPost]
        [NoDirectAccess]
        public ActionResult ImportarProyecto(HttpPostedFileBase file, string modificacion)
        {
            if (file != null && file.ContentLength > 0)
            {
                if (Path.GetExtension(file.FileName).Equals(".csv", StringComparison.OrdinalIgnoreCase))
                {
                    var csvData = new List<string[]>();
                    using (var reader = new StreamReader(file.InputStream))
                    {
                        while (!reader.EndOfStream)
                        {
                            var line = reader.ReadLine();
                            var values = line.Split(';');
                            csvData.Add(values);
                        }
                    }

                    //var hola = csvData[1][14];

                    var proyectosExistentes = contexto.Proyectos.ToDictionary(p => p.Codigo, p => p);

                    using (var db = new DB_ManagementEntities())
                    {
                        List<Usuarios> usuarios = new List<Usuarios>();
                        foreach (Usuarios usuario in contexto.Usuarios.ToList())
                        {
                            usuarios.Add(usuario);
                        }
                        decimal defectoHoras = 0M;
                        int defectoCompletado = 0;
                        try
                        {
                            for (int i = 1; i < csvData.Count; i++)
                            {
                                var proyecto = new Proyectos
                                {
                                    Nombre = csvData[i][0],
                                    Completado = int.TryParse(csvData[i][1], out int completado) ? completado : defectoCompletado,
                                    FechaComienzo = DateTime.Parse(csvData[i][2]),
                                    Fase = csvData[i][3].ToString(),
                                    HorasNormales = decimal.TryParse(csvData[i][4], out decimal horasNormales) ? horasNormales : defectoHoras,
                                    HorasEspeciales = decimal.TryParse(csvData[i][5], out decimal horasEspeciales) ? horasEspeciales : defectoHoras,
                                    HorasReales = decimal.TryParse(csvData[i][6], out decimal horasReales) ? horasReales : defectoHoras,
                                    FechaInicio = string.IsNullOrWhiteSpace(csvData[i][7]) ? (DateTime?)null : DateTime.Parse(csvData[i][7]),
                                    FechaDisenio = string.IsNullOrWhiteSpace(csvData[i][8]) ? (DateTime?)null : DateTime.Parse(csvData[i][8]),
                                    FechaValidacion = string.IsNullOrWhiteSpace(csvData[i][9]) ? (DateTime?)null : DateTime.Parse(csvData[i][9]),
                                    FechaEnVivo = string.IsNullOrWhiteSpace(csvData[i][10]) ? (DateTime?)null : DateTime.Parse(csvData[i][10]),
                                    FechaRecepcion = string.IsNullOrWhiteSpace(csvData[i][11]) ? (DateTime?)null : DateTime.Parse(csvData[i][11]),
                                    FechaFin = string.IsNullOrWhiteSpace(csvData[i][12]) ? (DateTime?)null : DateTime.Parse(csvData[i][12]),
                                    Cliente = csvData[i][13],
                                    JefeProyecto = string.IsNullOrWhiteSpace(csvData[i][14]) ? null : csvData[i][14],
                                    Codigo = csvData[i][15],
                                    CodigoOferta = csvData[i][16],
                                    Replanificaciones = string.IsNullOrWhiteSpace(csvData[i][17]) ? (int?)null : int.Parse(csvData[i][17]),
                                    Incidencias = string.IsNullOrWhiteSpace(csvData[i][18]) ? (int?)null : int.Parse(csvData[i][18]),
                                    HorasAnalisis = decimal.TryParse(csvData[i][19], out decimal horasAnalisis) ? horasAnalisis : defectoHoras,
                                    Consultor = string.IsNullOrWhiteSpace(csvData[i][20]) ? null : csvData[i][20],
                                };

                                proyecto.HorasDesarrollo = proyecto.HorasReales - proyecto.HorasAnalisis;

                                proyecto.Desviacion = CalcularDesviacion(proyecto.HorasEspeciales, proyecto.HorasNormales, proyecto.HorasReales);
                                proyecto.DesviacionAnalisis = CalcularDesviacionAnalisis(proyecto.HorasEspeciales, proyecto.HorasNormales, proyecto.HorasAnalisis);
                                proyecto.DesviacionDesarrollo = CalcularDesviacionDesarrollo(proyecto.HorasEspeciales, proyecto.HorasNormales, proyecto.HorasAnalisis, proyecto.HorasReales);

                                if (proyecto.JefeProyecto != null)
                                {
                                    foreach (Usuarios usuario in usuarios)
                                    {
                                        if (usuario.Usuario == proyecto.JefeProyecto)
                                        {
                                            proyecto.IdUsuario = usuario.IdUsuario;
                                        }
                                    }
                                }

                                if (modificacion == "Sobreescribir")
                                {
                                    //Sobreescribe los registros existentes y añade los registros nuevos
                                    if (proyectosExistentes.ContainsKey(proyecto.Codigo))
                                    {
                                        db.Proyectos.Remove(db.Proyectos.FirstOrDefault(x => x.Codigo == proyecto.Codigo));
                                    }
                                    db.Proyectos.Add(proyecto);
                                    db.SaveChanges();
                                }
                                else if (modificacion == "Actualizar")
                                {
                                    //Actualiza la tabla sin sobreescribir los registros existentes, es decir añade solo registros nuevos
                                    if (!proyectosExistentes.ContainsKey(proyecto.Codigo))
                                    {
                                        db.Proyectos.Add(proyecto);
                                        db.SaveChanges();

                                    }
                                }

                            }
                        }
                        catch
                        {
                            ViewBag.Error = "Se produjo un error al importar el archivo ";
                            return View("ImportarProyecto");
                        }
                    }
                    Thread.Sleep(1500);
                    return RedirectToAction("ListarProyectos", "Proyecto");
                }
            }
            return View();
        }

        public decimal CalcularDesviacion(decimal hEspeciales, decimal hNormales, decimal hReales)
        {
            if (hEspeciales == 0 && hNormales == 0)
            {
                return 0;
            }
            else
            {
                decimal cuenta = Math.Round(((hReales - (hNormales + hEspeciales)) * 100) / (hNormales + hEspeciales), 2);
                return cuenta;
            }
        }
        public decimal CalcularDesviacionAnalisis(decimal hEspeciales, decimal hNormales, decimal hAnalisis)
        {
            if (hEspeciales == 0 && hNormales == 0)
            {
                return 0;
            }
            else
            {
                decimal hPlanificadas = hNormales + hEspeciales;
                decimal hPlanificadasAnalisis = 0.2m * hPlanificadas;



                decimal desviacionA = ((hAnalisis - hPlanificadasAnalisis) * 100) / hPlanificadasAnalisis;
                decimal cuenta = Math.Round(desviacionA, 2);
                return cuenta;
            }
        }
        public decimal CalcularDesviacionDesarrollo(decimal hEspeciales, decimal hNormales, decimal hAnalisis, decimal hReales)
        {
            if (hEspeciales == 0 && hNormales == 0)
            {
                return 0;
            }
            else
            {
                decimal hDesarrollo = hReales - hAnalisis;
                decimal hPlanificadasDesarrollo = 0.8m * hReales;



                decimal desviacionD = ((hDesarrollo - hPlanificadasDesarrollo) * 100) / hPlanificadasDesarrollo;
                decimal cuenta = Math.Round(desviacionD, 2);
                return cuenta;
            }
        }
        public JsonResult AutoCompletado(string term)
        {
            using (DB_ManagementEntities db = new DB_ManagementEntities())
            {
                var resultado = db.Proyectos.Where(x => x.Nombre.Contains(term))
                    .Select(x => x.Nombre).Take(5).ToList();

                return Json(resultado, JsonRequestBehavior.AllowGet);
            }
        }

        [NoDirectAccess]
        public ActionResult ProyectosPendientes()
        {
            List<Proyectos> proyectos = contexto.Proyectos.ToList();
            List<Usuarios> misUsuarios = new List<Usuarios>();
            Usuarios usuarioActual = ((Usuarios)Session["usuario"]);
            List<Tuple<DateTime?, string, string, string>> fechas = new List<Tuple<DateTime?, string, string, string>>();
            bool tieneEquipo = true;

            if (usuarioActual.Equipos.Count == 0)
            {
                tieneEquipo = false;
            }
            else
            {
                foreach (Equipos e in usuarioActual.Equipos)
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

            foreach (Proyectos proyecto in proyectos)
            {
                if (!usuarioActual.Admin == true && proyecto.JefeProyecto == usuarioActual.Usuario)
                {
                    fechas.Add(new Tuple<DateTime?, string, string, string>(proyecto.FechaInicio, proyecto.JefeProyecto, proyecto.Nombre, "Inicio"));
                    fechas.Add(new Tuple<DateTime?, string, string, string>(proyecto.FechaDisenio, proyecto.JefeProyecto, proyecto.Nombre, "Diseño"));
                    fechas.Add(new Tuple<DateTime?, string, string, string>(proyecto.FechaValidacion, proyecto.JefeProyecto, proyecto.Nombre, "Validacion"));
                    fechas.Add(new Tuple<DateTime?, string, string, string>(proyecto.FechaEnVivo, proyecto.JefeProyecto, proyecto.Nombre, "En Vivo"));
                    fechas.Add(new Tuple<DateTime?, string, string, string>(proyecto.FechaRecepcion, proyecto.JefeProyecto, proyecto.Nombre, "Recepcion"));
                    fechas.Add(new Tuple<DateTime?, string, string, string>(proyecto.FechaFin, proyecto.JefeProyecto, proyecto.Nombre, "Fin"));
                }
                else if (usuarioActual.Admin == true && usuarioActual.Usuario != "admin")
                {
                    foreach (Usuarios u in misUsuarios)
                    {
                        if (proyecto.JefeProyecto == u.Usuario)
                        {
                            fechas.Add(new Tuple<DateTime?, string, string, string>(proyecto.FechaInicio, proyecto.JefeProyecto, proyecto.Nombre, "Inicio"));
                            fechas.Add(new Tuple<DateTime?, string, string, string>(proyecto.FechaDisenio, proyecto.JefeProyecto, proyecto.Nombre, "Diseño"));
                            fechas.Add(new Tuple<DateTime?, string, string, string>(proyecto.FechaValidacion, proyecto.JefeProyecto, proyecto.Nombre, "Validacion"));
                            fechas.Add(new Tuple<DateTime?, string, string, string>(proyecto.FechaEnVivo, proyecto.JefeProyecto, proyecto.Nombre, "En Vivo"));
                            fechas.Add(new Tuple<DateTime?, string, string, string>(proyecto.FechaRecepcion, proyecto.JefeProyecto, proyecto.Nombre, "Recepcion"));
                            fechas.Add(new Tuple<DateTime?, string, string, string>(proyecto.FechaFin, proyecto.JefeProyecto, proyecto.Nombre, "Fin"));
                        }
                    }
                }
                else if (usuarioActual.Consultor == true)
                {
                    if (tieneEquipo)
                    {
                        foreach (Usuarios u in misUsuarios)
                        {
                            if (proyecto.Consultor == u.Usuario)
                            {
                                fechas.Add(new Tuple<DateTime?, string, string, string>(proyecto.FechaInicio, proyecto.JefeProyecto, proyecto.Nombre, "Inicio"));
                                fechas.Add(new Tuple<DateTime?, string, string, string>(proyecto.FechaDisenio, proyecto.JefeProyecto, proyecto.Nombre, "Diseño"));
                                fechas.Add(new Tuple<DateTime?, string, string, string>(proyecto.FechaValidacion, proyecto.JefeProyecto, proyecto.Nombre, "Validacion"));
                            }
                        }
                    }
                    else if (proyecto.Consultor == usuarioActual.Usuario)
                    {
                        fechas.Add(new Tuple<DateTime?, string, string, string>(proyecto.FechaInicio, proyecto.JefeProyecto, proyecto.Nombre, "Inicio"));
                        fechas.Add(new Tuple<DateTime?, string, string, string>(proyecto.FechaDisenio, proyecto.JefeProyecto, proyecto.Nombre, "Diseño"));
                        fechas.Add(new Tuple<DateTime?, string, string, string>(proyecto.FechaValidacion, proyecto.JefeProyecto, proyecto.Nombre, "Validacion"));
                    }

                }
                else if (usuarioActual.Usuario == "admin")
                {
                    fechas.Add(new Tuple<DateTime?, string, string, string>(proyecto.FechaInicio, proyecto.JefeProyecto, proyecto.Nombre, "Inicio"));
                    fechas.Add(new Tuple<DateTime?, string, string, string>(proyecto.FechaDisenio, proyecto.JefeProyecto, proyecto.Nombre, "Diseño"));
                    fechas.Add(new Tuple<DateTime?, string, string, string>(proyecto.FechaValidacion, proyecto.JefeProyecto, proyecto.Nombre, "Validacion"));
                    fechas.Add(new Tuple<DateTime?, string, string, string>(proyecto.FechaEnVivo, proyecto.JefeProyecto, proyecto.Nombre, "En Vivo"));
                    fechas.Add(new Tuple<DateTime?, string, string, string>(proyecto.FechaRecepcion, proyecto.JefeProyecto, proyecto.Nombre, "Recepcion"));
                    fechas.Add(new Tuple<DateTime?, string, string, string>(proyecto.FechaFin, proyecto.JefeProyecto, proyecto.Nombre, "Fin"));
                }
            }



            var fechasOrdenadas = fechas.OrderBy(r => r.Item1).ToList();



            fechasOrdenadas.RemoveAll(p => p.Item1 == null);


            return View("ProyectosPendientes", fechasOrdenadas);
        }

        public List<Tuple<string, string, string, string, string>> ProyectosHoy(int idUs)
        {
            List<Proyectos> misProyectos = new List<Proyectos>();
            List<Usuarios> misUsuarios = new List<Usuarios>();
            var fechasOrdenadas = new List<Tuple<DateTime?, string, string, string, string>>();
            var data = new List<Tuple<string, string, string, string, string>>();

            if (idUs != null)
            {
                int usuId = idUs;
                Usuarios usu = contexto.Usuarios.FirstOrDefault(x => x.IdUsuario == usuId);

                if (usu.Usuario.Equals("admin"))
                {
                    misProyectos = contexto.Proyectos.ToList();
                }
                else if (usu.Admin == true || usu.Consultor == true)
                {
                    if (usu.Equipos.Count > 0)
                    {
                        foreach (Equipos e in usu.Equipos)
                        {
                            foreach (Usuarios u in e.Usuarios)
                            {
                                if (!misUsuarios.Contains(u))
                                {
                                    misUsuarios.Add(u);
                                }
                            }
                        }
                        if (usu.Consultor != true)
                        {
                            foreach (Proyectos p in contexto.Proyectos)
                            {
                                foreach (Usuarios u in misUsuarios)
                                {
                                    if (p.JefeProyecto != null && u.Usuario.Equals(p.JefeProyecto) && !misProyectos.Contains(p))
                                    {
                                        misProyectos.Add(p);
                                    }
                                }
                            }
                        }
                        else
                        {
                            foreach (Proyectos p in contexto.Proyectos)
                            {
                                foreach (Usuarios u in misUsuarios)
                                {
                                    if (p.Consultor != null && u.Usuario.Equals(p.Consultor) && !misProyectos.Contains(p))
                                    {
                                        misProyectos.Add(p);
                                    }
                                }
                            }
                        }

                    }
                    else
                    {
                        if (usu.Consultor != true)
                        {
                            foreach (Proyectos p in contexto.Proyectos)
                            {

                                if (p.JefeProyecto != null && usu.Usuario.Equals(p.JefeProyecto) && !misProyectos.Contains(p))
                                {
                                    misProyectos.Add(p);
                                }
                            }
                        }
                        else
                        {
                            foreach (Proyectos p in contexto.Proyectos)
                            {

                                if (p.Consultor != null && usu.Usuario.Equals(p.Consultor) && !misProyectos.Contains(p))
                                {
                                    misProyectos.Add(p);
                                }

                            }
                        }

                    }
                }
                else
                {
                    foreach (Proyectos p in contexto.Proyectos)
                    {
                        if (p.JefeProyecto != null && usu.Usuario.Equals(p.JefeProyecto) && !misProyectos.Contains(p))
                        {
                            misProyectos.Add(p);
                        }
                    }
                }

                if (misProyectos.Count > 0)
                {
                    List<Tuple<DateTime?, string, string, string, string>> fechas = new List<Tuple<DateTime?, string, string, string, string>>();
                    foreach (Proyectos proyecto in misProyectos)
                    {
                        fechas.Add(new Tuple<DateTime?, string, string, string, string>(proyecto.FechaInicio, proyecto.JefeProyecto, proyecto.Nombre, "Inicio", proyecto.Codigo));
                        fechas.Add(new Tuple<DateTime?, string, string, string, string>(proyecto.FechaDisenio, proyecto.JefeProyecto, proyecto.Nombre, "Diseño", proyecto.Codigo));
                        fechas.Add(new Tuple<DateTime?, string, string, string, string>(proyecto.FechaValidacion, proyecto.JefeProyecto, proyecto.Nombre, "Validacion", proyecto.Codigo));
                        fechas.Add(new Tuple<DateTime?, string, string, string, string>(proyecto.FechaEnVivo, proyecto.JefeProyecto, proyecto.Nombre, "En Vivo", proyecto.Codigo));
                        fechas.Add(new Tuple<DateTime?, string, string, string, string>(proyecto.FechaRecepcion, proyecto.JefeProyecto, proyecto.Nombre, "Recepcion", proyecto.Codigo));
                        fechas.Add(new Tuple<DateTime?, string, string, string, string>(proyecto.FechaFin, proyecto.JefeProyecto, proyecto.Nombre, "Fin", proyecto.Codigo));
                    }
                    fechasOrdenadas = fechas.OrderBy(r => r.Item1).ToList();
                }

                fechasOrdenadas.RemoveAll(p => p.Item1 == null || p.Item1?.ToString("dd/MM/yyyy") != DateTime.Now.ToString("dd/MM/yyyy"));

                if (usu.Consultor == true)
                {
                    fechasOrdenadas.RemoveAll(p => p.Item4.Equals("En Vivo") || p.Item4.Equals("Recepcion") || p.Item4.Equals("Fin"));
                }

                foreach (var d in fechasOrdenadas)
                {
                    if (usu.Admin == false)
                    {
                        data.Add(new Tuple<string, string, string, string, string>(d.Item1?.ToString(), "", d.Item3, d.Item4, d.Item5));
                    }
                    else
                    {
                        data.Add(new Tuple<string, string, string, string, string>(d.Item1?.ToString(), d.Item2 == null ? null : d.Item2, d.Item3, d.Item4, d.Item5));
                    }

                }
            }

            return data;
        }

        public ActionResult ExportarProyecto(List<string> valoresSeleccionados)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Nombre;Completado;FechaComienzo;Fase;HorasNormales;HorasEspeciales;HorasReales;FechaInicio;FechaDisenio;FechaValidacion;FechaEnVivo;FechaRecepcion;FechaFin;Cliente;JefeProyecto;Codigo;CodigoOferta;Replanificaciones;Incidencias;HorasAnalisis;Consultor");



            IEnumerable<Proyectos> proyectos = new List<Proyectos>();



            if (valoresSeleccionados == null || valoresSeleccionados.Count == 0)
            {
                proyectos = Session["Proyectos"] as IEnumerable<Proyectos>;
            }
            else
            {
                List<Proyectos> listaProyectos = new List<Proyectos>();
                foreach (var codigo in valoresSeleccionados)
                {
                    var proyecto = contexto.Proyectos.FirstOrDefault(x => x.Codigo == codigo);
                    if (proyecto != null)
                    {
                        listaProyectos.Add(proyecto);
                    }
                }
                proyectos = listaProyectos;
            }

            foreach (var proyecto in proyectos)
            {
                sb.AppendLine(string.Join(";",
                proyecto.Nombre,
                proyecto.Completado,
                proyecto.FechaComienzo.ToString("dd/MM/yyyy"),
                proyecto.Fase,
                proyecto.HorasNormales,
                proyecto.HorasEspeciales,
                proyecto.HorasReales,
                proyecto.FechaInicio.HasValue ? proyecto.FechaInicio.Value.ToString("dd/MM/yyyy") : "",
                proyecto.FechaDisenio.HasValue ? proyecto.FechaDisenio.Value.ToString("dd/MM/yyyy") : "",
                proyecto.FechaValidacion.HasValue ? proyecto.FechaValidacion.Value.ToString("dd/MM/yyyy") : "",
                proyecto.FechaEnVivo.HasValue ? proyecto.FechaEnVivo.Value.ToString("dd/MM/yyyy") : "",
                proyecto.FechaRecepcion.HasValue ? proyecto.FechaRecepcion.Value.ToString("dd/MM/yyyy") : "",
                proyecto.FechaFin.HasValue ? proyecto.FechaFin.Value.ToString("dd/MM/yyyy") : "",
                proyecto.Cliente,
                proyecto.JefeProyecto,
                proyecto.Codigo,
                proyecto.CodigoOferta,
                proyecto.Replanificaciones,
                proyecto.Incidencias,
                proyecto.HorasAnalisis,
                proyecto.Consultor));
            }

            byte[] buffer = Encoding.UTF8.GetBytes(sb.ToString());
            return File(buffer, "text/csv", "proyectos.csv");
        }

        public ActionResult EliminarProyectos(List<string> valoresSeleccionados)
        {
            foreach (var codigo in valoresSeleccionados)
            {
                contexto.Proyectos.Remove(contexto.Proyectos.FirstOrDefault(x => x.Codigo == codigo));
                contexto.SaveChanges();
            }
            return RedirectToAction("ListarProyectos");
        }


    }
}