using managementPanel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web.Mvc;

namespace managementPanel.Controllers
{
    public class LoginController : Controller
    {
        DB_ManagementEntities contexto = new DB_ManagementEntities();
        public ActionResult Index()
        {
            List<Proyectos> misProyectos = new List<Proyectos>();
            List<Usuarios> misUsuarios = new List<Usuarios>();
            var fechasOrdenadas = new List<Tuple<DateTime?, string, string, string>>();

            if (Session["usuario"] != null)
            {
                int usuId = ((Usuarios)Session["usuario"]).IdUsuario;
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
                    List<Tuple<DateTime?, string, string, string>> fechas = new List<Tuple<DateTime?, string, string, string>>();
                    foreach (Proyectos proyecto in misProyectos)
                    {
                        fechas.Add(new Tuple<DateTime?, string, string, string>(proyecto.FechaInicio, proyecto.JefeProyecto, proyecto.Nombre, "Inicio"));
                        fechas.Add(new Tuple<DateTime?, string, string, string>(proyecto.FechaDisenio, proyecto.JefeProyecto, proyecto.Nombre, "Diseño"));
                        fechas.Add(new Tuple<DateTime?, string, string, string>(proyecto.FechaValidacion, proyecto.JefeProyecto, proyecto.Nombre, "Validacion"));
                        fechas.Add(new Tuple<DateTime?, string, string, string>(proyecto.FechaEnVivo, proyecto.JefeProyecto, proyecto.Nombre, "En Vivo"));
                        fechas.Add(new Tuple<DateTime?, string, string, string>(proyecto.FechaRecepcion, proyecto.JefeProyecto, proyecto.Nombre, "Recepcion"));
                        fechas.Add(new Tuple<DateTime?, string, string, string>(proyecto.FechaFin, proyecto.JefeProyecto, proyecto.Nombre, "Fin"));
                    }
                    fechasOrdenadas = fechas.OrderBy(r => r.Item1).ToList();
                }

                fechasOrdenadas.RemoveAll(p => p.Item1 == null || p.Item1 < DateTime.Now);

                if (usu.Consultor==true)
                {
                    fechasOrdenadas.RemoveAll(p => p.Item4.Equals("En Vivo") || p.Item4.Equals("Recepcion") || p.Item4.Equals("Fin"));
                }
            }
            return View("Principal", fechasOrdenadas);
        }

        public ActionResult IniciarSesion()
        {
            return View();
        }

        public void Instalacion()
        {
            if (!contexto.Usuarios.Any())
            {
                Usuarios admin = new Usuarios();

                admin.Nombre = "admin";
                admin.Apellidos = "admin";
                admin.Usuario = "admin";
                admin.Contrasenna = Encriptar("admin");
                admin.Admin = true;
                admin.FechaInicioVacaciones = null;
                admin.FechaFinVacaciones = null;
                admin.Vacaciones = false;

                contexto.Usuarios.Add(admin);

                contexto.SaveChanges();
            }
        }



        [HttpPost]
        public ActionResult IniciarSesion(String Usuario, String Contrasenna)
        {
            Instalacion();

            Usuarios usuTemp = contexto.Usuarios.FirstOrDefault(x => x.Usuario == Usuario);

            if (usuTemp != null)
            {
                usuTemp.Contrasenna = Desencriptar(usuTemp.Contrasenna);

                if (usuTemp.Contrasenna == Contrasenna)
                {
                    TempData["usuario"] = "1";
                    ViewBag.Nombre = usuTemp.Nombre;
                    //Por seguridad
                    usuTemp.Contrasenna = null;
                    Session.Add("usuario", usuTemp);
                    return RedirectToAction("Index");
                }
                else
                {
                    //Aquí el usuario se equivoca al meter la contraseña
                    TempData["msjInicio"] = "noCont";
                    ViewBag.msjInicio = TempData["msjInicio"];
                    return View("IniciarSesion");
                }
            }
            //Aquí el usuario no exite
            TempData["msjInicio"] = "noUsu";
            ViewBag.msjInicio = TempData["msjInicio"];
            return View("IniciarSesion");
        }

        public JsonResult CheckUnique(string value)
        {
            bool isUnique = !contexto.Usuarios.Any(x => x.Usuario == value);
            return Json(isUnique, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Registrar()
        {
            //Para cargar el select con los nombres de equipos disponibles
            List<Equipos> dataEquipos = contexto.Equipos.ToList();

            //Creación de nuevo usuario
            Usuarios usuario = new Usuarios();

            object[] modelos = new object[2];
            modelos[0] = dataEquipos;
            modelos[1] = usuario;


            return View(modelos);
        }

        [HttpPost]
        [NoDirectAccess]
        public ActionResult Registrar(Usuarios usuario, List<int> equiposId, String rol)
        {
            if (usuario.Usuario == null)
            {
                return RedirectToAction("Registrar", "Login");
            }
            else
            {
                usuario.Nombre = usuario.Nombre.Substring(0, 1).ToUpper() + usuario.Nombre.Substring(1);

                usuario.Contrasenna = Encriptar(usuario.Contrasenna);

                usuario.FechaInicioVacaciones = null;
                usuario.FechaFinVacaciones = null;
                usuario.Vacaciones = false;

                if (equiposId != null)
                {
                    foreach (int i in equiposId)
                    {
                        Equipos e = contexto.Equipos.Find(i);
                        usuario.Equipos.Add(e);
                    }
                }

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
                    usuario.Consultor = null;
                }
                else if (usuario.Consultor != null)
                {
                    usuario.Admin = null;
                }

                contexto.Usuarios.Add(usuario);
                contexto.SaveChanges();

                //Comprobamos que es un cliente nuevo, no un admin registrando 
                if (Session["usuario"] == null)
                {
                    Session["usuario"] = usuario;
                }

                TempData["msj"] = null;

                //Paramos el hilo de ejecución para que se vea bn el toast
                Thread.Sleep(1500);

                if (((Usuarios)Session["usuario"]).Usuario.Equals("admin"))
                {
                    return RedirectToAction("ListarUsuarios", "Usuario");
                }
                return RedirectToAction("Index", "Login");
            }
        }
        [NoDirectAccess]
        public ActionResult CerrarSesion()
        {
            Session["usuario"] = null;
            return View("Principal");
        }

        public string Encriptar(string mensaje)
        {
            string result = string.Empty;

            byte[] encrypted = System.Text.Encoding.Unicode.GetBytes(mensaje);

            result = Convert.ToBase64String(encrypted);

            return result;
        }

        public string Desencriptar(string contrasenna)
        {
            string result = string.Empty;

            byte[] decrypted = Convert.FromBase64String(contrasenna);

            result = System.Text.Encoding.Unicode.GetString(decrypted);

            return result;

        }

        [HttpPost]
        public JsonResult devolverCont(string nomUsuario)
        {
            Usuarios usu = contexto.Usuarios.FirstOrDefault(x => x.Usuario.Equals(nomUsuario));
            String cont = Desencriptar(usu.Contrasenna);
            return Json(cont, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ControlUrl()
        {
            if (Session["usuario"] != null)
            {
                return RedirectToAction("Index");
            }
            else
            {
                return RedirectToAction("IniciarSesion", "Login");
            }
        }

        public ActionResult CodigoError(int codigo)
        {
            return View("Errores", codigo);

        }
    }
}