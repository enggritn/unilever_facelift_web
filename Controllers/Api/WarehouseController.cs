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
    public class WarehouseController : ApiController
    {
        private readonly IWarehouses IWarehouses;
        private readonly IUsers IUsers;

        public WarehouseController(IWarehouses Warehouses, IUsers Users)
        {
            IWarehouses = Warehouses;
            IUsers = Users;
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
                    if(user != null)
                    {
                        IEnumerable<MsWarehouse> list = IWarehouses.GetByUsername(username);
                        IEnumerable<WarehouseDTO> warehouses = from x in list
                                                               select new WarehouseDTO
                                                               {
                                                                   WarehouseId = x.WarehouseId,
                                                                   WarehouseCode = x.WarehouseCode,
                                                                   WarehouseName = x.WarehouseName
                                                               };
                        obj.Add("warehouses", warehouses);
                        status = true;
                        message = "Retrieve warehouse succeeded.";
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

        // GET api/values
        public async Task<IHttpActionResult> GetByWarehouseName(string WarehouseName)
        {
            Dictionary<string, object> obj = new Dictionary<string, object>();
            string message = "";
            bool status = false;
            var re = Request;
            var headers = re.Headers;

            try
            {
                MsWarehouse warehouse = await IWarehouses.GetDataByWarehouseNameAsync(WarehouseName);
                if(warehouse == null)
                {
                    throw new Exception("Warehouse not recognized, please contact system administrator.");
                }

                status = true;
                message = "warehouse found.";
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
