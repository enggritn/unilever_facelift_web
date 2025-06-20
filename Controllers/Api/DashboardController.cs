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
    public class DashboardController : ApiController
    {
        private readonly IUsers IUsers;
        private readonly IPallets IPallets;
        private readonly IWarehouses IWarehouses;
        private readonly IDashboards IDashboards;

        public DashboardController(IUsers Users, IPallets Pallets, IWarehouses Warehouses, IDashboards Dashboards)
        {
            IUsers = Users;
            IPallets = Pallets;
            IWarehouses = Warehouses;
            IDashboards = Dashboards;
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
                if (headers.Contains(Constant.facelift_token_name) && headers.Contains("warehouseId"))
                {
                    string token = headers.GetValues(Constant.facelift_token_name).First();
                    string warehouseId = headers.GetValues("warehouseId").First();
                    string username = Encryptor.Decrypt(Utilities.DecodeFrom64(token), Constant.facelift_token_key);
                    MsUser user = await IUsers.GetDataByIdAsync(username);
                    MsWarehouse warehouse = await IWarehouses.GetDataByIdAsync(warehouseId);
                    if (user != null && warehouse != null)
                    {
                        Dashboard dashboard = new Dashboard
                        {
                            totalRegistration = IDashboards.TotalRegistration(warehouseId),
                            totalOutbound = IDashboards.TotalOutbound(warehouseId),
                            totalInbound = IDashboards.TotalInbound(warehouseId),
                            totalInspection = IDashboards.TotalInspection(warehouseId),
                            totalCycleCount = IDashboards.TotalCycleCount(warehouseId),
                            totalRecall = IDashboards.TotalRecall(warehouseId)
                        };
                        obj.Add("dashboard", dashboard);
                        status = true;
                        message = "Retrieve dashboard succeeded.";
                    }
                    else
                    {
                        return BadRequest();
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
