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
    public class LoginController : Controller
    {
        private readonly IUsers IUsers;

        public LoginController(IUsers Users)
        {
            IUsers = Users;
        }

        // GET: Login
        public ActionResult Index()
        {
            if(Session != null && Session["username"] != null)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            LoginVM loginVM = new LoginVM();
            if (Request.Cookies["facelift_username"] != null)
            {
                loginVM.Username = Request.Cookies["facelift_username"].Value;
                loginVM.IsRemember = true;
            }       
            
            return View(loginVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Index(LoginVM loginVM)
        {
            if (ModelState.IsValid)
            {
                //check user exist or not
                MsUser user = await IUsers.GetDataByIdAsync(loginVM.Username);
                if(user != null)
                {
                    if (user.IsActive)
                    {
                        if (Encryptor.ValidatePassword(loginVM.Password, user.UserPassword))
                        {
                            if (loginVM.IsRemember)
                            {
                                HttpCookie username = new HttpCookie("facelift_username");
                                username.Value = loginVM.Username;
                                username.Expires = DateTime.Now.AddYears(1);

                                Response.Cookies.Add(username);
                            }
                            else
                            {
                                HttpCookie usrCookie = new HttpCookie("facelift_username");
                                usrCookie.Value = null;
                                usrCookie.Expires = DateTime.Now.AddDays(-1);
                                Response.Cookies.Add(usrCookie);
                            }

                            string token = Utilities.EncodeTo64(Encryptor.Encrypt(user.Username, Constant.facelift_token_key));

                            Session.Add("username", user.Username);

                            //set warehouse access default
                            Session.Add("warehouseAccess", user.DefaultWarehouseId);
                            Session.Add("token", token);

                            await IUsers.LastLoginAsync(user);

                            return RedirectToAction("Index", "Dashboard");
                        }
                        else
                        {
                            ModelState.AddModelError("", "Invalid username or password");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Username already inactive");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Invalid username or password");
                }
            }
            return View(loginVM);
        }

        [HttpGet]
        public ActionResult Exit()
        {
            Session.Clear();
            Session.Abandon();
            //HttpCookie usrCookie = new HttpCookie("facelift_usr");
            //usrCookie.Value = null;
            //usrCookie.Expires = DateTime.Now.AddDays(-1);
            //Response.Cookies.Add(usrCookie);
            return RedirectToAction("Index", "Login");
        }

        [HttpGet]
        public ActionResult ForgotPassword()
        {
            if (Session != null && Session["username"] != null)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            //ForgotPassVM dataVM = new ForgotPassVM();

            //if (TempData.ContainsKey("Username"))
            //    dataVM.Username = TempData["Username"].ToString();

            //if (TempData.ContainsKey("UserEmail"))
            //    dataVM.UserEmail = TempData["UserEmail"].ToString();

            if (TempData.ContainsKey("TempPassword"))
                ViewBag.TempPassword = TempData["TempPassword"].ToString();

            return View();
        }

        [HttpPost]
        public async Task<ActionResult> ForgotPassword(ForgotPassVM dataVM)
        {
            if (Session != null && Session["username"] != null)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            if (ModelState.IsValid)
            {
                //check user exist or not
                MsUser user = await IUsers.GetDataByIdAsync(dataVM.Username);
                if (user != null)
                {
                    if (user.IsActive)
                    {
                        //check if email match, then generate random password
                        if (user.UserEmail.Equals(dataVM.UserEmail))
                        {
                            Random random = new Random();
                            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                            string tempPassword =  new string(Enumerable.Repeat(chars, 5)
                              .Select(s => s[random.Next(s.Length)]).ToArray());

                            //update password
                            user.UserPassword = Encryptor.HashPassword(tempPassword);

                            bool result = await IUsers.ChangePasswordAsync(user);

                            if (result)
                            {
                                TempData["Username"] = dataVM.Username;
                                TempData["UserEmail"] = dataVM.UserEmail;
                                TempData["TempPassword"] = tempPassword;
                                return RedirectToAction("ForgotPassword", "Login");
                            }
                            else
                            {
                                ModelState.AddModelError("", "Change password failed. Please contact system administrator.");
                            }

                           
                        }
                        else
                        {
                            ModelState.AddModelError("", "Invalid username or email");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Username already inactive");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Invalid username or email");
                }
            }
            return View(dataVM);

        }
    }
}