using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AutoInputViewsDemo.Areas.Nasa.Controllers
{
    public class NeoController : Controller
    {
        // GET: Nasa/Neo
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Item(int? id)
        {
            ViewBag.ItemId = id;
            return View();
        }
    }
}