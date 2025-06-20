using Facelift_App.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Facelift_App.Helper
{
    public class AuthCheck : ActionFilterAttribute, IActionFilter
    {
        void IActionFilter.OnActionExecuting(ActionExecutingContext filterContext)
        {
            HttpSessionStateBase session = filterContext.HttpContext.Session;
            string controllerName = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
            string username = session["username"].ToString();
            
            using (FaceliftEntities db = new FaceliftEntities())
            {
                MsUser msUser = db.MsUsers.Find(username);
                //check if user inactive, destroy session.
                if(msUser != null && msUser.IsActive)
                {
                    session.Add("full_name", msUser.FullName);
                    MsMenu msMenu = db.MsMenus.Where(z => z.MenuPath.Equals(controllerName)).FirstOrDefault();

                    //check if controller exist in role permission
                    if (msUser.MsRole.IsActive && msMenu.IsActive && msUser.MsRole.MsRolePermissions.Any() && msUser.MsRole.MsRolePermissions.Where(z => z.MenuId.Equals(msMenu.MenuId)).FirstOrDefault() != null)
                    {
                        //update
                        msUser.LastVisitUrl = controllerName;
                        msUser.LastVisitAt = DateTime.Now;
                        db.SaveChanges();

                        filterContext.Controller.ViewBag.MenuParent = msMenu.MsMainMenu.MenuName;
                        filterContext.Controller.ViewBag.Title = msMenu.MenuTitle;
                        filterContext.Controller.ViewBag.MenuIcon = msMenu.MenuIcon;
                    }
                    else
                    {
                        filterContext.Result = new RedirectToRouteResult(
                       new RouteValueDictionary {
                                { "Controller", "AccessDenied" },
                                { "Action", "Index" }
                                   });
                    }
                }
                else
                {
                    filterContext.Result = new RedirectToRouteResult(
                       new RouteValueDictionary {
                                { "Controller", "Login" },
                                { "Action", "Exit" }
                                   });
                }

               
                OnActionExecuting(filterContext);
            }
           
        }
    }
}