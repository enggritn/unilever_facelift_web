using Facelift_App.Helper;
using Facelift_App.Models.Api;
using Facelift_App.Services;
using NLog;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace Facelift_App.Controllers.Api
{
    public class InspectionSelectionController : ApiController
    {
        FaceliftEntities db = new FaceliftEntities();
        private static Logger logger = LogManager.GetCurrentClassLogger();

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
                    MsUser user = db.MsUsers.Where(m => m.Username.Equals(username)).FirstOrDefault();
                    if (user != null)
                    {
                        List<PICDTO> pICDTOs = new List<PICDTO>();
                        pICDTOs.Add(new PICDTO("", "Select"));
                        foreach (var item in Constant.InspectionPIC)
                        {
                            pICDTOs.Add(new PICDTO(item.Key, item.Value));
                        }

                        List<ClassificationDTO> classificationDTOs = new List<ClassificationDTO>();
                        classificationDTOs.Add(new ClassificationDTO("", "Select", ""));
                        foreach (var item in Constant.DamageClassification)
                        {
                            string imageURL = "";
                            string folderPath = "/Content/img/InspectionClassification";
                            string imagePath = folderPath + "/" + string.Format("{0}.jpg", item.Key);
                            if (File.Exists(HttpContext.Current.Server.MapPath("~" + imagePath)))
                            {
                                using (Image image = Image.FromFile(HttpContext.Current.Server.MapPath("~" + imagePath)))
                                {
                                    using (MemoryStream m = new MemoryStream())
                                    {
                                        image.Save(m, image.RawFormat);
                                        byte[] imageBytes = m.ToArray();

                                        // Convert byte[] to Base64 String
                                        imageURL = Convert.ToBase64String(imageBytes);
                                    }
                                }
                            }
                            
                            classificationDTOs.Add(new ClassificationDTO(item.Key, item.Value, imageURL));
                        }

                        obj.Add("pics", pICDTOs);
                        obj.Add("classifications", classificationDTOs);
                        status = true;
                        message = "Retrieve selection succeeded.";
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
