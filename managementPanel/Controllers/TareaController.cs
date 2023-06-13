using managementPanel.Controllers;
using managementPanel.Models;
using Microsoft.Ajax.Utilities;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;

namespace managementPanel.Controllers
{
    public class TareaController : Controller
    {
        DB_ManagementEntities contexto = new DB_ManagementEntities();

        [NoDirectAccess]
        public ActionResult TableroTareas(string Usuario, string buscar, string idProyecto)
        {
            List<Usuarios> misUsuarios = new List<Usuarios>();
            List<Tareas> tareas = new List<Tareas>();
            int idBuscar = ((Usuarios)Session["usuario"]).IdUsuario;
            string buscarTablero = buscar ?? string.Empty;

            if (idProyecto == "Ver todas")
            {
                idProyecto = null;
            }
            string idProyect = idProyecto ?? null;

            Usuarios usu = contexto.Usuarios.FirstOrDefault(x => x.IdUsuario == idBuscar);

            if (idProyecto != null)
            {
                foreach (Tareas t in contexto.Tareas.ToList())
                {
                    if (t.IdProyecto == idProyecto)
                    {
                        tareas.Add(t);
                    }
                }
            }
            else
            {
                if (!usu.Admin == true)
                {
                    foreach (Tareas t in contexto.Tareas.ToList())
                    {
                        if (t.IdUsuario == usu.IdUsuario)
                        {
                            tareas.Add(t);
                        }
                    }
                }
                else
                {
                    if (usu.Usuario.Equals("admin"))
                    {
                        misUsuarios = contexto.Usuarios.ToList();

                        if (Usuario != null && !Usuario.Equals("Ver todas"))
                        {
                            Usuarios usuSelect = contexto.Usuarios.FirstOrDefault(x => x.Usuario.Equals(Usuario));

                            foreach (Tareas t in contexto.Tareas.ToList())
                            {
                                if (t.IdUsuario == usuSelect.IdUsuario)
                                {
                                    tareas.Add(t);
                                }
                            }
                        }
                        else
                        {
                            tareas = contexto.Tareas.ToList();
                        }
                    }
                    else if (usu.Admin == true)
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
                        }
                        if (Usuario != null)
                        {
                            if (Usuario.Equals("Ver todas"))
                            {
                                foreach (Usuarios u in misUsuarios)
                                {
                                    foreach (Tareas t in contexto.Tareas.ToList())
                                    {
                                        if (t.IdUsuario == u.IdUsuario)
                                        {
                                            tareas.Add(t);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                Usuarios usuSelect = contexto.Usuarios.FirstOrDefault(x => x.Usuario.Equals(Usuario));

                                foreach (Tareas t in contexto.Tareas.ToList())
                                {
                                    if (t.IdUsuario == usuSelect.IdUsuario)
                                    {
                                        tareas.Add(t);
                                    }
                                }
                            }
                        }
                    }
                }

                if (!String.IsNullOrEmpty(buscarTablero))
                {
                    tareas = tareas.Where(x => x.Titulo.ToLower().Contains(buscarTablero.ToLower())).ToList();
                }
            }


            object[] model = new object[3];

            model[0] = misUsuarios;
            model[1] = tareas;
            model[2] = idProyect;
            return View(model);
        }

        [NoDirectAccess]
        public void CambiarEstado(int idTarea, string estado)
        {
            if (estado == "Por hacer" || estado == "En curso" || estado == "Bloqueo" || estado == "Listo")
            {
                Tareas tarea = contexto.Tareas.FirstOrDefault(x => x.IdTarea == idTarea);
                tarea.Estado = estado;
                contexto.SaveChanges();
            }
        }

        [HttpPost]
        [NoDirectAccess]
        public ActionResult EditarTarea(Tareas tareas)
        {
            if (tareas.Titulo == null)
            {
                TempData["msjEditar"] = "ERROR - Tarea no editada";
                return RedirectToAction("ListarTareas", "Tarea");
            }
            else
            {
                Tareas temp = contexto.Tareas.FirstOrDefault(x => x.IdTarea == tareas.IdTarea);

                temp.IdUsuario = tareas.IdUsuario;
                temp.Titulo = tareas.Titulo;
                temp.IdProyecto = tareas.IdProyecto;
                temp.Descripcion = tareas.Descripcion;
                temp.Horas = tareas.Horas;
                temp.FechaFin = tareas.FechaFin;
                temp.Estado = tareas.Estado;

                contexto.SaveChanges();

                return RedirectToAction("TableroTareas");
            }
        }

        [HttpPost]
        [NoDirectAccess]
        public ActionResult EliminarTarea(int idTarea)
        {
            contexto.Tareas.Remove(contexto.Tareas.FirstOrDefault(x => x.IdTarea == idTarea));
            contexto.SaveChanges();

            return RedirectToAction("TableroTareas");

        }

        [HttpPost]
        [NoDirectAccess]
        public ActionResult InsertarTareas(Tareas tareas)
        {
            if (tareas.IdUsuario == 0)
            {
                tareas.IdUsuario = ((Usuarios)Session["usuario"]).IdUsuario;
            }

            contexto.Tareas.Add(tareas);
            contexto.SaveChanges();

            if (tareas.IdProyecto == null)
            {
                return RedirectToAction("ListarTareas");
            }
            else
            {
                return RedirectToAction("TableroTareas", new { idProyecto = tareas.IdProyecto });
            }
        }

        [NoDirectAccess]
        public ActionResult ListarTareas(int? page, int? pageS, string buscar, string campoOrden, string Usuario)
        {
            List<Tareas> tareasTotales = contexto.Tareas.ToList();
            List<Tareas> tareas = new List<Tareas>();
            List<Tareas> tareasBuscadas = new List<Tareas>();
            List<Usuarios> misUsuarios = new List<Usuarios>();

            int pageNumber = page ?? 1;
            int pageSize = pageS ?? 10;
            string buscarTarea = buscar ?? "";
            string pageOrden = campoOrden ?? "FechaFin";

            ViewBag.pageSize = pageSize;
            ViewBag.campo = pageOrden;

            int id = ((Usuarios)Session["usuario"]).IdUsuario;
            Usuarios usu = contexto.Usuarios.FirstOrDefault(x => x.IdUsuario == id);


            if (usu.Usuario.Equals("admin"))
            {
                misUsuarios = contexto.Usuarios.ToList();

                if (Usuario == null || Usuario.Equals("Ver todas"))
                {
                    tareas = tareasTotales;
                }
                else
                {
                    Usuarios usuSelect = contexto.Usuarios.FirstOrDefault(x => x.Usuario.Equals(Usuario));

                    foreach (Tareas t in contexto.Tareas.ToList())
                    {
                        if (t.IdUsuario == usuSelect.IdUsuario)
                        {
                            tareas.Add(t);
                        }
                    }
                }
            }
            else if (usu.Admin == true)
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
                }
                if (Usuario == null)
                {
                    if (((Usuarios)Session["usuario"]).Equipos.Count > 0)
                    {
                        foreach (Equipos e in contexto.Equipos)
                        {
                            foreach (Usuarios u in contexto.Usuarios)
                            {
                                foreach (Tareas t in contexto.Tareas)
                                {
                                    if (t.IdUsuario == u.IdUsuario && e.Usuarios.Contains(u))
                                    {
                                        tareas.Add(t);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (Usuario.Equals("Ver todas"))
                    {
                        foreach (Usuarios u in misUsuarios)
                        {
                            foreach (Tareas t in contexto.Tareas.ToList())
                            {
                                if (t.IdUsuario == u.IdUsuario)
                                {
                                    tareas.Add(t);
                                }
                            }
                        }
                    }
                    else
                    {
                        Usuarios usuSelect = contexto.Usuarios.FirstOrDefault(x => x.Usuario.Equals(Usuario));

                        foreach (Tareas t in contexto.Tareas.ToList())
                        {
                            if (t.IdUsuario == usuSelect.IdUsuario)
                            {
                                tareas.Add(t);
                            }
                        }
                    }
                }

            }
            else
            {
                tareas = contexto.Tareas.Where(x => x.IdUsuario == id).ToList();
            }

            if (!pageOrden.Equals("Usuarios"))
            {
                var campo = typeof(Tareas).GetProperty(pageOrden);

                tareas = tareas.OrderBy(r => campo.GetValue(r, null)).ToList();
            }
            else
            {
                tareas = tareas.OrderBy(x => x.Usuarios.Nombre).ToList();

            }

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                tareasBuscadas = tareas.Where(x => x.Titulo.ToLower().Contains(buscar.ToLower())).ToList();

                var pagedList = tareasBuscadas.ToPagedList(pageNumber, pageSize);

                if (pagedList.Count == 0)
                {
                    while (pagedList.Count == 0 && pageNumber > 1)
                    {
                        pageNumber--;
                        pagedList = tareasBuscadas.ToPagedList(pageNumber, pageSize);
                    }
                    ViewBag.pageNumber = pageNumber + 1;
                }

                return View(pagedList);
            }
            else
            {
                var pagedList = tareas.ToPagedList(pageNumber, pageSize);

                if (pagedList.Count == 0)
                {
                    while (pagedList.Count == 0 && pageNumber > 1)
                    {
                        pageNumber--;
                        pagedList = tareas.ToPagedList(pageNumber, pageSize);
                    }
                    ViewBag.pageNumber = pageNumber + 1;
                }

                return View(pagedList);
            }
        }

        public List<Tuple<string, string, DateTime?>> TareasHoy(int idUsu)
        {
            List<Tareas> tareasHoy = new List<Tareas>();
            var datos = new List<Tuple<string, string, DateTime?>>();

            Usuarios usu = contexto.Usuarios.FirstOrDefault(x => x.IdUsuario == idUsu);

            if (usu.Usuario.Equals("admin"))
            {
                tareasHoy = contexto.Tareas.ToList();

                if (tareasHoy.Count > 0)
                {
                    foreach (var tareas in tareasHoy)
                    {
                        foreach (Usuarios user in contexto.Usuarios)
                        {
                            if (tareas.IdUsuario == user.IdUsuario)
                            {
                                datos.Add(new Tuple<string, string, DateTime?>(tareas.Descripcion, user.Usuario, tareas.FechaFin));
                            }
                        }
                    }
                }
            }
            else if (usu.Admin == true)
            {
                if (usu.Equipos.Count > 0)
                {
                    foreach (Equipos e in contexto.Equipos)
                    {
                        foreach (Usuarios u in contexto.Usuarios)
                        {
                            foreach (Tareas t in contexto.Tareas)
                            {
                                if (t.IdUsuario == u.IdUsuario && e.Usuarios.Contains(u))
                                {
                                    datos.Add(new Tuple<string, string, DateTime?>(t.Descripcion, u.Usuario, t.FechaFin));
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                tareasHoy = contexto.Tareas.Where(x => x.IdUsuario == idUsu).ToList();


                if (tareasHoy.Count > 0)
                {
                    foreach (var tareas in tareasHoy)
                    {
                        datos.Add(new Tuple<string, string, DateTime?>(tareas.Descripcion, usu.Usuario, tareas.FechaFin));
                    }
                }

            }
            datos.RemoveAll(d => d.Item3?.ToString("dd/MM/yyyy") != DateTime.Now.ToString("dd/MM/yyyy"));
            return datos;
        }

        public ActionResult EliminarTareas(List<string> valoresSeleccionados)
        {
            foreach (var idTarea in valoresSeleccionados)
            {
                int id = Convert.ToInt16(idTarea);



                contexto.Tareas.Remove(contexto.Tareas.FirstOrDefault(x => x.IdTarea == id));
                contexto.SaveChanges();
            }
            return RedirectToAction("ListarTareas");
        }
    }
}
