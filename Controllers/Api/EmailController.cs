using Facelift_App.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace Facelift_App.Controllers.Api
{
    public class EmailController : ApiController
    {
        public async Task<IHttpActionResult> Post()
        {
            Dictionary<string, object> obj = new Dictionary<string, object>();
            string message = "Test email.";
            bool status = false;
            try
            {
                List<string> rec = new List<string>();
                rec.Add("tss1@sgp-dkp.com");
                rec.Add("muhammadbhovdair@gmail.com");
                status = true;
                Mailing mailing = new Mailing();
                mailing.SendEmail(rec, "Facelift - Test Email", "test");
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
