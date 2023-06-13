using managementPanel.Controllers;
using managementPanel.Models;
using Microsoft.Ajax.Utilities;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web.Mvc;

namespace managementPanel.Controllers
{
    public class UsuarioController : Controller
    {
        DB_ManagementEntities contexto = new DB_ManagementEntities();
        LoginController loginController = new LoginController();


        /*
         * 
         * Método que lista todos los usuarios guardados en la base de datos.
         * Los muestra en la vista ListarUsuarios.
         * 
         */
        [NoDirectAccess]
        public ActionResult ListarUsuarios(int? page, int? pageS)
        {
            int pageNumber = page ?? 1;
            int pageSize = pageS ?? 10;

            ViewBag.pageSize = pageSize;

            List<Usuarios> usuarios = new List<Usuarios>();

            if (((Usuarios)Session["usuario"]).Usuario.Equals("admin"))
            {
                usuarios = contexto.Usuarios.Where(u => u.Usuario != "admin").ToList();

            }
            else
            {
                var usuarioActual = (Usuarios)Session["usuario"];
                var idsEquipos = usuarioActual.Equipos.Select(e => e.IdEquipo);
                usuarios = contexto.Usuarios.Where(u => u.Equipos.Any(e => idsEquipos.Contains(e.IdEquipo))).ToList();
            }

            usuarios = usuarios.OrderBy(e => e.Equipos.FirstOrDefault()?.Nombre).ThenBy(p => p.Nombre).ToList();

            var user = usuarios.ToPagedList(pageNumber, pageSize);

            if (user.Count == 0)
            {
                while (user.Count == 0 && pageNumber > 1)
                {
                    pageNumber--;
                    user = usuarios.ToPagedList(pageNumber, pageSize);
                }
            }

            ViewBag.pageNumber = pageNumber;

            return View(user);
        }

        /*
         * 
         * Método que muestra todos los datos de un usuario en específico.
         * 
         */
        [NoDirectAccess]
        public ActionResult verPerfil(int idUsuario, int? page)
        {
            Usuarios usuario = contexto.Usuarios.FirstOrDefault(x => x.IdUsuario == idUsuario);

            object[] model = new object[2];

            model[0] = usuario;
            model[1] = page;

            return View("Perfil", model);
        }


        /*
         * 
         * Método que elimina de la base de datos un usuario.
         * 
         */

        [HttpPost]
        [NoDirectAccess]
        public ActionResult Eliminar(string borrar, int? idUsuarioBorrar, int? page, int? pageS)
        {
            if (borrar != "Cancelar")
            {
                Usuarios usuario = contexto.Usuarios.FirstOrDefault(x => x.IdUsuario == idUsuarioBorrar);
                bool proyectosAsig = false;

                foreach (Proyectos p in contexto.Proyectos.ToList())
                {
                    if (p.JefeProyecto != null && usuario.Usuario.Equals(p.JefeProyecto))
                    {
                        proyectosAsig = true;
                    }
                }

                if (borrar == "Borrar" && proyectosAsig == false)
                {
                    contexto.Usuarios.Remove(usuario);
                    contexto.SaveChanges();
                }
            }

            Thread.Sleep(2000);

            return RedirectToAction("ListarUsuarios", new { page, pageS });
        }

        /*
         * 
         * Método que carga todos los datos del usuario a editar en la vista 
         * Editar de usuario.
         * 
         */
        [NoDirectAccess]
        public ActionResult Editar(int IdUsuario, int? page)
        {
            Usuarios modificar = contexto.Usuarios.FirstOrDefault(x => x.IdUsuario == IdUsuario);

            List<Equipos> data = contexto.Equipos.ToList();
            modificar.Contrasenna = loginController.Desencriptar(modificar.Contrasenna);

            object[] modelos = new object[3];
            modelos[0] = data;
            modelos[1] = modificar;

            modelos[2] = page;


            return View(modelos);
        }

        /*
         * 
         * Método que edita un usuario en la base de datos con los datos recibidos
         * en el formulario de Editar.
         * 
         */


        [HttpPost]
        [NoDirectAccess]
        public ActionResult Editar(Usuarios usuario, List<int> equiposId, int? page, string rol)
        {
            if (usuario.Usuario == null)
            {
                TempData["msj"] = "ERROR";
                return RedirectToAction("ListarUsuarios", "Usuario");
            }
            else
            {

                Usuarios temp = contexto.Usuarios.FirstOrDefault(x => x.IdUsuario == usuario.IdUsuario);
                temp.IdUsuario = usuario.IdUsuario;
                temp.Nombre = usuario.Nombre;
                temp.Apellidos = usuario.Apellidos;
                temp.Usuario = usuario.Usuario;

                if (rol.Equals("Admin"))
                {
                    usuario.Admin = true;
                }
                else if (rol.Equals("Técnico"))
                {
                    usuario.Admin = false;
                }
                else if (rol.Equals("Consultor"))
                {
                    usuario.Consultor = true;
                }

                if (usuario.Admin != null)
                {
                    temp.Admin = usuario.Admin;
                    temp.Consultor = null;
                }
                else if (usuario.Consultor != null)
                {
                    temp.Consultor = usuario.Consultor;
                    temp.Admin = null;
                }

                temp.Equipos.Clear();
                if (equiposId != null)
                {
                    foreach (int i in equiposId)
                    {
                        Equipos e = contexto.Equipos.Find(i);
                        temp.Equipos.Add(e);
                    }
                }
                temp.Contrasenna = loginController.Encriptar(usuario.Contrasenna);

                contexto.SaveChanges();


                TempData["msj"] = null;

                if (((Usuarios)Session["usuario"]).Admin == false)
                {
                    return RedirectToAction("Index", "Login");
                }

                return RedirectToAction("ListarUsuarios", new { page = page });
            }
        }

