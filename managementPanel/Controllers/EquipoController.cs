using managementPanel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.Mvc;
using System.Threading;
using PagedList;
using managementPanel.Controllers;

namespace managementPanel.Controllers
{
    public class EquipoController : Controller
    {
        DB_ManagementEntities contexto = new DB_ManagementEntities();

        /*
         * 
         * Método que controla si un nombre de equipo está repetido o no
         * en la base de datos.
         * 
         */
        public JsonResult CheckUnique(string value)
        {
            bool isUnique = !contexto.Equipos.Any(x => x.Nombre == value);
            return Json(isUnique, JsonRequestBehavior.AllowGet);
        }


        /*
         * 
         * Método que lista todos los equipos que hay en la base de datos.
         * Retorna los datos a la vista ListarEquipos.
         * 
         */
        [NoDirectAccess]
        public ActionResult ListarEquipos(int? page, int? pageS)
        {
            int pageNumber = page ?? 1;
            int pageSize = pageS ?? 10;

            ViewBag.pageSize = pageSize;

            if (((Usuarios)Session["usuario"]).Usuario.Equals("admin"))
            {

                List<Equipos> equipos = contexto.Equipos.ToList();

                var equiposList = equipos.ToPagedList(pageNumber, pageSize);

                if (equiposList.Count == 0)
                {
                    while (equiposList.Count == 0 && pageNumber > 1)
                    {
                        pageNumber--;
                        equiposList = equipos.ToPagedList(pageNumber, pageSize);
                    }
                }
                ViewBag.pageNumber = pageNumber;

                return View(equiposList);
            }
            else
            {
                string nomUsu = ((Usuarios)Session["usuario"]).Usuario;
                Usuarios usuTemp = contexto.Usuarios.FirstOrDefault(x => x.Usuario == nomUsu);

                var equiposList = usuTemp.Equipos.ToPagedList(pageNumber, pageSize);

                if (equiposList.Count == 0)
                {
                    while (equiposList.Count == 0 && pageNumber > 1)
                    {
                        pageNumber--;
                        equiposList = usuTemp.Equipos.ToPagedList(pageNumber, pageSize);
                    }
                }
                ViewBag.pageNumber = pageNumber;

                return View(equiposList);
            }
        }

        [NoDirectAccess]
        public ActionResult InsertarEquipos()
        {
            List<Equipos> data = contexto.Equipos.ToList();

            return View(data);
        }

        /*
         * 
         * Método que recibe inserta en la base de datos los datos recibidos en el formulario
         * de InsertarEquipo. 
         * 
         */

        [HttpPost]
        [NoDirectAccess]
        public ActionResult InsertarEquipos(Equipos equ)
        {
            var equipoExistente = contexto.Equipos.FirstOrDefault(e => e.Nombre == equ.Nombre);

            equ.Nombre = equ.Nombre.Substring(0, 1).ToUpper() + equ.Nombre.Substring(1);

            if (equipoExistente != null)
            {
                return View(equ);
            }

            contexto.Equipos.Add(equ);
            contexto.SaveChanges();

            string nomUsu = ((Usuarios)Session["usuario"]).Usuario;
            Usuarios UsuarioCreadorEquipo = contexto.Usuarios.FirstOrDefault(e => e.Usuario.Equals(nomUsu));

            if (!UsuarioCreadorEquipo.Usuario.Equals("admin"))
            {
                UsuarioCreadorEquipo.Equipos.Add(equ);
                contexto.SaveChanges();
            }


            return RedirectToAction("ListarEquipos");
        }

        /*
         * 
         * Método que elimina un equipo seleccionado de la vista
         * ListarEquipos.
         * 
         */
        [NoDirectAccess]
        public ActionResult EliminarEquipos(string equipoSesion, string borrarEquipo, int? page, int? pageS)
        {
            TempData["EliminarEquipos"] = "";

            if (borrarEquipo == "Borrar")
            {
                var equiposUsuarios = contexto.Usuarios.SelectMany(u => u.Equipos.Select(e => e.Nombre));
                bool hayEquiposAsociados = equiposUsuarios.Contains(equipoSesion);

                if (hayEquiposAsociados)
                {
                    TempData["EliminarEquipos"] = "true";
                }
                else
                {
                    contexto.Equipos.Remove(contexto.Equipos.FirstOrDefault(x => x.Nombre == equipoSesion));
                    contexto.SaveChanges();
                    TempData["EliminarEquipos"] = "false";
                }

                List<Equipos> equipos = new List<Equipos>();

                string nomUsu = ((Usuarios)Session["usuario"]).Usuario;

                if (nomUsu.Equals("admin"))
                {
                    equipos = contexto.Equipos.ToList();
                }
                else
                {
                    Usuarios UsuarioEliminadorEquipo = contexto.Usuarios.FirstOrDefault(e => e.Usuario.Equals(nomUsu));
                    equipos = UsuarioEliminadorEquipo.Equipos.ToList();
                }
            }

            Thread.Sleep(2000);

            return RedirectToAction("ListarEquipos", new { page, pageS });
        }

        /*
         * 
         * Método que carga todos los datos de un equipo a editar en la vista EditarE.
         * 
         */
        [NoDirectAccess]
        public ActionResult EditarEquipos(int equipoSesion, int? page)
        {
            Equipos editar = contexto.Equipos.FirstOrDefault(x => x.IdEquipo == equipoSesion);

            object[] model = new object[2];

            model[0] = editar;
            model[1] = page;

            return View("EditarE", model);
        }

        /*
         * 
         * Método que recibe los datos introducidos en el formulario de editar equipos,
         * y edita el equipo seleccionado.
         * 
         */
        [HttpPost]
        [NoDirectAccess]
        public ActionResult EditarEquipos(Equipos equipo, int? page)
        {
            var equipoExistente = contexto.Equipos.FirstOrDefault(e => e.Nombre == equipo.Nombre && e.IdEquipo != equipo.IdEquipo);

            if (equipoExistente != null)
            {
                return RedirectToAction("ListarEquipos");
            }

            Equipos temp = contexto.Equipos.FirstOrDefault(x => x.IdEquipo == equipo.IdEquipo);
            temp.IdEquipo = equipo.IdEquipo;
            temp.Nombre = equipo.Nombre;
            temp.Departamento = equipo.Departamento;
            temp.Descripcion = equipo.Descripcion;
            contexto.SaveChanges();

            string nomUsu = ((Usuarios)Session["usuario"]).Usuario;


            List<Equipos> data = contexto.Equipos.ToList();

            return RedirectToAction("ListarEquipos", new { page = page });
        }
    }
}