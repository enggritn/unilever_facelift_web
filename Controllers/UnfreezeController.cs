using Facelift_App.Helper;
using Facelift_App.Models;
using Facelift_App.Services;
using Facelift_App.Validators;
using NLog;
using OfficeOpenXml;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Facelift_App.Controllers
{

    public class UnfreezeController : Controller
    {
        FaceliftEntities db = new FaceliftEntities();
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public ActionResult Index()
        {
            ViewBag.TempMessage = TempData["TempMessage"];
            return View();
        }


        [HttpPost]
        public async Task<JsonResult> UploadItem()
        {
            bool result = false;
            string message = "";

            if (Request.Files.Count > 0)
            {
                HttpPostedFileBase file = Request.Files[0];

                if (file != null && file.ContentLength > 0 && Path.GetExtension(file.FileName).ToLower() == ".csv")
                {
                    if (file.ContentLength < (4 * 1024 * 1024))
                    {
                        try
                        {

                            StreamReader sr = new StreamReader(file.InputStream, System.Text.Encoding.Default);
                            string results = sr.ReadToEnd();
                            sr.Close();

                            string[] row = results.Split('\n');

                            List<string> tags = new List<string>();
                            //row
                            for (int i = 1; i < row.Length; i++)
                            {
                                //loop by col index
                                if (!string.IsNullOrEmpty(row[i]))
                                {
                                    string[] col = Regex.Split(row[i], ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");
                                    string val = col[0].Replace("\"", string.Empty);
                                    string tagId = Utilities.ConvertTag(val);
                                    if (!tags.Contains(tagId))
                                    {
                                        tags.Add(tagId);
                                    }

                                }
                            }

                            foreach (string tag in tags)
                            {

                                MsPallet pallet = db.MsPallets.Where(m => m.TagId.Equals(tag)).FirstOrDefault();
                                if(pallet != null && pallet.PalletCondition.Equals("FREEZE"))
                                {
                                    pallet.PalletCondition = Constant.PalletCondition.GOOD.ToString();
                                    pallet.PalletMovementStatus = Constant.PalletMovementStatus.ST.ToString();
                                }
                            }

                            db.SaveChanges();

                            result = true;
                            message = "Unfreeze succeeded.";


                        }
                        catch (Exception)
                        {
                            message = "Upload item failed";
                        }
                    }
                    else
                    {
                        message = "Upload failed. Maximum allowed file size : 4MB ";
                    }

                }
                else
                {
                    message = "Upload item failed. File is invalid.";
                }
            }
            else
            {
                message = "No file uploaded.";
            }
            return Json(new { stat = result, msg = message });
        }



    }
}