        public ActionResult VerVacaciones()
        {
            int idUsuario = ((Usuarios)Session["usuario"]).IdUsuario;

            Usuarios user = contexto.Usuarios.FirstOrDefault(x => x.IdUsuario == idUsuario);
            
            List<Usuarios> usuarios = contexto.Usuarios.Where(x => x.FechaInicioVacaciones != null && x.FechaFinVacaciones != null && x.Vacaciones == true && x.IdUsuario != idUsuario).ToList();

            List<Usuarios> misUsuarios = new List<Usuarios>();

            if (((Usuarios)Session["usuario"]).Admin == true)
            {
                if (((Usuarios)Session["usuario"]).Usuario.Equals("admin"))
                {
                    misUsuarios = usuarios;
                }
                else
                {
                    if (((Usuarios)Session["usuario"]).Equipos.Count > 0)
                    {
                        foreach (Equipos e in user.Equipos)
                        {
                            foreach (Usuarios u in e.Usuarios)
                            {
                                if (e.Usuarios.Contains(u) && u.FechaInicioVacaciones != null && u.FechaFinVacaciones != null)
                                {
                                    misUsuarios.Add(u);
                                }
                            }
                        }
                    }
                }                
            }
            else
            {
                if (user.Admin == false)
                {
                    misUsuarios.Add(user);
                }
                
                if (TempData["fecha"] == "1")
                {
                    ViewBag.fechas = "1";
                }
            }
            
            return View("Vacaciones", misUsuarios);
        }

        public List<Usuarios> VacacionesNotificaciones() 
        {
            List<Usuarios> usuarios = contexto.Usuarios.Where(x => x.FechaInicioVacaciones != null && x.FechaFinVacaciones != null && x.Vacaciones == false).ToList();

            return usuarios;
        }

        [HttpPost]
        public ActionResult SolicitarVacaciones(string fechaInicio, string fechaFin)
        {
            int idUsuario = ((Usuarios)Session["usuario"]).IdUsuario;

            Usuarios user = contexto.Usuarios.FirstOrDefault(x => x.IdUsuario == idUsuario);

            if (user.FechaFinVacaciones == null && user.FechaInicioVacaciones == null && user.Vacaciones == false)
            {
                if (!String.IsNullOrEmpty(fechaInicio) && !String.IsNullOrEmpty(fechaFin))
                {
                    if (Convert.ToDateTime(fechaInicio) < Convert.ToDateTime(fechaFin))
                    {
                        user.FechaInicioVacaciones = Convert.ToDateTime(fechaInicio);
                        user.FechaFinVacaciones = Convert.ToDateTime(fechaFin).AddDays(1);                        
                    }
                    else
                    {
                        user.FechaInicioVacaciones = Convert.ToDateTime(fechaFin);
                        user.FechaFinVacaciones = Convert.ToDateTime(fechaInicio).AddDays(1);                        
                    }
                    if (((Usuarios)Session["usuario"]).Admin == true)
                    {
                        user.Vacaciones = true;
                    }
                    contexto.SaveChanges();
                    TempData["fecha"] = "0";
                }
            }
            else
            {
                TempData["fecha"] = "1";
            }
            return RedirectToAction("VerVacaciones");
        }

        public ActionResult GestionarVacaciones(int idUsuario, string estado)
        {
            Usuarios user = contexto.Usuarios.FirstOrDefault(x => x.IdUsuario == idUsuario);

            if (estado == "Aceptar")
            {
                user.Vacaciones = true;
            }
            else
            {
                user.FechaInicioVacaciones = null;
                user.FechaFinVacaciones = null;
            }

            contexto.SaveChanges();

            return RedirectToAction("VerVacaciones");
        }

        public ActionResult EliminarVacaciones(int idUsuario)
        {
            Usuarios usu = contexto.Usuarios.FirstOrDefault(x => x.IdUsuario == idUsuario);

            usu.FechaInicioVacaciones = null;
            usu.FechaFinVacaciones = null;
            usu.Vacaciones = false;

            contexto.SaveChanges();

            return RedirectToAction("VerVacaciones");
        }

        [HttpPost]
        public ActionResult CrearVacaciones(int idUsuario, DateTime fechaInicio, DateTime fechaFin)
        {
            Usuarios usu = contexto.Usuarios.FirstOrDefault(x => x.IdUsuario == idUsuario); 

            usu.FechaInicioVacaciones = fechaInicio;
            usu.FechaFinVacaciones= fechaFin;
            usu.Vacaciones = true;

            contexto.SaveChanges();

            return RedirectToAction("VerVacaciones");
        }

        public List<Usuarios> MisSolicitudes(int idUsuario)
        {            
            Usuarios usu = contexto.Usuarios.FirstOrDefault(x => x.IdUsuario == idUsuario);

            List<Usuarios> usuarios = contexto.Usuarios.Where(x => x.FechaFinVacaciones != null && x.FechaInicioVacaciones != null && x.Vacaciones == false && x.IdUsuario != idUsuario).ToList();
            List<Usuarios> misUsuarios = new List<Usuarios>();

            if (usu.Usuario.Equals("admin"))
            {
                foreach (var usuario in usuarios)
                {
                    misUsuarios.Add(usuario);
                }
                
            }
            else
            {
                foreach(Equipos e in usu.Equipos)
                {
                    foreach(var users in usuarios)
                    {
                        if (e.Usuarios.Contains(users))
                        {
                            misUsuarios.Add(users);
                        }
                    }
                }
            }

            return misUsuarios;
        }
    }
}