using Facelift_App.Services;
using System.Threading.Tasks;

namespace Facelift_App.Validators
{
    public class RoleValidator
    {

        private readonly IRoles IRoles;


        public RoleValidator(IRoles Roles)
        {
            IRoles = Roles;
        }

        public async Task<string> IsUniqueName(string RoleName, string id)
        {
            //logic explanation, if errMsg is "true" then data pass the validation
            string errMsg = "true";
            MsRole data = await IRoles.GetDataByRoleNameAsync(RoleName);
            if (data != null && !data.RoleId.Equals(id))
            {
                errMsg = "Role Name already registered.";
            }
            return errMsg;
        }

        public async Task<string> IsRoleExist(string RoleId)
        {
            //logic explanation, if errMsg is "true" then data pass the validation
            return await IRoles.GetDataByIdAsync(RoleId) != null ? "true" : "Role not recognized.";
        }
    }
}