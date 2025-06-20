using Facelift_App.Helper;
using Facelift_App.Models.Api;
using Facelift_App.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace Facelift_App.Controllers.Api
{
    public class AccessController : ApiController
    {

        private readonly IUsers IUsers;
        private readonly IMenus IMenus;

        public AccessController(IUsers Users, IMenus Menus)
        {
            IUsers = Users;
            IMenus = Menus;
        }

        // GET api/values
        public async Task<IHttpActionResult> Get()
        {
            Dictionary<string, object> obj = new Dictionary<string, object>();
            string message = "";
            bool status = false;
            var re = Request;
            var headers = re.Headers;

            try
            {
                //get user access
                if (headers.Contains(Constant.facelift_token_name))
                {
                    string token = headers.GetValues(Constant.facelift_token_name).First();
                    string username = Encryptor.Decrypt(Utilities.DecodeFrom64(token), Constant.facelift_token_key);
                    MsUser user = await IUsers.GetDataByIdAsync(username);
                    if (user != null)
                    {
                        IEnumerable<MsMainMenu> list = IMenus.GetByUsername(username, "Mobile");
                        IEnumerable<MenuDTO> menus = from x in list
                                                               select new MenuDTO
                                                               {
                                                                   MenuId = x.MenuId.ToString(),
                                                                   MenuName = x.MenuName,
                                                                   MenuTitle = x.MenuName,
                                                                   menus = from z in x.MsMenus
                                                                           select new MenuDTO
                                                                           {
                                                                               MenuId = z.MenuId.ToString(),
                                                                               MenuName = z.MenuName,
                                                                               MenuTitle = z.MenuTitle
                                                                           }
                                                               };
                        obj.Add("menus", menus);
                        status = true;
                        message = "Retrieve menu succeeded.";
                    }
                    else
                    {
                        return Unauthorized();
                    }

                }
                else
                {
                    return Unauthorized();
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
