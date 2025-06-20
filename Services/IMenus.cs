using Facelift_App.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Facelift_App.Services
{
    public interface IMenus
    {
        Task<IEnumerable<MsMainMenu>> GetAllAsync();
        IEnumerable<MsMainMenu> GetByUsername(string Username, string type);
        Task<MsMenu> GetMenuByPathAsync(string path);
    }
}
