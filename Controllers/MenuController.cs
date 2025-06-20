using Facelift_App.Helper;
using Facelift_App.Models;
using Facelift_App.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Facelift_App.Controllers
{
    [SessionCheck]
    public class MenuController : Controller
    {

        private readonly IMenus IMenus;


        public MenuController(IMenus Menus)
        {
            IMenus = Menus;
        }

        public ActionResult ListMenu(string cc)
        {
            string username = Session["username"].ToString();
            IEnumerable<MsMainMenu> listMenu = IMenus.GetByUsername(username, "Web");

            ViewBag.CurrentController = cc;

            return PartialView("ListMenu", listMenu);
        }
    }
}