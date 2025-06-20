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
    public class ManualController : ApiController
    {
        private readonly IUsers IUsers;
        private readonly IMenus IMenus;
        private readonly IAccidents IAccidents;
        private readonly IWarehouses IWarehouses;
        private readonly IPallets IPallets;

        public ManualController(IUsers Users, IMenus Menus, IAccidents Accidents, IWarehouses Warehouses, IPallets Pallets)
        {
            IUsers = Users;
            IMenus = Menus;
            IAccidents = Accidents;
            IWarehouses = Warehouses;
            IPallets = Pallets;
        }       

        public async Task<IHttpActionResult> Post(Manual manual)
        {
            Dictionary<string, object> obj = new Dictionary<string, object>();
            string message = "Invalid form submission.";
            bool status = false;
            var re = Request;
            var headers = re.Headers;
            try
            {
                if (ModelState.IsValid)
                {
                    //get user access
                    if (headers.Contains(Constant.facelift_token_name))
                    {
                        string token = headers.GetValues(Constant.facelift_token_name).First();
                        string username = Encryptor.Decrypt(Utilities.DecodeFrom64(token), Constant.facelift_token_key);
                        MsUser user = await IUsers.GetDataByIdAsync(username);
                        if (user != null)
                        {
                            status = await IPallets.UpdateManualAsync(manual.TagId, manual.WarehouseName, manual.PalletCondition);
                            if (status)
                            {
                                message = "Update item successfuly.";
                            }
                            else
                            {
                                message = "Update item failed. Please contact system administrator.";
                            }
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
