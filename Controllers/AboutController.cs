using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Facelift_App.Controllers
{
    public class AboutController : Controller
    {

        /*
        ____ ____ ____ ____ ___ ____ ___     ___  _   _    _  _ _  _ _  _ ____ _  _ _  _ ____ ___     ___  _  _ ____ _  _ ___  ____ _ ____
        |    |__/ |___ |__|  |  |___ |  \    |__]  \_/     |\/| |  | |__| |__| |\/| |\/| |__| |  \    |__] |__| |  | |  | |  \ |__| | |__/
        |___ |  \ |___ |  |  |  |___ |__/    |__]   |      |  | |__| |  | |  | |  | |  | |  | |__/    |__] |  | |__|  \/  |__/ |  | | |  \
        
        */

        public void Index()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<style>");
            sb.Append("body {" +
                      "background-color: #000;" +
                      "}");
            sb.Append("pre code {"+
                      "border: 1px solid #fff;"+
                      "display: block;"+
                      "padding: 20px;"+
                      "color: #85ff00;" +
                      "width: 35%;" +
                      "}");
            sb.Append("</style>");

            sb.Append("<pre><code id=\"json\"></code></pre>");
            sb.Append("<script type=\"text/javascript\">");
            sb.Append("let data = {" +
                "\"developedBy\" : \"Muhammad Bhovdair - Fullstack Developer\", " +
                "\"companyName\" : \"PT. Duta Kalingga Pratama\", " +
                "\"clientName\" : \"Unilever Indonesia\", " +
                "\"projectName\" : \"Facelift (Pallet Management System) [RFID Automation]\", " +
                "\"language\" : \"C#\", " +
                "\"framework\" : \"ASP.MVC 5\"" +
                "};");
            sb.Append("document.getElementById(\"json\").innerHTML = JSON.stringify(data, undefined, 2);");
            sb.Append("</script>");

            Response.Write(sb.ToString());
        }

    }
}