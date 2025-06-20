using Facelift_App.Models;
using Facelift_App.Services;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Z.EntityFramework.Plus;

namespace Facelift_App.Repositories
{
    public class MenuRepository : IMenus
    {
        FaceliftEntities db = new FaceliftEntities();
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public async Task<IEnumerable<MsMainMenu>> GetAllAsync()
        {
            IEnumerable<MsMainMenu> list = null;
            try
            {
                IQueryable<MsMainMenu> query = db.MsMainMenus
                   .IncludeFilter(m => m.MsMenus.Where(i => i.IsActive == true).OrderBy(i => i.MenuIndex))
                   .Where(m => m.IsActive == true)
                   .OrderBy(m => m.MenuIndex);

                list = await query.ToListAsync();
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return list;
        }

        public IEnumerable<MsMainMenu> GetByUsername(string Username, string type)
        {
            IEnumerable<MsMainMenu> list = null;
            try
            {
                MsUser msUser = db.MsUsers.Find(Username);
                if (msUser.MsRole.IsActive)
                {
                    int[] MenuIds = db.MsRolePermissions.Where(m => m.RoleId.Equals(msUser.RoleId)).Select(x => x.MenuId).ToArray();
                    IQueryable<MsMainMenu> query = db.MsMainMenus
                        .IncludeFilter(m => m.MsMenus
                                        .Where(x => x.IsActive == true && MenuIds.Contains(x.MenuId))
                                        .OrderBy(i => i.MsMainMenu.MenuIndex)
                                        .ThenBy(i => i.MenuIndex))
                        .Where(m => m.IsActive == true && m.MenuType.Equals(type))
                        .OrderBy(m => m.MenuIndex);

                    list = query.ToList();
                }
               
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return list;
        }

        public async Task<MsMenu> GetMenuByPathAsync(string path)
        {
            MsMenu data = null;
            try
            {
                data = await db.MsMenus.Where(x => x.MenuPath.Equals(path)).FirstOrDefaultAsync();
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return data;
        }
    }
}