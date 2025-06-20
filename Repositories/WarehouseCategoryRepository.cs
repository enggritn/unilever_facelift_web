using Facelift_App.Services;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Facelift_App.Repositories
{
    public class WarehouseCategoryRepository : IWarehouseCategories
    {
        FaceliftEntities db = new FaceliftEntities();
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public async Task<IEnumerable<MsWarehouseCategory>> GetAllAsync()
        {
            IEnumerable<MsWarehouseCategory> list = null;
            try
            {
                IQueryable<MsWarehouseCategory> query = db.MsWarehouseCategories
                   .OrderBy(m => m.CategoryName);

                list = await query.ToListAsync();
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return list;
        }

        public async Task<MsWarehouseCategory> GetDataByIdAsync(string id)
        {
            MsWarehouseCategory data = null;
            try
            {
                data = await db.MsWarehouseCategories.FindAsync(Convert.ToInt32(id));
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