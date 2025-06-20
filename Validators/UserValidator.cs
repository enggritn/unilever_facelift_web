using Facelift_App.Helper;
using Facelift_App.Services;
using System.Threading.Tasks;

namespace Facelift_App.Validators
{
    public class UserValidator
    {

        private readonly IUsers IUsers;

        public UserValidator(IUsers Users)
        {
            IUsers = Users;
        }

        public async Task<string> IsUniqueUsername(string Username)
        {
            //logic explanation, if errMsg is "true" then data pass the validation
            string errMsg = await IUsers.GetDataByUsernameAsync(Username) == null ? "true" : "Username already registered.";
            return errMsg;
        }

        public async Task<string> IsUserExist(string Username)
        {
            //logic explanation, if errMsg is "true" then data pass the validation
            string errMsg = await IUsers.GetDataByUsernameAsync(Username) != null ? "true" : "User not recognized.";
            return errMsg;
        }

        public async Task<string> IsUniqueEmail(string UserEmail, string id)
        {
            //logic explanation, if errMsg is "true" then data pass the validation
            string errMsg = "true"; 
            MsUser data = await IUsers.GetDataByEmailAsync(UserEmail);
            if(data != null && !data.Username.Equals(id))
            {
                errMsg = "Email already registered.";
            }
            return errMsg;
        }

        public async Task<string> IsPasswordMatch(string Username, string OldPassword)
        {
            //select to database, check current password to compare with old password
            MsUser data = await IUsers.GetDataByIdAsync(Username);
            if(data == null)
            {
                return "Data not found.";
            }
            //logic explanation, if errMsg is "true" then data pass the validation
            string errMsg = Encryptor.ValidatePassword(OldPassword, data.UserPassword) ? "true" : "Invalid current password.";
            return errMsg;
        }
    }
}