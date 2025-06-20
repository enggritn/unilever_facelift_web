using Facelift_App.Helper;
using Facelift_App.Models;
using Facelift_App.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace Facelift_App.Controllers.Api
{
    public class LoginController : ApiController
    {
        private readonly IUsers IUsers;

        public LoginController(IUsers Users)
        {
            IUsers = Users;
        }

        // POST api/values
        public async Task<IHttpActionResult> Post(LoginVM loginVM)
        {
            Dictionary<string, object> obj = new Dictionary<string, object>();
            string message = "Invalid username or password.";
            bool status = false;
            try
            {
                if (ModelState.IsValid)
                {
                    MsUser user = await IUsers.GetDataByIdAsync(loginVM.Username);
                    if (user != null)
                    {
                        if (user.IsActive)
                        {
                            if (Encryptor.ValidatePassword(loginVM.Password, user.UserPassword))
                            {
                                await IUsers.LastLoginAsync(user);
                                status = true;
                                string token = Utilities.EncodeTo64(Encryptor.Encrypt(user.Username, Constant.facelift_token_key));
                                message = "Authentication succeeded.";
                                obj.Add("username", user.Username);
                                obj.Add("fullName", user.FullName);
                                obj.Add("token", token);
                            }
                        }
                        else
                        {
                            message = "Username already inactive.";
                        }
                    }
                }

            }
            catch (HttpRequestException reqpEx)
            {
                message = reqpEx.Message;
                return BadRequest();
            }
            catch (HttpResponseException respEx)
            {
                message = respEx.Message;
                return NotFound();
            }
            catch (Exception ex)
            {
                message = ex.Message;
                //return InternalServerError();
               
            }


            obj.Add("status", status);
            obj.Add("message", message);
            


            return Ok(obj);
        }
    }
}
