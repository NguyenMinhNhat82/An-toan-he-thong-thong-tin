using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.SqlClient;

namespace UngDungGiaoDich.Controllers
{
    public class AppController : Controller
    {
  
        // GET: App
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult ChungKhoan()
        {
            return View();
        }
        public ActionResult Vang()
        {
            return View();
        }
        public ActionResult NhaDat()
        {
            return View();
        }
        public ActionResult Login() {
            return View();
        }
        public ActionResult SignIn() {
            return View();
        }
    }
}