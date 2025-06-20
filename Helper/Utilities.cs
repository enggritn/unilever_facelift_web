using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace Facelift_App.Helper
{
    public class Utilities
    {
        public static string RenderViewToString(ControllerContext context, String viewPath, object model = null)
        {
            context.Controller.ViewData.Model = model;
            using (var sw = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindView(context, viewPath, null);
                var viewContext = new ViewContext(context, viewResult.View, context.Controller.ViewData, context.Controller.TempData, sw);
                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(context, viewResult.View);
                return sw.GetStringBuilder().ToString();
            }
        }

        public static string CreateGuid(string prefix)
        {
            return string.Format("{0}-{1:N}", prefix, Guid.NewGuid());
        }

        public static string EncodeTo64(string value)
        {
            Encoding encodingUTF8 = Encoding.UTF8;
            byte[] encData_byte = new byte[value.Length];
            encData_byte = encodingUTF8.GetBytes(value);
            string returnValue = Convert.ToBase64String(encData_byte);
            return returnValue;
        }

        public static string DecodeFrom64(string value)
        {
            Encoding encodingUTF8 = Encoding.UTF8;
            return encodingUTF8.GetString(Convert.FromBase64String(value));
        }

        public static string UpperFirstCase(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            string val = "";
            string[] words = value.Split(' ');

            foreach (string word in words)
            {
                val += char.ToUpper(word[0]) + word.Substring(1).ToLower() + ' ';
            }

            return val.Trim();
        }

        public static string ToUpper(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            return value.ToUpper().Trim();
        }

        public static string ToLower(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            return value.ToLower().Trim();
        }

        public static string CallOut(string type, string message)
        {
            string callout = string.Format("<div class=\"alert alert-{0}\" role=\"alert\">", type);
            callout += message;
            callout += "</div>";

            return callout;
        }

        public static string NullDateTimeToString(DateTime? date)
        {
            return date.HasValue ? date.Value.ToString("dd MMM yyyy HH:mm:ss") : "-";
        }

        public static string NullDateToString(DateTime? date)
        {
            return date.HasValue ? date.Value.ToString("dd MMM yyyy") : "-";
        }

        public static string ConvertMonthToRoman(int month)
        {
            string[] romans = { "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX", "X", "XI", "XII"};
            string roman = "";
            try
            {
                roman = romans[month - 1];
            }
            catch (Exception)
            {

            }
            return roman;
        }

        public static string FormatThousand(int value)
        {
            return string.Format("{0:#,0}", value);
        }

        public static string FormatDecimalToThousand(decimal? value)
        {
            return string.Format("{0:n}", value);
        }

        public static string FormatDoubleToThousand(double? value)
        {
            return string.Format("{0:#,0.####}", value);
        }

        public static string IsActiveStatusBadge(bool state)
        {
            string badge = "";
            switch (state)
            {
                case true:
                    badge = "<span class=\"badge badge-primary\">Active</span>";
                    break;
                case false:
                    badge = "<span class=\"badge badge-danger\">In Active</span>";
                    break;
            }

            return badge;
        }

        public static string TransactionStatusBadge(string status)
        {
            Constant.TransactionStatus tStatus = (Constant.TransactionStatus)Enum.Parse(typeof(Constant.TransactionStatus), status, true);
            string badge = "";
            switch (tStatus)
            {
                case Constant.TransactionStatus.OPEN:
                    badge = "<span class=\"badge badge-warning\">" + Constant.TransactionStatus.OPEN + "</span>";
                    break;
                case Constant.TransactionStatus.PROGRESS:
                    badge = "<span class=\"badge badge-info\">" + Constant.TransactionStatus.PROGRESS + "</span>";
                    break;
                case Constant.TransactionStatus.CLOSED:
                    badge = "<span class=\"badge badge-success\">" + Constant.TransactionStatus.CLOSED + "</span>";
                    break;
            }

            return badge;
        }

        public static string ShipmentStatusBadge(string status)
        {
            Constant.ShipmentStatus tStatus = (Constant.ShipmentStatus)Enum.Parse(typeof(Constant.ShipmentStatus), status, true);
            string badge = "";
            switch (tStatus)
            {
                case Constant.ShipmentStatus.LOADING:
                    badge = "<span class=\"badge badge-primary\">" + Constant.ShipmentStatus.LOADING + "</span>";
                    break;
                case Constant.ShipmentStatus.DISPATCH:
                    badge = "<span class=\"badge badge-warning\">" + "IN-TRANSIT" + "</span>";
                    break;
                case Constant.ShipmentStatus.RECEIVE:
                    badge = "<span class=\"badge badge-success\">" + Constant.ShipmentStatus.RECEIVE + "</span>";
                    break;
            }

            return badge;
        }

        public static string MovementStatusBadge(string status)
        {
            Constant.PalletMovementStatus tStatus = (Constant.PalletMovementStatus)Enum.Parse(typeof(Constant.PalletMovementStatus), status, true);
            string badge = "";
            switch (tStatus)
            {
                case Constant.PalletMovementStatus.OP:
                    badge = "<span class=\"badge badge-warning\">" + Constant.PalletMovementStatus.OP + "</span>";
                    break;
                case Constant.PalletMovementStatus.OT:
                    badge = "<span class=\"badge badge-info\">" + Constant.PalletMovementStatus.OT + "</span>";
                    break;
                case Constant.PalletMovementStatus.IN:
                    badge = "<span class=\"badge badge-success\">" + Constant.PalletMovementStatus.IN + "</span>";
                    break;
                case Constant.PalletMovementStatus.ST:
                    badge = "<span class=\"badge badge-primary\">" + Constant.PalletMovementStatus.ST + "</span>";
                    break;
            }

            return badge;
        }

        public static string InsertMethodBadge(string method)
        {
            Constant.InsertMethod mtd = (Constant.InsertMethod)Enum.Parse(typeof(Constant.InsertMethod), method, true);
            string badge = "";
            switch (mtd)
            {
                case Constant.InsertMethod.SCAN:
                    badge = "<span class=\"badge badge-primary\">" + Constant.InsertMethod.SCAN + "</span>";
                    break;
                case Constant.InsertMethod.MANUAL:
                    badge = "<span class=\"badge badge-secondary\">" + Constant.InsertMethod.MANUAL + "</span>";
                    break;
            }

            return badge;
        }


        public static string RegistrationItemStatusBadge(string status)
        {
            Constant.RegistrationItemStatus itemStatus = (Constant.RegistrationItemStatus)Enum.Parse(typeof(Constant.RegistrationItemStatus), status, true);
            string badge = "";
            switch (itemStatus)
            {
                case Constant.RegistrationItemStatus.NEW:
                    badge = "<span class=\"badge badge-info\">" + Constant.RegistrationItemStatus.NEW + "</span>";
                    break;
                case Constant.RegistrationItemStatus.REGISTERED:
                    badge = "<span class=\"badge badge-warning\">" + Constant.RegistrationItemStatus.REGISTERED + "</span>";
                    break;
            }

            return badge;
        }


        public static string PalletConditionBadge(string condition)
        {
            Constant.PalletCondition tCondition = (Constant.PalletCondition)Enum.Parse(typeof(Constant.PalletCondition), condition, true);
            string badge = "";
            switch (tCondition)
            {
                case Constant.PalletCondition.GOOD:
                    badge = "<span class=\"badge badge-success\">" + Constant.PalletCondition.GOOD + "</span>";
                    break;
                case Constant.PalletCondition.DAMAGE:
                    badge = "<span class=\"badge badge-danger\">" + Constant.PalletCondition.DAMAGE + "</span>";
                    break;
                case Constant.PalletCondition.LOSS:
                    badge = "<span class=\"badge badge-dark\">" + Constant.PalletCondition.LOSS + "</span>";
                    break;
                case Constant.PalletCondition.FOUND:
                    badge = "<span class=\"badge badge-info\">" + Constant.PalletCondition.FOUND + "</span>";
                    break;
            }

            return badge;
        }

        public static string ConvertTag(string tag)
        {
            return Regex.Replace(tag, @"\s", "");
        }

        public static string ColorBadge(string value, string status)
        {
            string badge = "";
            switch (status)
            {
                case "success":
                    badge = "<span class=\"badge badge-success\">" + value + "</span>";
                    break;
                case "danger":
                    badge = "<span class=\"badge badge-danger\">" + value + "</span>";
                    break;
                case "warning":
                    badge = "<span class=\"badge badge-warning\">" + value + "</span>";
                    break;
                case "info":
                    badge = "<span class=\"badge badge-info\">" + value + "</span>";
                    break;

            }

            return badge;
        }

        public static string PalletMovementStatusBadge(string status)
        {
            Constant.PalletMovementStatus tStatus = (Constant.PalletMovementStatus)Enum.Parse(typeof(Constant.PalletMovementStatus), status, true);
            string badge = "";
            switch (tStatus)
            {
                case Constant.PalletMovementStatus.OP:
                    badge = "<span class=\"badge badge-info\">ON-PROGRESS</span>";
                    break;
                case Constant.PalletMovementStatus.OT:
                    badge = "<span class=\"badge badge-warning\">TRANSIT (OUT)</span>";
                    break;
                case Constant.PalletMovementStatus.IN:
                    badge = "<span class=\"badge badge-success\">TRANSIT (IN)</span>";
                    break;
                case Constant.PalletMovementStatus.ST:
                    badge = "<span class=\"badge badge-primary\">SETTLED</span>";
                    break;
            }

            return badge;
        }

        public static string RentStatusBadge(string status)
        {
            string badge = "";
            switch (status)
            {
                case "OnHand":
                    badge = "<span class=\"badge badge-primary\">IN</span>";
                    break;
                case "NotOnHand":
                    badge = "<span class=\"badge badge-warning\">OUT</span>";
                    break;
            }

            return badge;
        }


        public static string TransactionTypeBadge(string type)
        {
            string badge = "";
            switch (type)
            {
                case "Registration":
                    badge = "<span class=\"badge badge-info\">Registration</span>";
                    break;
                case "Outbound":
                    badge = "<span class=\"badge badge-danger\">Outbound</span>";
                    break;
                case "Inbound":
                    badge = "<span class=\"badge badge-primary\">Inbound</span>";
                    break;
            }

            return badge;
        }

        public static string InvoiceStatusBadge(bool status)
        {
            string badge = "<span class=\"badge badge-warning\">NOT GENERATED</span>";
            if (status)
            {
                badge = "<span class=\"badge badge-success\">GENERATED</span>";
            }

            return badge;
        }
    }
}