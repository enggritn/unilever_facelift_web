using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Web;

namespace Facelift_App.Helper
{
    public static class Constant
    {
        public const string facelift_encryption_key = "facelift_dkp_2020";
        public static string facelift_pallet_owner = ConfigurationManager.AppSettings["ControlTowerName"].ToString();
        public const string facelift_token_name = "facelift_token";
        public const string facelift_token_key = "facelift_mobile";
        public static int facelift_token_expired = Convert.ToInt16(ConfigurationManager.AppSettings["TokenExpiry"].ToString());

        public enum DeviceType
        {
            GATE,
            HANDHELD
        }

        public enum InsertMethod
        {
            SCAN,
            MANUAL
        }

        public enum TransactionName
        {
            [Description("REGISTRATION")]
            REGISTRATION,
            [Description("SHIPMENT - OUTBOUND")]
            SHIPMENT_OUTBOUND,
            [Description("SHIPMENT - INBOUND")]
            SHIPMENT_INBOUND,
            [Description("INSPECTION")]
            INSPECTION,
            [Description("STOCK TAKE")]
            STOCK_TAKE,
            [Description("RECALL")]
            RECALL
        }

        public enum RegistrationItemStatus
        {
            NEW,
            REGISTERED
        }

        public enum PalletCondition
        {
            GOOD,
            DAMAGE,
            LOSS,
            FOUND,
            FREEZE,
            DISPOSED, //used if pallet need to removed from data
        }

        public enum PalletMovementStatus
        {
            [Description("ON PREPARATION")]
            OP,
            [Description("ON TRANSACTION")]
            OT,
            [Description("INBOUND IN-TRANSIT")]
            IN,
            [Description("SETTLED")]
            ST
        }

        public enum TransactionStatus
        {
            OPEN,
            PROGRESS,
            CLOSED
        }

        public enum ShipmentStatus
        {
            LOADING,
            DISPATCH,
            RECEIVE
        }

        public enum AccidentType
        {
            INBOUND,
            STOCK_TAKE,
            INSPECTION
        }

        public enum ReasonType
        {
            DAMAGE,
            LOSS
        }

        public static Dictionary<string, ReasonType> ReasonList = new Dictionary<string, ReasonType>
        {
            {"DAMAGE - PYSHIC", ReasonType.DAMAGE},
            {"DAMAGE - SENDER", ReasonType.DAMAGE},
            {"DAMAGE - TRANSPORTER", ReasonType.DAMAGE},
            {"DAMAGE - CHIP", ReasonType.DAMAGE},
            {"LOSS - SENDER", ReasonType.LOSS},
            {"LOSS - TRANSPORTER", ReasonType.LOSS},
            {"LOSS - STOCK TAKE", ReasonType.LOSS},
            {"LOSS - HIJACK", ReasonType.LOSS},
            {"LOSS - ACCIDENT", ReasonType.LOSS}
        };


        public static string GetDescription(this Enum value)
        {
            Type type = value.GetType();
            string name = Enum.GetName(type, value);
            if (name != null)
            {
                FieldInfo field = type.GetField(name);
                if (field != null)
                {
                    DescriptionAttribute attr =
                           Attribute.GetCustomAttribute(field,
                             typeof(DescriptionAttribute)) as DescriptionAttribute;
                    if (attr != null)
                    {
                        return attr.Description;
                    }
                }
            }
            return null;
        }

        public static IEnumerable<PalletCondition> PalletConditions()
        {
            return Enum.GetValues(typeof(PalletCondition)).Cast<PalletCondition>();
        }

        public enum AgingType
        {
            USED,
            UNUSED
        }


        public static List<string> GetReasonByAccidentType(string accidentType)
        {
            List<string> reasons = new List<string>();

            Constant.AccidentType type = (Constant.AccidentType)Enum.Parse(typeof(Constant.AccidentType), accidentType, true);
            switch (type)
            {
                case Constant.AccidentType.INBOUND:
                    reasons.Add("DAMAGE - SENDER");
                    reasons.Add("DAMAGE - TRANSPORTER");
                    reasons.Add("DAMAGE - CHIP");
                    reasons.Add("LOSS - SENDER");
                    reasons.Add("LOSS - TRANSPORTER");
                    reasons.Add("LOSS - HIJACK");
                    reasons.Add("LOSS - ACCIDENT");
                    break;
                case Constant.AccidentType.STOCK_TAKE:
                    reasons.Add("DAMAGE - PYSHIC");
                    reasons.Add("DAMAGE - CHIP");
                    reasons.Add("LOSS - STOCK TAKE");
                    break;
                case Constant.AccidentType.INSPECTION:
                    reasons.Add("DAMAGE - PYSHIC");
                    //reasons.Add("DAMAGE - CHIP");
                    break;
            }

            return reasons;
        }


        public static Dictionary<string, string> InspectionClassification = new Dictionary<string, string>
        {
            {"DMG1", "Handling Entry Leg Chipped"},
            {"DMG2", "Broken Bottom Skid Completely"},
            {"DMG3", "Skid Broken"},
            {"DMG4", "Top and Bottom Skid Crack"},
            {"DMG5", "Broken Pallet (>15%/Pallet Crack)"},
            {"DMG6", "Corner Leg Chipped (2 wall out of minimal 3 walls)" },
            {"DMG7", "RFID Chip Not Available"},
            {"LOSS1", "Loss Pyshic"},
        };

        public static Dictionary<string, string> DamageClassification = new Dictionary<string, string>
        {
            {"DMG1", "Handling Entry Leg Chipped"},
            {"DMG2", "Broken Bottom Skid Completely"},
            {"DMG3", "Skid Broken"},
            {"DMG4", "Top and Bottom Skid Crack"},
            {"DMG5", "Broken Pallet (>15%/Pallet Crack)"},
            {"DMG6", "Corner Leg Chipped (2 wall out of minimal 3 walls)" },
        };


        public static Dictionary<string, string> InspectionPIC = new Dictionary<string, string>
        {
            {"PIC1", "Operator"},
            {"PIC2", "Sender"},
            {"PIC3", "Transporter"}
        };

        public enum StatusApproval
        {
            APPROVED,
            REJECTED
        }

        public enum RoleName
        {
            Outbound,
            Inbound,
            Security,
            Sysadmin,
            Inventory,
            Approval,
            Admin,
            Finance
        }
    }
}